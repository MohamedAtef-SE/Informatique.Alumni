using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

using Informatique.Alumni.Events;

namespace Informatique.Alumni.Companies;

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

    public async Task<Company> CreateAsync(
        string nameAr, 
        string nameEn, 
        string logoBlobName, 
        string? websiteUrl = null,
        string? industry = null,
        string? description = null,
        string? email = null,
        string? phoneNumber = null,
        bool isActive = true)
    {
        await ValidateUniquenessAsync(nameAr, nameEn);
        return new Company(
            GuidGenerator.Create(), 
            nameAr, 
            nameEn, 
            logoBlobName, 
            websiteUrl,
            industry,
            description,
            email,
            phoneNumber,
            isActive);
    }

    public async Task UpdateAsync(
        Company company,
        string nameAr,
        string nameEn,
        string? logoBlobName,
        string? websiteUrl,
        string? industry,
        string? description,
        string? email,
        string? phoneNumber,
        bool isActive)
    {
        if (company.NameAr != nameAr || company.NameEn != nameEn)
        {
            await ValidateUniquenessAsync(nameAr, nameEn, company.Id);
        }

        company.SetNameAr(nameAr);
        company.SetNameEn(nameEn);
        if (logoBlobName != null) company.SetLogoBlobName(logoBlobName);
        company.SetWebsiteUrl(websiteUrl);
        company.SetIndustry(industry);
        company.SetDescription(description);
        company.SetEmail(email);
        company.SetPhoneNumber(phoneNumber);
        company.SetIsActive(isActive);
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

    private async Task ValidateUniquenessAsync(string nameAr, string nameEn, Guid? excludeId = null)
    {
        if (await _companyRepository.AnyAsync(x => (x.NameAr == nameAr || x.NameEn == nameEn) && x.Id != excludeId))
        {
            throw new BusinessException(AlumniDomainErrorCodes.Company.CompanyAlreadyExists);
        }
    }
}
