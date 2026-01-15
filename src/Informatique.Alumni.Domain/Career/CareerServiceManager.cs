using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Career;

public class CareerServiceManager : DomainService
{
    private readonly IRepository<CareerService, Guid> _serviceRepository;

    public CareerServiceManager(IRepository<CareerService, Guid> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<CareerService> CreateAsync(
        string nameAr,
        string nameEn,
        string code,
        string description,
        bool hasFees,
        decimal feeAmount,
        DateTime lastSubscriptionDate,
        Guid serviceTypeId,
        Guid branchId,
        List<(DateTime date, TimeSpan start, TimeSpan end, string lecturer, string room, string address, int capacity)> timeslots
    )
    {
        // 1. Validate Code Uniqueness
        if (await _serviceRepository.AnyAsync(x => x.Code == code))
        {
            throw new BusinessException("Career:DuplicateServiceCode");
        }

        // 2. Validate Schedule (Overlaps & Input Logic)
        if (timeslots == null || !timeslots.Any())
        {
            throw new BusinessException("Career:ServiceMustHaveTimeslots");
        }

        foreach (var slot in timeslots)
        {
            if (slot.start >= slot.end)
            {
                throw new BusinessException("Career:StartTimeMustBeBeforeEndTime");
            }
            
            // Check Last Subscription Date validity
            var slotStartDateTime = slot.date.Date.Add(slot.start);
            if (lastSubscriptionDate > slotStartDateTime)
            {
                throw new BusinessException("Career:LastSubscriptionDateTooLate");
            }

            // Check overlap with OTHER services (same room)
            // Rule: (Start < End) and (End > Start)
            // We need to check against DB.
            // Optimization: This check might be heavy. 
            // Query logic:
            // Services -> Timeslots -> Room == slot.room && Date == slot.date 
            // && (slot.start < t.EndTime && slot.end > t.StartTime)
            
            var overlapExists = await _serviceRepository.GetListAsync(
                s => s.Timeslots.Any(t => 
                    t.Room == slot.room && 
                    t.Date.Date == slot.date.Date &&
                    (slot.start < t.EndTime && slot.end > t.StartTime)
                )
            );

            if (overlapExists.Any())
            {
                throw new BusinessException("Career:RoomScheduleConflict")
                    .WithData("Room", slot.room)
                    .WithData("Date", slot.date.ToString("yyyy-MM-dd"));
            }
        }

        // 3. Create Entity
        var service = new CareerService(
            GuidGenerator.Create(),
            nameAr,
            nameEn,
            code,
            description,
            hasFees,
            feeAmount,
            lastSubscriptionDate,
            serviceTypeId,
            branchId
        );

        // 4. Add Timeslots
        foreach (var slot in timeslots)
        {
            service.Timeslots.Add(new CareerServiceTimeslot(
                GuidGenerator.Create(),
                service.Id,
                slot.date,
                slot.start,
                slot.end,
                slot.lecturer,
                slot.room,
                slot.address,
                slot.capacity
            ));
        }

        return service;
    }
}
