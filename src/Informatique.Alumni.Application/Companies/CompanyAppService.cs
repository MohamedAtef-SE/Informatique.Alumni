// Remove unused using

using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Companies;


//[Authorize(AlumniPermissions.Companies.Manage)]
public class CompanyAppService : ApplicationService, ICompanyAppService
{
    private readonly CompanyManager _companyManager;
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IBlobContainer<CompanyLogoContainer> _blobContainer;

    public CompanyAppService(
        CompanyManager companyManager,
        IRepository<Company, Guid> companyRepository,
        IBlobContainer<CompanyLogoContainer> blobContainer)
    {
        _companyManager = companyManager;
        _companyRepository = companyRepository;
        _blobContainer = blobContainer;
    }


    public async Task<PagedResultDto<CompanyDto>> GetListAsync([FromQuery] CompanyFilterDto input)
    {
        var queryable = await _companyRepository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(x =>
                x.NameAr.Contains(input.Filter!) ||
                x.NameEn.Contains(input.Filter!) ||
                x.Industry.Contains(input.Filter!)
            );
        }

        if (input.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == input.IsActive.Value);
        }

        // --- FIX FOR THE 500 ERROR ---
        // Ensure the sorting string matches the PascalCase C# properties
        var sorting = input.Sorting;
        if (string.IsNullOrWhiteSpace(sorting))
        {
            sorting = nameof(Company.NameEn);
        }
        else
        {
            // Converts "nameEn" to "NameEn", or "nameEn desc" to "NameEn desc"
            var parts = sorting.Split(' ');
            parts[0] = char.ToUpper(parts[0][0]) + parts[0].Substring(1);
            sorting = string.Join(" ", parts);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var list = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(sorting ?? nameof(Company.NameEn))

                     .PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<CompanyDto>(
            totalCount,
            list.Select(MapToDto).ToList()
        );
    }


    public async Task<CompanyDto> GetAsync(Guid id)
    {
        var company = await _companyRepository.GetAsync(id);
        return MapToDto(company);
    }

    public async Task<Guid> CreateAsync([FromForm] CreateCompanyDto input)

    {
        string logoBlobName = await UploadLogoAsync(input.Logo);

        var company = await _companyManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            logoBlobName,
            input.WebsiteUrl,
            input.Industry,
            input.Description,
            input.Email,
            input.PhoneNumber
        );

        await _companyRepository.InsertAsync(company);

        return company.Id;
    }

    
    public async Task UpdateAsync(Guid id, [FromForm] UpdateCompanyDto input)

    {
        var company = await _companyRepository.GetAsync(id);

        string? logoBlobName = null;
        if (input.Logo != null)
        {
            // Delete old logo
            await _blobContainer.DeleteAsync(company.LogoBlobName);
            // Upload new logo
            logoBlobName = await UploadLogoAsync(input.Logo);
        }

        await _companyManager.UpdateAsync(
            company,
            input.NameAr,
            input.NameEn,
            logoBlobName,
            input.WebsiteUrl,
            input.Industry,
            input.Description,
            input.Email,
            input.PhoneNumber,
            input.IsActive
        );

        await _companyRepository.UpdateAsync(company);
    }


    
    public async Task DeleteAsync(Guid id)
    {
        var company = await _companyRepository.GetAsync(id);

        // Check usage and delete
        await _companyManager.DeleteAsync(id);

        // Delete logo from blob storage
        await _blobContainer.DeleteAsync(company.LogoBlobName);
    }

    private async Task<string> UploadLogoAsync(Volo.Abp.Content.IRemoteStreamContent logo)
    {
        var extension = Path.GetExtension(logo.FileName)?.ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            throw new UserFriendlyException("Only .jpg, .jpeg, and .png formats are allowed for logos.");
        }

        var logoBlobName = Guid.NewGuid().ToString() + extension;

        using (var stream = logo.GetStream())
        {
            await _blobContainer.SaveAsync(logoBlobName, stream);
        }

        return logoBlobName;
    }

    private CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            NameAr = company.NameAr,
            NameEn = company.NameEn,
            LogoBlobName = company.LogoBlobName,
            WebsiteUrl = company.WebsiteUrl,
            Industry = company.Industry,
            Description = company.Description,
            Email = company.Email,
            PhoneNumber = company.PhoneNumber,
            IsActive = company.IsActive
        };
    }
}
