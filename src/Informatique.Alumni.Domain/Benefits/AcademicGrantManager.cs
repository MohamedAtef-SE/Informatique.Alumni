using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Benefits;

public class AcademicGrantManager : DomainService
{
    private readonly IRepository<AcademicGrant, Guid> _grantRepository;

    public AcademicGrantManager(IRepository<AcademicGrant, Guid> grantRepository)
    {
        _grantRepository = grantRepository;
    }

    public async Task<AcademicGrant> CreateAsync(string nameAr, string nameEn, string type, double percentage)
    {
        // Validation: Unique Name+Type
        if (await _grantRepository.AnyAsync(x => x.NameEn == nameEn && x.Type == type))
        {
            throw new UserFriendlyException("A grant with this Name (En) and Type already exists.");
        }
        // Additional check for Arabic Name optional but good practice

        var grant = new AcademicGrant(GuidGenerator.Create(), nameAr, nameEn, type, percentage);
        return await _grantRepository.InsertAsync(grant);
    }
}
