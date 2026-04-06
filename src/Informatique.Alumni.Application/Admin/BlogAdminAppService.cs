using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Magazine;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.ContentModerate)]
public class BlogAdminAppService : AlumniAppService, IBlogAdminAppService
{
    private readonly IPostRepository _postRepository;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<ArticleCategory, Guid> _categoryRepository;

    public BlogAdminAppService(
        IPostRepository postRepository,
        AlumniApplicationMappers alumniMappers,
        IIdentityUserRepository userRepository,
        IRepository<ArticleCategory, Guid> categoryRepository)
    {
        _postRepository = postRepository;
        _alumniMappers = alumniMappers;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResultDto<BlogPostDto>> GetListAsync(PostSearchInputDto input)
    {
        // Notice we explicitly pass null for isPublished so both Drafts and Published are returned
        var count = await _postRepository.GetCountAsync(input.CategoryId, input.Keyword, input.MinDate, input.MaxDate, null, input.IsFeatured, input.Tag);
        var items = await _postRepository.GetListAsync(input.CategoryId, input.Keyword, input.MinDate, input.MaxDate, null, input.IsFeatured, input.Tag, input.SkipCount, input.MaxResultCount, input.Sorting);
        
        var dtos = _alumniMappers.MapToDtos(items);

        // Populate Author Names
        var authorIds = items.Select(x => x.AuthorId).Distinct().ToList();
        var authors = await _userRepository.GetListByIdsAsync(authorIds);
        var authorDictionary = authors.ToDictionary(x => x.Id, x => $"{x.Name} {x.Surname}".Trim());

        // Populate Category Names
        var categoryIds = items.Where(x => x.CategoryId.HasValue).Select(x => x.CategoryId.Value).Distinct().ToList();
        var categories = await _categoryRepository.GetListAsync(c => categoryIds.Contains(c.Id));
        var isArabic = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("ar");
        var categoryDictionary = categories.ToDictionary(x => x.Id, x => isArabic ? x.NameAr : x.NameEn);

        foreach (var dto in dtos)
        {
            if (authorDictionary.TryGetValue(dto.AuthorId, out var authorName))
            {
                dto.AuthorName = authorName;
            }
            if (dto.CategoryId.HasValue && categoryDictionary.TryGetValue(dto.CategoryId.Value, out var categoryName))
            {
                dto.Category = categoryName;
            }
        }

        return new PagedResultDto<BlogPostDto>(count, dtos);
    }

    public async Task PublishAsync(Guid id)
    {
        var post = await _postRepository.GetAsync(id);
        post.IsPublished = true;
        await _postRepository.UpdateAsync(post);
    }

    public async Task UnpublishAsync(Guid id)
    {
        var post = await _postRepository.GetAsync(id);
        post.IsPublished = false;
        await _postRepository.UpdateAsync(post);
    }

    public async Task<PagedResultDto<ArticleCategoryDto>> GetCategoriesAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _categoryRepository.GetQueryableAsync();
        
        var count = await AsyncExecuter.CountAsync(queryable);
        
        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(ArticleCategory.NameEn) : input.Sorting)
                .PageBy(input)
        );

        return new PagedResultDto<ArticleCategoryDto>(
            count,
            _alumniMappers.MapToDtos(items)
        );
    }

    public async Task<ArticleCategoryDto> CreateCategoryAsync(CreateUpdateArticleCategoryDto input)
    {
        var category = new ArticleCategory(
            GuidGenerator.Create(),
            input.NameEn,
            input.NameAr
        )
        {
            IsActive = input.IsActive
        };

        await _categoryRepository.InsertAsync(category);
        return _alumniMappers.MapToDto(category);
    }

    public async Task<ArticleCategoryDto> UpdateCategoryAsync(Guid id, CreateUpdateArticleCategoryDto input)
    {
        var category = await _categoryRepository.GetAsync(id);
        category.NameEn = input.NameEn;
        category.NameAr = input.NameAr;
        category.IsActive = input.IsActive;

        await _categoryRepository.UpdateAsync(category);
        return _alumniMappers.MapToDto(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        await _categoryRepository.DeleteAsync(id);
    }
}
