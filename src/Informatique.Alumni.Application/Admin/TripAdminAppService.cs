using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Trips;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

using Volo.Abp.Identity;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.TripManage)]
public class TripAdminAppService : AlumniAppService, ITripAdminAppService
{
    private readonly IRepository<AlumniTrip, Guid> _tripRepository;
    private readonly IRepository<TripRequest, Guid> _requestRepository;
    private readonly IIdentityUserRepository _userRepository;

    public TripAdminAppService(
        IRepository<AlumniTrip, Guid> tripRepository,
        IRepository<TripRequest, Guid> requestRepository,
        IIdentityUserRepository userRepository)
    {
        _tripRepository = tripRepository;
        _requestRepository = requestRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResultDto<TripAdminDto>> GetTripsAsync(TripAdminGetListInput input)
    {
        var queryable = await _tripRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.NameAr.Contains(input.Filter) ||
                x.NameEn.Contains(input.Filter));
        }

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var trips = queryable.ToList();
        var reqQueryable = await _requestRepository.GetQueryableAsync();

        var items = trips.Select(t => new TripAdminDto
        {
            Id = t.Id,
            NameAr = t.NameAr,
            NameEn = t.NameEn,
            TripType = t.Type,
            StartDate = t.DateFrom,
            EndDate = t.DateTo,
            Location = t.Location,
            PricePerPerson = t.PricePerPerson ?? 0,
            MaxCapacity = t.MaxCapacity,
            IsActive = t.IsActive,
            RequestCount = reqQueryable.Count(r => r.TripId == t.Id),
            CreationTime = t.CreationTime
        }).ToList();

        return new PagedResultDto<TripAdminDto>(totalCount, items);
    }

    public async Task<PagedResultDto<TripRequestAdminDto>> GetRequestsAsync(
        Guid tripId, PagedAndSortedResultRequestDto input)
    {
        var queryable = (await _requestRepository.GetQueryableAsync())
            .Where(x => x.TripId == tripId);

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var requestEntities = queryable.ToList();
        
        // Fetch associative user data from Identity
        var userIds = requestEntities.Select(x => x.AlumniId).Distinct().ToList();
        var users = (await _userRepository.GetListByIdsAsync(userIds))
            .ToDictionary(u => u.Id, u => u);

        var items = requestEntities.Select(r => 
        {
            users.TryGetValue(r.AlumniId, out var user);
            return new TripRequestAdminDto
            {
                Id = r.Id,
                TripId = r.TripId,
                AlumniId = r.AlumniId,
                AlumniName = user?.Name ?? user?.UserName ?? "Unknown User",
                AlumniEmail = user?.Email ?? "-",
                PhoneNumber = user?.PhoneNumber ?? "-",
                GuestCount = r.GuestCount,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                CreationTime = r.CreationTime
            };
        }).ToList();

        return new PagedResultDto<TripRequestAdminDto>(totalCount, items);
    }

    public async Task ApproveRequestAsync(Guid requestId)
    {
        var request = await _requestRepository.GetAsync(requestId);
        request.Approve();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task RejectRequestAsync(Guid requestId)
    {
        var request = await _requestRepository.GetAsync(requestId);
        request.Reject();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task ActivateTripAsync(Guid id)
    {
        var trip = await _tripRepository.GetAsync(id);
        trip.Activate();
        await _tripRepository.UpdateAsync(trip);
    }

    public async Task DeactivateTripAsync(Guid id)
    {
        var trip = await _tripRepository.GetAsync(id);
        trip.Deactivate();
        await _tripRepository.UpdateAsync(trip);
    }

    public async Task DeleteTripAsync(Guid id)
    {
        await _tripRepository.DeleteAsync(id);
    }
}
