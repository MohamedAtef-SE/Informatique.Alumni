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
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        string? sorting = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, category, keyword, minDate, maxDate, isPublished);
        
        return await query
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(BlogPost.CreationTime) + " desc" : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<long> GetCountAsync(
        string? category = null,
        string? keyword = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool? isPublished = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = ApplyFilter(query, category, keyword, minDate, maxDate, isPublished);
        
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    protected virtual IQueryable<BlogPost> ApplyFilter(
        IQueryable<BlogPost> query,
        string? category = null,
        string? keyword = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool? isPublished = null)
    {
        return query
            .WhereIf(!category.IsNullOrWhiteSpace(), x => x.Category == category)
            .WhereIf(!keyword.IsNullOrWhiteSpace(), x => x.Title.Contains(keyword!) || x.Content.Contains(keyword!))
            .WhereIf(minDate.HasValue, x => x.CreationTime >= minDate.Value)
            .WhereIf(maxDate.HasValue, x => x.CreationTime <= maxDate.Value)
            .WhereIf(isPublished.HasValue, x => x.IsPublished == isPublished.Value);
    }
}
