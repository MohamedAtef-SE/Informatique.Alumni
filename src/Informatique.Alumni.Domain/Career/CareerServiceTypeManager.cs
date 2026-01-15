using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Career;

public class CareerServiceTypeManager : DomainService
{
    private readonly IRepository<CareerServiceType, Guid> _serviceTypeRepository;
    private readonly IRepository<CareerService, Guid> _careerServiceRepository;

    public CareerServiceTypeManager(
        IRepository<CareerServiceType, Guid> serviceTypeRepository,
        IRepository<CareerService, Guid> careerServiceRepository)
    {
        _serviceTypeRepository = serviceTypeRepository;
        _careerServiceRepository = careerServiceRepository;
    }

    public async Task<CareerServiceType> CreateAsync(string nameAr, string nameEn)
    {
        await ValidateUniquenessAsync(nameAr, nameEn);
        return new CareerServiceType(GuidGenerator.Create(), nameAr, nameEn);
    }

    public async Task UpdateAsync(CareerServiceType serviceType, string nameAr, string nameEn, bool isActive)
    {
        await ValidateUniquenessAsync(nameAr, nameEn, serviceType.Id);
        
        serviceType.SetNames(nameAr, nameEn);
        if (isActive)
        {
            serviceType.Activate();
        }
        else
        {
            serviceType.Deactivate();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var isUsed = await _careerServiceRepository.AnyAsync(x => x.ServiceTypeId == id);
        if (isUsed)
        {
            throw new BusinessException("Career:CannotDeleteUsedServiceType");
        }

        await _serviceTypeRepository.DeleteAsync(id);
    }

    private async Task ValidateUniquenessAsync(string nameAr, string nameEn, Guid? existingId = null)
    {
        if (await _serviceTypeRepository.AnyAsync(x => x.NameAr == nameAr && x.Id != existingId))
        {
            throw new BusinessException("Career:DuplicateServiceTypeNameAr");
        }

        if (await _serviceTypeRepository.AnyAsync(x => x.NameEn == nameEn && x.Id != existingId))
        {
            throw new BusinessException("Career:DuplicateServiceTypeNameEn");
        }
    }
}
