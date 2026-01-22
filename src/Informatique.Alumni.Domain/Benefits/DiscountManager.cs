using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Benefits;

public class DiscountManager : DomainService
{
    private readonly IRepository<DiscountOffer, Guid> _offerRepository;

    public DiscountManager(IRepository<DiscountOffer, Guid> offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<DiscountOffer> CreateOfferAsync(Guid categoryId, string title, string fileUrl)
    {
        // Validation: One File per Category
        if (await _offerRepository.AnyAsync(x => x.DiscountCategoryId == categoryId))
        {
            throw new UserFriendlyException("This category already has an active discount offer file. Please update/delete it first.");
        }

        return new DiscountOffer(GuidGenerator.Create(), categoryId, title, fileUrl);
    }
}
