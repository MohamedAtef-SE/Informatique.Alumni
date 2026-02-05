using System;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Certificates;

/// <summary>
/// Application service for Certificate Definition management.
/// Uses CertificateDefinitionManager for business rule enforcement.
/// </summary>
[Authorize(AlumniPermissions.Certificates.ManageDefinitions)]
public class CertificateDefinitionAppService : ApplicationService, ICertificateDefinitionAppService
{
    private readonly IRepository<CertificateDefinition, Guid> _repository;
    private readonly CertificateDefinitionManager _definitionManager;
    private readonly AlumniApplicationMappers _mappers;

    public CertificateDefinitionAppService(
        IRepository<CertificateDefinition, Guid> repository,
        CertificateDefinitionManager definitionManager,
        AlumniApplicationMappers mappers)
    {
        _repository = repository;
        _definitionManager = definitionManager;
        _mappers = mappers;
    }

    public async Task<CertificateDefinitionDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return _mappers.MapToDto(entity);
    }

    public async Task<PagedResultDto<CertificateDefinitionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var totalCount = await _repository.GetCountAsync();
        var entities = await _repository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, input.Sorting ?? "NameEn");
        
        var dtos = _mappers.MapToDtos(entities);
        return new PagedResultDto<CertificateDefinitionDto>(totalCount, dtos);
    }

    /// <summary>
    /// Create certificate definition with scoped uniqueness validation.
    /// Uses domain manager to enforce: Names must be unique per DegreeType.
    /// </summary>
    public async Task<CertificateDefinitionDto> CreateAsync(CreateCertificateDefinitionDto input)
    {
        var entity = await _definitionManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.Fee,
            input.DegreeType,
            input.Description,
            input.RequiredDocuments
        );

        return _mappers.MapToDto(entity);
    }

    /// <summary>
    /// Update certificate definition with scoped uniqueness validation.
    /// Uses domain manager to enforce: Names must be unique per DegreeType (excluding self).
    /// </summary>
    public async Task<CertificateDefinitionDto> UpdateAsync(Guid id, UpdateCertificateDefinitionDto input)
    {
        var entity = await _repository.GetAsync(id);

        await _definitionManager.UpdateAsync(
            entity,
            input.NameAr,
            input.NameEn,
            input.Fee,
            input.DegreeType,
            input.Description,
            input.RequiredDocuments
        );

        return _mappers.MapToDto(entity);
    }

    /// <summary>
    /// Delete certificate definition with referential integrity check.
    /// Uses domain manager to prevent deletion if certificate is in use.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await _definitionManager.DeleteAsync(id);
    }
    /// <summary>
    /// Public/Alumni endpoint to get available certificate definitions for connection.
    /// </summary>
    [Authorize]
    public async Task<ListResultDto<CertificateDefinitionDto>> GetAvailableAsync()
    {
        var entities = await _repository.GetListAsync();
        // Optional: Filter by active/available if such a property exists
        var dtos = _mappers.MapToDtos(entities);
        return new ListResultDto<CertificateDefinitionDto>(dtos);
    }
}
