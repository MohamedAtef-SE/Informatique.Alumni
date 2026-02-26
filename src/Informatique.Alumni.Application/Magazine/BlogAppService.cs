using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Users;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Magazine;

public class CommentRateLimitCacheItem
{
    public DateTime LastCommentTime { get; set; }
}

[Authorize]
public class BlogAppService : AlumniAppService, IBlogAppService
{
    private readonly IPostRepository _postRepository;
    private readonly IRepository<PostComment, Guid> _commentRepository;
    private readonly IRepository<ArticleCategory, Guid> _categoryRepository;
    private readonly HtmlSanitizerService _sanitizer;
    private readonly IDistributedCache<CommentRateLimitCacheItem> _commentLimitCache;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly Volo.Abp.Identity.IIdentityUserRepository _userRepository;

    public BlogAppService(
        IPostRepository postRepository,
        IRepository<PostComment, Guid> commentRepository,
        IRepository<ArticleCategory, Guid> categoryRepository,
        HtmlSanitizerService sanitizer,
        IDistributedCache<CommentRateLimitCacheItem> commentLimitCache,
        AlumniApplicationMappers alumniMappers,
        Volo.Abp.Identity.IIdentityUserRepository userRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _categoryRepository = categoryRepository;
        _sanitizer = sanitizer;
        _commentLimitCache = commentLimitCache;
        _alumniMappers = alumniMappers;
        _userRepository = userRepository;
    }

    [Authorize(AlumniPermissions.Magazine.ManagePosts)]
    public async Task<BlogPostDto> CreatePostAsync(CreateUpdateBlogPostDto input)
    {
        var post = new BlogPost(
            GuidGenerator.Create(),
            input.Title,
            input.Slug,
            input.Summary,
            _sanitizer.Sanitize(input.Content),
            CurrentUser.GetId(),
            input.CategoryId,
            input.Tags
        )
        {
            IsPublished = input.IsPublished,
            IsFeatured = input.IsFeatured,
            CoverImageBlobName = input.CoverImageUrl
        };

        await _postRepository.InsertAsync(post);
        return _alumniMappers.MapToDto(post);
    }

    [Authorize(AlumniPermissions.Magazine.ManagePosts)]
    public async Task<BlogPostDto> UpdatePostAsync(Guid id, CreateUpdateBlogPostDto input)
    {
        var post = await _postRepository.GetAsync(id);
        post.Title = input.Title;
        post.Slug = input.Slug;
        post.Summary = input.Summary;
        post.Content = _sanitizer.Sanitize(input.Content);
        post.CategoryId = input.CategoryId;
        post.Tags = input.Tags;
        post.IsPublished = input.IsPublished;
        post.IsFeatured = input.IsFeatured;
        post.CoverImageBlobName = input.CoverImageUrl;

        await _postRepository.UpdateAsync(post);
        return _alumniMappers.MapToDto(post);
    }

    [AllowAnonymous]
    public async Task<BlogPostDto> GetAsync(Guid id)
    {
        var post = await _postRepository.GetAsync(id);
        post.ViewCount++; // Increment view count
        await _postRepository.UpdateAsync(post); // Save changes
        
        var dto = _alumniMappers.MapToDto(post);
        var author = await _userRepository.FindAsync(post.AuthorId);
        dto.AuthorName = author != null ? $"{author.Name} {author.Surname}".Trim() : "Unknown";
        
        return dto;
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<BlogPostDto>> GetListAsync(PostSearchInputDto input)
    {
        var count = await _postRepository.GetCountAsync(input.CategoryId, input.Keyword, input.MinDate, input.MaxDate, true, input.IsFeatured, input.Tag);
        var items = await _postRepository.GetListAsync(input.CategoryId, input.Keyword, input.MinDate, input.MaxDate, true, input.IsFeatured, input.Tag, input.SkipCount, input.MaxResultCount, input.Sorting);
        
        var dtos = _alumniMappers.MapToDtos(items);

        // Populate Author Names
        var authorIds = items.Select(x => x.AuthorId).Distinct().ToList();
        var authors = await _userRepository.GetListByIdsAsync(authorIds);
        var authorDictionary = authors.ToDictionary(x => x.Id, x => $"{x.Name} {x.Surname}".Trim());

        foreach (var dto in dtos)
        {
            if (authorDictionary.TryGetValue(dto.AuthorId, out var authorName))
            {
                dto.AuthorName = authorName;
            }
        }

        return new PagedResultDto<BlogPostDto>(count, dtos);
    }

    [AllowAnonymous]
    public async Task<ListResultDto<ArticleCategoryLookupDto>> GetCategoryLookupAsync()
    {
        var categories = await _categoryRepository.GetListAsync(x => x.IsActive);
        return new ListResultDto<ArticleCategoryLookupDto>(
            categories.Select(x => new ArticleCategoryLookupDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn
            }).ToList()
        );
    }

    [Authorize(AlumniPermissions.Magazine.ManagePosts)]
    public async Task DeletePostAsync(Guid id)
    {
        await _postRepository.DeleteAsync(id);
    }

    public async Task<PostCommentDto> AddCommentAsync(Guid postId, CreatePostCommentDto input)
    {
        var currentUserId = CurrentUser.GetId();
        
        // Rate Limiting (1 comment per minute)
        var cacheKey = $"CommentLimit_{currentUserId}_{postId}";
        var cacheItem = await _commentLimitCache.GetAsync(cacheKey);
        
        if (cacheItem != null && (DateTime.Now - cacheItem.LastCommentTime).TotalMinutes < 1)
        {
            throw new UserFriendlyException("Please wait 1 minute between comments.");
        }

        var post = await _postRepository.WithDetailsAsync(x => x.Comments)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == postId));
            
        if (post == null) throw new EntityNotFoundException(typeof(BlogPost), postId);

        var commentId = GuidGenerator.Create();
        post.AddComment(commentId, currentUserId, _sanitizer.Sanitize(input.Content));
        
        await _postRepository.UpdateAsync(post);
        await _commentLimitCache.SetAsync(cacheKey, new CommentRateLimitCacheItem { LastCommentTime = DateTime.Now }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
        
        var comment = post.Comments.First(x => x.Id == commentId);
        return _alumniMappers.MapToDto(comment);
    }

    public async Task<PagedResultDto<PostCommentDto>> GetCommentsAsync(Guid postId, PagedResultRequestDto input)
    {
        var query = await _commentRepository.GetQueryableAsync();
        query = query.Where(x => x.BlogPostId == postId && x.IsApproved);
        
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<PostCommentDto>(totalCount, _alumniMappers.MapToDtos(items));
    }

    [Authorize(AlumniPermissions.Magazine.ApproveComments)]
    public async Task ApproveCommentAsync(Guid postId, Guid commentId)
    {
        var comment = await _commentRepository.GetAsync(commentId);
        comment.IsApproved = true;
        await _commentRepository.UpdateAsync(comment);
    }

    [Authorize(AlumniPermissions.Magazine.ApproveComments)]
    public async Task DeleteCommentAsync(Guid postId, Guid commentId)
    {
        await _commentRepository.DeleteAsync(commentId);
    }
}
