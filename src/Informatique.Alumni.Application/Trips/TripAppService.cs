using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Trips;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Dashboard;

namespace Informatique.Alumni.Trips;

[Authorize]
public class TripAppService : AlumniAppService, ITripAppService
{
    private readonly IRepository<AlumniTrip, Guid> _tripRepository;
    private readonly IRepository<TripRequest, Guid> _requestRepository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public TripAppService(
        IRepository<AlumniTrip, Guid> tripRepository,
        IRepository<TripRequest, Guid> requestRepository,
        AlumniApplicationMappers alumniMappers)
    {
        _tripRepository = tripRepository;
        _requestRepository = requestRepository;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<AlumniTripDto>> GetActiveTripsAsync(PagedAndSortedResultRequestDto input)
    {
        var query = (await _tripRepository.GetQueryableAsync())
            .Where(x => x.IsActive && x.StartDate > DateTime.Now);
        
        var count = await AsyncExecuter.CountAsync(query);
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "StartDate").PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<AlumniTripDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Trips.Manage)]
    public async Task<AlumniTripDto> CreateTripAsync(AlumniTripDto input)
    {
        var trip = new AlumniTrip(
            GuidGenerator.Create(),
            Guid.Empty, // Default BranchId
            input.Title,
            input.Title,
            TripType.Internal, // Default Type
            input.StartDate,
            input.EndDate,
            TimeSpan.Zero,
            input.StartDate.AddDays(-1),
            input.StartDate.AddDays(-1),
            input.Destination,
            0m, // AdminFees
            input.MaxCapacity > 0,
            input.MaxCapacity
        );
        
        // Use Update method to set description and other fields
        trip.Update(
            input.Title,
            input.Destination,
            input.StartDate,
            input.EndDate,
            input.MaxCapacity,
            input.PricePerPerson,
            input.Description ?? string.Empty
        );
        
        await _tripRepository.InsertAsync(trip);
        return _alumniMappers.MapToDto(trip);
    }

    [Authorize(AlumniPermissions.Trips.Request)]
    public async Task RequestTripAsync(Guid tripId, int guestCount)
    {
        var trip = await _tripRepository.GetAsync(tripId);
        if (!trip.IsActive || (trip.StartDate.HasValue && trip.StartDate.Value <= DateTime.Now))
        {
            throw new UserFriendlyException("This trip is no longer active or has already started.");
        }

        // Capacity Planning: Alumni + Family
        var totalRequestedParticipants = 1 + guestCount;
        
        var existingRequests = await _requestRepository.GetListAsync(x => x.TripId == tripId && x.Status != TripRequestStatus.Cancelled && x.Status != TripRequestStatus.Rejected);
        var currentParticipantsCount = existingRequests.Sum(x => x.TotalParticipants);

        if (trip.MaxCapacity.HasValue && currentParticipantsCount + totalRequestedParticipants > trip.MaxCapacity.Value)
        {
            throw new UserFriendlyException($"Not enough capacity. Only {trip.MaxCapacity.Value - currentParticipantsCount} spots left.");
        }

        var price = trip.PricePerPerson ?? 0;
        var totalAmount = totalRequestedParticipants * price;
        var Request = new TripRequest(GuidGenerator.Create(), tripId, CurrentUser.Id ?? throw new UnauthorizedAccessException(), guestCount, totalAmount);
        
        await _requestRepository.InsertAsync(Request);
    }
}
