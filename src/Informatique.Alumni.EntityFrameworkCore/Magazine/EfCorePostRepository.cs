using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Informatique.Alumni.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Informatique.Alumni.Magazine;

public class EfCorePostRepository : EfCoreRepository<AlumniDbContext, BlogPost, Guid>, IPostRepository
{
    public EfCorePostRepository(IDbContextProvider<AlumniDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<BlogPost>> GetListAsync(
        string? category = null,
        string? keyword = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool? isPublished = null,
        bool? isFeatured = null,
        string? tag = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string? sorting = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, category, keyword, minDate, maxDate, isPublished, isFeatured, tag);
        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? "CreationTime DESC" : sorting);
        return await query.PageBy(skipCount, maxResultCount).ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? category = null,
        string? keyword = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool? isPublished = null,
        bool? isFeatured = null,
        string? tag = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, category, keyword, minDate, maxDate, isPublished, isFeatured, tag);
        return await query.LongCountAsync(cancellationToken);
    }

    protected IQueryable<BlogPost> ApplyFilter(
        IQueryable<BlogPost> query,
        string? category,
        string? keyword,
        DateTime? minDate,
        DateTime? maxDate,
        bool? isPublished,
        bool? isFeatured,
        string? tag)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(category), x => x.Category == category)
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x => x.Title.Contains(keyword) || x.Summary.Contains(keyword))
            .WhereIf(minDate.HasValue, x => x.CreationTime >= minDate.Value)
            .WhereIf(maxDate.HasValue, x => x.CreationTime <= maxDate.Value)
            .WhereIf(isPublished.HasValue, x => x.IsPublished == isPublished.Value)
            .WhereIf(isFeatured.HasValue, x => x.IsFeatured == isFeatured.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(tag), x => x.Tags.Contains(tag));
    }
}
