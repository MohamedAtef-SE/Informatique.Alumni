
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Benefits;

public class DiscountOfferManager : DomainService
{
    private readonly IRepository<DiscountOffer, Guid> _offerRepository;

    public DiscountOfferManager(IRepository<DiscountOffer, Guid> offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<DiscountOffer> CreateAsync(Guid categoryId, string title, string filePath)
    {
        await ValidateUniquenessAsync(categoryId);
        ValidateFileType(filePath);

        return new DiscountOffer(
            GuidGenerator.Create(),
            categoryId,
            title,
            filePath
        );
    }

    public async Task UpdateAsync(DiscountOffer offer, string title, string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath) && offer.FilePath != filePath)
        {
             ValidateFileType(filePath);
        }

        offer.UpdateInfo(title, filePath);
        await Task.CompletedTask; // No async work needed if just updating entity logic, but keeping async signature standard.
    }

    private async Task ValidateUniquenessAsync(Guid categoryId)
    {
        if (await _offerRepository.AnyAsync(x => x.DiscountCategoryId == categoryId))
        {
            throw new UserFriendlyException("A file already exists for this category.");
        }
    }

    private void ValidateFileType(string filePath)
    {
        var ext = Path.GetExtension(filePath)?.ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
        {
             throw new UserFriendlyException("Invalid file type. Only JPG, PNG, and PDF are allowed.");
        }
    }
}
