using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DailyStatsDto> GetLatestStatsAsync();
}

public interface ITripAppService : IApplicationService
{
    Task<PagedResultDto<AlumniTripDto>> GetActiveTripsAsync(PagedAndSortedResultRequestDto input);
    Task<AlumniTripDto> CreateTripAsync(AlumniTripDto input);
    Task RequestTripAsync(Guid tripId, int guestCount);
}
