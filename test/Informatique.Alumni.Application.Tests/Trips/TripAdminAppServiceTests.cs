using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Admin;
using Xunit;

using System.Linq;

namespace Informatique.Alumni.Trips;

public class TripAdminAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ITripAdminAppService _tripAdminAppService;
    private readonly IRepository<AlumniTrip, Guid> _tripRepository;

    public TripAdminAppServiceTests()
    {
        _tripAdminAppService = GetRequiredService<ITripAdminAppService>();
        _tripRepository = GetRequiredService<IRepository<AlumniTrip, Guid>>();
    }

    [Fact]
    public async Task GetTripsAsync_Should_Return_Paginated_Trips_Without_Errors()
    {
        // Act
        var result = await _tripAdminAppService.GetTripsAsync(new TripAdminGetListInput { MaxResultCount = 10 });

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task GetRequestsAsync_Should_Return_Paginated_Requests_For_Valid_Trip()
    {
        // Arrange
        var tripId = Guid.NewGuid(); // Or ideally seeded trip ID

        // Act
        var result = await _tripAdminAppService.GetRequestsAsync(tripId, new PagedAndSortedResultRequestDto());

        // Assert
        result.ShouldNotBeNull();
    }
}
