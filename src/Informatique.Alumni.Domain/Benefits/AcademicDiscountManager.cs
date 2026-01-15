
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Benefits;

public class AcademicDiscountManager : DomainService
{
    private readonly IRepository<AcademicDiscount, Guid> _discountRepository;

    public AcademicDiscountManager(IRepository<AcademicDiscount, Guid> discountRepository)
    {
        _discountRepository = discountRepository;
    }

    public async Task<AcademicDiscount> CreateAsync(string nameAr, string nameEn, AcademicDiscountType type, decimal percentage)
    {
        await ValidateUniquenessAsync(nameAr, type);

        return new AcademicDiscount(
            GuidGenerator.Create(),
            nameAr,
            nameEn,
            type,
            percentage
        );
    }

    public async Task UpdateAsync(AcademicDiscount discount, string nameAr, string nameEn, AcademicDiscountType type, decimal percentage)
    {
        if (discount.NameAr != nameAr || discount.Type != type)
        {
             await ValidateUniquenessAsync(nameAr, type);
        }

        discount.UpdateInfo(nameAr, nameEn, type, percentage);
    }

    private async Task ValidateUniquenessAsync(string nameAr, AcademicDiscountType type)
    {
        if (await _discountRepository.AnyAsync(x => x.NameAr == nameAr && x.Type == type))
        {
            throw new UserFriendlyException($"An Academic Discount with name '{nameAr}' and type '{type}' already exists.");
        }
    }
}
