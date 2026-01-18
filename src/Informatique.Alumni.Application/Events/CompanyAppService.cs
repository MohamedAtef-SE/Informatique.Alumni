using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;

namespace Informatique.Alumni.Events;

[Authorize]
public class CompanyAppService : ApplicationService
{
    private readonly CompanyManager _companyManager;
    private readonly IBlobContainer<CompanyLogoContainer> _blobContainer;

    public CompanyAppService(
        CompanyManager companyManager,
        IBlobContainer<CompanyLogoContainer> blobContainer)
    {
        _companyManager = companyManager;
        _blobContainer = blobContainer;
    }

    public async Task<Guid> CreateAsync(CreateCompanyDto input)
    {
        var logoBlobName = Guid.NewGuid().ToString() + Path.GetExtension(input.Logo.FileName);
        
        // Validate Extension
        var extension = Path.GetExtension(input.Logo.FileName)?.ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            throw new UserFriendlyException("Only .jpg, .jpeg, and .png formats are allowed for logos.");
        }

        // Upload to Blob Storage
        using (var stream = input.Logo.GetStream())
        {
            await _blobContainer.SaveAsync(logoBlobName, stream);
        }

        var company = await _companyManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            logoBlobName,
            input.WebsiteUrl
        );

        return company.Id;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _companyManager.DeleteAsync(id);
    }
}
