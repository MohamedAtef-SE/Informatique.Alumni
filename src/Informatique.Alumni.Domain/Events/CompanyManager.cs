using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Events;

public class CompanyManager : DomainService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IRepository<EventParticipatingCompany, Guid> _eventParticipatingCompanyRepository;

    public CompanyManager(
        IRepository<Company, Guid> companyRepository,
        IRepository<EventParticipatingCompany, Guid> eventParticipatingCompanyRepository)
    {
        _companyRepository = companyRepository;
        _eventParticipatingCompanyRepository = eventParticipatingCompanyRepository;
    }

    public async Task<Company> CreateAsync(string nameAr, string nameEn, string logoBlobName, string? websiteUrl = null)
    {
        await ValidateUniquenessAsync(nameAr, nameEn);
        return new Company(GuidGenerator.Create(), nameAr, nameEn, logoBlobName, websiteUrl);
    }

    public async Task DeleteAsync(Guid id)
    {
        var isUsed = await _eventParticipatingCompanyRepository.AnyAsync(x => x.CompanyId == id);
        if (isUsed)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Company.CannotDeleteUsedCompany);
        }

        await _companyRepository.DeleteAsync(id);
    }

    private async Task ValidateUniquenessAsync(string nameAr, string nameEn)
    {
        if (await _companyRepository.AnyAsync(x => x.NameAr == nameAr || x.NameEn == nameEn))
        {
            throw new BusinessException(AlumniDomainErrorCodes.Company.CompanyAlreadyExists);
        }
    }
}
