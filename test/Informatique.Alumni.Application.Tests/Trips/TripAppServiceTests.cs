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
    
    [Fact]
    public async Task RequestTripAsync_Should_Throw_When_Capacity_Exceeded()
    {
        // Arrange
        var trip = new AlumniTrip(
            Guid.NewGuid(), Guid.Empty, "Test Trip", "Test Trip", TripType.Internal, 
            DateTime.Now.AddDays(10), DateTime.Now.AddDays(15), TimeSpan.Zero, 
            DateTime.Now.AddDays(5), DateTime.Now.AddDays(5), "Location", 0m, true, 2);
            
        await _tripRepository.InsertAsync(trip);
        
        // Act & Assert
        // Guest count = 2 implies 3 total participants (1 alumni + 2 guests). This exceeds max capacity of 2.
        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(async () =>
        {
            await _tripAppService.RequestTripAsync(trip.Id, 2);
        });
    }
}
