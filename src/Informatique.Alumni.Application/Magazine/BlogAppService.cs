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
    private readonly HtmlSanitizerService _sanitizer;
    private readonly IDistributedCache<CommentRateLimitCacheItem> _commentLimitCache;
    private readonly AlumniApplicationMappers _alumniMappers;

    public BlogAppService(
        IPostRepository postRepository,
        IRepository<PostComment, Guid> commentRepository,
        HtmlSanitizerService sanitizer,
        IDistributedCache<CommentRateLimitCacheItem> commentLimitCache,
        AlumniApplicationMappers alumniMappers)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _sanitizer = sanitizer;
        _commentLimitCache = commentLimitCache;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Magazine.ManagePosts)]
    public async Task<BlogPostDto> CreatePostAsync(CreateUpdateBlogPostDto input)
    {
        var post = new BlogPost(
            GuidGenerator.Create(),
            input.Title,
            _sanitizer.Sanitize(input.Content),
            CurrentUser.GetId(),
            input.Category
        ) { IsPublished = input.IsPublished };

        await _postRepository.InsertAsync(post);
        return _alumniMappers.MapToDto(post);
    }

    [Authorize(AlumniPermissions.Magazine.ManagePosts)]
    public async Task<BlogPostDto> UpdatePostAsync(Guid id, CreateUpdateBlogPostDto input)
    {
        var post = await _postRepository.GetAsync(id);
        post.Title = input.Title;
        post.Content = _sanitizer.Sanitize(input.Content);
        post.Category = input.Category;
        post.IsPublished = input.IsPublished;

        await _postRepository.UpdateAsync(post);
        return _alumniMappers.MapToDto(post);
    }

    public async Task<BlogPostDto> GetPostAsync(Guid id)
    {
        var post = await _postRepository.GetAsync(id);
        return _alumniMappers.MapToDto(post);
    }

    public async Task<PagedResultDto<BlogPostDto>> GetPostsAsync(PostSearchInputDto input)
    {
        var count = await _postRepository.GetCountAsync(input.Category, input.Keyword, input.MinDate, input.MaxDate, true);
        var items = await _postRepository.GetListAsync(input.Category, input.Keyword, input.MinDate, input.MaxDate, true, input.SkipCount, input.MaxResultCount, input.Sorting);
        
        return new PagedResultDto<BlogPostDto>(count, _alumniMappers.MapToDtos(items));
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
