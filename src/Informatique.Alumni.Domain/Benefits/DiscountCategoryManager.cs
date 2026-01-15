
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Benefits;

public class DiscountCategoryManager : DomainService
{
    private readonly IRepository<DiscountCategory, Guid> _categoryRepository;
    private readonly IRepository<CommercialDiscount, Guid> _discountRepository;

    public DiscountCategoryManager(
        IRepository<DiscountCategory, Guid> categoryRepository,
        IRepository<CommercialDiscount, Guid> discountRepository)
    {
        _categoryRepository = categoryRepository;
        _discountRepository = discountRepository;
    }

    public async Task<DiscountCategory> CreateAsync(string nameAr, string nameEn, string? logoPath)
    {
        await ValidateUniquenessAsync(nameAr, nameEn);

        return new DiscountCategory(
            GuidGenerator.Create(),
            nameAr,
            nameEn,
            logoPath
        );
    }

    public async Task UpdateAsync(DiscountCategory category, string nameAr, string nameEn, string? logoPath)
    {
        if (category.NameAr != nameAr || category.NameEn != nameEn)
        {
             await ValidateUniquenessAsync(nameAr, nameEn, category.Id);
        }

        category.UpdateInfo(nameAr, nameEn, logoPath);
    }

    public async Task DeleteAsync(Guid id)
    {
        // Integrity Check: Cannot delete if linked offers exist
        if (await _discountRepository.AnyAsync(x => x.CategoryId == id))
        {
            throw new UserFriendlyException("Cannot delete category because it has related offers.");
        }
        
        await _categoryRepository.DeleteAsync(id);
    }

    private async Task ValidateUniquenessAsync(string nameAr, string nameEn, Guid? excludeId = null)
    {
        var existing = await _categoryRepository.AnyAsync(x => 
            (x.NameAr == nameAr || x.NameEn == nameEn) && 
            x.Id != excludeId);
            
        if (existing)
        {
            throw new UserFriendlyException($"A Discount Category with name '{nameAr}' or '{nameEn}' already exists.");
        }
    }
}
