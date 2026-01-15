using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Trips;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Dashboard;

[Authorize(AlumniPermissions.Dashboard.ViewStatistics)]
public class DashboardAppService : AlumniAppService, IDashboardAppService
{
    private readonly IRepository<DailyStats, Guid> _statsRepository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public DashboardAppService(
        IRepository<DailyStats, Guid> statsRepository,
        AlumniApplicationMappers alumniMappers)
    {
        _statsRepository = statsRepository;
        _alumniMappers = alumniMappers;
    }

    public async Task<DailyStatsDto> GetLatestStatsAsync()
    {
        var stats = (await _statsRepository.GetQueryableAsync())
            .OrderByDescending(x => x.Date)
            .FirstOrDefault();

        if (stats == null)
        {
            return new DailyStatsDto { Date = DateTime.Today };
        }

        var dto = _alumniMappers.MapToDto(stats);

        // Security check for Financials
        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Dashboard.ViewFinancials))
        {
            dto.TotalRevenue = 0; // Redact revenue if not President
        }

        return dto;
    }
}
