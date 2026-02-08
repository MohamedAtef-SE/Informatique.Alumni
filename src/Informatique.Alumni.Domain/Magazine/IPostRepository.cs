using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Magazine;

public interface IPostRepository : IRepository<BlogPost, Guid>
{
    Task<List<BlogPost>> GetListAsync(
        string? category = null,
        string? keyword = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool? isPublished = null,
        bool? isFeatured = null,
        string? tag = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        string? sorting = null,
        CancellationToken cancellationToken = default
    );

    Task<long> GetCountAsync(
        string? category = null,
        string? keyword = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool? isPublished = null,
        bool? isFeatured = null,
        string? tag = null,
        CancellationToken cancellationToken = default
    );
}
