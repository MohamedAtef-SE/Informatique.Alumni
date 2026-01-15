using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.BlobContainers;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Health;

/// <summary>
/// Application service for healthcare offer management (employee operations).
/// Business Rule: Requires Healthcare.Manage permission.
/// Clean Code: CRUD operations with file handling via domain service.
/// </summary>
[Authorize(AlumniPermissions.Healthcare.Manage)]
public class HealthcareOfferManagementAppService : ApplicationService, IHealthcareOfferManagementAppService
{
    private readonly HealthcareOfferManager _manager;
    private readonly IRepository<HealthcareOffer, Guid> _repository;
    private readonly IBlobContainer<HealthcareBlobContainer> _blobContainer;

    public HealthcareOfferManagementAppService(
        HealthcareOfferManager manager,
        IRepository<HealthcareOffer, Guid> repository,
        IBlobContainer<HealthcareBlobContainer> blobContainer)
    {
        _manager = manager;
        _repository = repository;
        _blobContainer = blobContainer;
    }

    /// <summary>
    /// Gets a paged list of healthcare offers with optional filtering.
    /// Clean Code: Pagination for performance, clear filtering logic.
    /// </summary>
    public async Task<PagedResultDto<HealthcareOfferDto>> GetListAsync(GetHealthcareOffersInput input)
    {
        var queryable = await _repository.GetQueryableAsync();

        // Apply title filter
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(o => o.Title.Contains(input.Filter));
        }

        // Get total count for pagination
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting and paging
        var offers = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(o => o.UploadDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = ObjectMapper.Map<List<HealthcareOffer>, List<HealthcareOfferDto>>(offers);

        return new PagedResultDto<HealthcareOfferDto>(totalCount, dtos);
    }

    /// <summary>
    /// Gets a single healthcare offer by ID.
    /// </summary>
    public async Task<HealthcareOfferDto> GetAsync(Guid id)
    {
        var offer = await _repository.GetAsync(id);
        return ObjectMapper.Map<HealthcareOffer, HealthcareOfferDto>(offer);
    }

    /// <summary>
    /// Creates a new healthcare offer with file upload.
    /// SRP: Domain service handles file validation and storage.
    /// </summary>
    public async Task<HealthcareOfferDto> CreateAsync(CreateHealthcareOfferDto input)
    {
        // Use domain service to create offer (handles validation and file storage)
        using var stream = new MemoryStream(input.FileContent);
        
        var offer = await _manager.CreateAsync(
            input.Title,
            stream,
            input.FileName
        );

        await _repository.InsertAsync(offer);

        return ObjectMapper.Map<HealthcareOffer, HealthcareOfferDto>(offer);
    }

    /// <summary>
    /// Updates an existing healthcare offer.
    /// Business Rule: Title can always be updated, file is optional.
    /// Clean Code: Optional file replacement pattern.
    /// </summary>
    public async Task<HealthcareOfferDto> UpdateAsync(Guid id, UpdateHealthcareOfferDto input)
    {
        var offer = await _repository.GetAsync(id);

        // Prepare file stream if provided
        System.IO.Stream? fileStream = null;
        string? fileName = null;
        MemoryStream? memoryStream = null;

        if (input.FileContent != null && !string.IsNullOrWhiteSpace(input.FileName))
        {
            memoryStream = new MemoryStream(input.FileContent);
            fileStream = memoryStream;
            fileName = input.FileName;
        }

        try
        {
            // Use domain service to update (handles file replacement if needed)
            await _manager.UpdateAsync(offer, input.Title, fileStream, fileName);
            await _repository.UpdateAsync(offer);
        }
        finally
        {
            memoryStream?.Dispose();
        }

        return ObjectMapper.Map<HealthcareOffer, HealthcareOfferDto>(offer);
    }

    /// <summary>
    /// Deletes a healthcare offer and its associated file.
    /// Clean Code: Domain service handles blob cleanup.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var offer = await _repository.GetAsync(id);

        // Delete file from blob storage (via domain service)
        await _manager.DeleteAsync(offer);

        // Delete entity
        await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// Downloads a healthcare offer file (employee view).
    /// Returns file stream.
    /// </summary>
    public async Task<Stream> DownloadAsync(Guid id)
    {
        var offer = await _repository.GetAsync(id);
        var stream = await _manager.GetFileStreamAsync(offer);
        return stream;
    }
}
