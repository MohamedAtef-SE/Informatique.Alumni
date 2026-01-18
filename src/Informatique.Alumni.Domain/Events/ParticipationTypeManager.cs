using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Events;

public class ParticipationTypeManager : DomainService
{
    private readonly IRepository<ParticipationType, Guid> _repository;
    private readonly IRepository<EventParticipatingCompany, Guid> _eventParticipatingCompanyRepository;

    public ParticipationTypeManager(
        IRepository<ParticipationType, Guid> repository,
        IRepository<EventParticipatingCompany, Guid> eventParticipatingCompanyRepository)
    {
        _repository = repository;
        _eventParticipatingCompanyRepository = eventParticipatingCompanyRepository;
    }

    public async Task<ParticipationType> CreateAsync(string nameAr, string nameEn)
    {
        await ValidateUniquenessAsync(nameAr, nameEn);
        return new ParticipationType(GuidGenerator.Create(), nameAr, nameEn);
    }

    public async Task DeleteAsync(Guid id)
    {
        var isUsed = await _eventParticipatingCompanyRepository.AnyAsync(x => x.ParticipationTypeId == id);
        if (isUsed)
        {
            throw new BusinessException(AlumniDomainErrorCodes.ParticipationType.CannotDeleteUsed);
        }

        await _repository.DeleteAsync(id);
    }

    private async Task ValidateUniquenessAsync(string nameAr, string nameEn)
    {
        if (await _repository.AnyAsync(x => x.NameAr == nameAr || x.NameEn == nameEn))
        {
            throw new BusinessException(AlumniDomainErrorCodes.ParticipationType.AlreadyExists);
        }
    }
}
