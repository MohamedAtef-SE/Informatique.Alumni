using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Dashboard;
using Xunit;

namespace Informatique.Alumni.Trips;

public class TripAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ITripAppService _tripAppService;
    private readonly IRepository<AlumniTrip, Guid> _tripRepository;

    public TripAppServiceTests()
    {
        _tripAppService = GetRequiredService<ITripAppService>();
        _tripRepository = GetRequiredService<IRepository<AlumniTrip, Guid>>();
    }

    [Fact]
    public async Task GetActiveTripsAsync_Should_Return_Active_Trips()
    {
        // Act
        var result = await _tripAppService.GetActiveTripsAsync(new PagedAndSortedResultRequestDto());

        // Assert
        result.ShouldNotBeNull();
    }
}
