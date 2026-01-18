using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Events;

public class ActivityTypeManager : DomainService
{
    private readonly IRepository<ActivityType, Guid> _activityTypeRepository;
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;

    public ActivityTypeManager(
        IRepository<ActivityType, Guid> activityTypeRepository,
        IRepository<AssociationEvent, Guid> eventRepository)
    {
        _activityTypeRepository = activityTypeRepository;
        _eventRepository = eventRepository;
    }

    public async Task<ActivityType> CreateAsync(string nameAr, string nameEn, bool isActive = true)
    {
        await ValidateUniquenessAsync(nameAr, nameEn);

        var activityType = new ActivityType(
            GuidGenerator.Create(),
            nameAr,
            nameEn,
            isActive
        );

        return await _activityTypeRepository.InsertAsync(activityType);
    }

    public async Task<ActivityType> UpdateAsync(ActivityType activityType, string nameAr, string nameEn, bool isActive)
    {
        await ValidateUniquenessAsync(nameAr, nameEn, activityType.Id);

        activityType.SetNameAr(nameAr);
        activityType.SetNameEn(nameEn);
        activityType.SetIsActive(isActive);

        return await _activityTypeRepository.UpdateAsync(activityType);
    }

    public async Task DeleteAsync(Guid id)
    {
        // Referential Integrity Check
        var isUsed = await _eventRepository.AnyAsync(e => e.ActivityTypeId == id);
        if (isUsed)
        {
            throw new BusinessException("Alumni:ActivityType:CannotDeleteUsed")
                .WithData("Id", id);
        }

        await _activityTypeRepository.DeleteAsync(id);
    }

    private async Task ValidateUniquenessAsync(string nameAr, string nameEn, Guid? id = null)
    {
        var existingAr = await _activityTypeRepository.AnyAsync(x => x.NameAr == nameAr && x.Id != id);
        if (existingAr)
        {
            throw new BusinessException("Alumni:ActivityType:DuplicateNameAr")
                .WithData("NameAr", nameAr);
        }

        var existingEn = await _activityTypeRepository.AnyAsync(x => x.NameEn == nameEn && x.Id != id);
        if (existingEn)
        {
            throw new BusinessException("Alumni:ActivityType:DuplicateNameEn")
                .WithData("NameEn", nameEn);
        }
    }
}
