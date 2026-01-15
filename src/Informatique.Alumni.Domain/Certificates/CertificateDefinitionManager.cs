using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Certificates;

/// <summary>
/// Domain service for managing Certificate Definitions with business rule enforcement.
/// Business Rules: Scoped uniqueness per DegreeType, prevent deletion if in use.
/// </summary>
public class CertificateDefinitionManager : DomainService
{
    private readonly IRepository<CertificateDefinition, Guid> _definitionRepository;
    private readonly IRepository<CertificateRequest, Guid> _requestRepository;

    public CertificateDefinitionManager(
        IRepository<CertificateDefinition, Guid> definitionRepository,
        IRepository<CertificateRequest, Guid> requestRepository)
    {
        _definitionRepository = definitionRepository;
        _requestRepository = requestRepository;
    }

    /// <summary>
    /// Create certificate definition with scoped uniqueness validation.
    /// Business Rule #2: Names must be unique per DegreeType.
    /// </summary>
    public async Task<CertificateDefinition> CreateAsync(
        string nameAr,
        string nameEn,
        decimal fee,
        DegreeType degreeType,
        string? description = null,
        string? requiredDocuments = null)
    {
        // Business Rule #2: Scoped Uniqueness Validation
        await CheckDuplicateNameAsync(nameAr, nameEn, degreeType);

        var definition = new CertificateDefinition(
            GuidGenerator.Create(),
            nameAr,
            nameEn,
            fee,
            degreeType,
            description,
            requiredDocuments
        );

        return await _definitionRepository.InsertAsync(definition);
    }

    /// <summary>
    /// Update certificate definition with scoped uniqueness validation.
    /// Business Rule #2: Names must be unique per DegreeType (excluding self).
    /// </summary>
    public async Task UpdateAsync(
        CertificateDefinition definition,
        string nameAr,
        string nameEn,
        decimal fee,
        DegreeType degreeType,
        string? description = null,
        string? requiredDocuments = null)
    {
        // Business Rule #2: Scoped Uniqueness Validation (exclude current record)
        await CheckDuplicateNameAsync(nameAr, nameEn, degreeType, definition.Id);

        definition.Update(nameAr, nameEn, fee, degreeType, description, requiredDocuments);
        await _definitionRepository.UpdateAsync(definition);
    }

    /// <summary>
    /// Delete certificate definition with usage validation.
    /// Business Rule #3: Cannot delete if linked to any existing requests.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        // Business Rule #3: Referential Integrity Check
        var isUsed = await IsDefinitionUsedAsync(id);
        
        if (isUsed)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Certificate.CannotDeleteUsedDefinition)
                .WithData("CertificateDefinitionId", id);
        }

        await _definitionRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Check if certificate definition name is duplicate for the given degree type.
    /// Scoped uniqueness: Can have same name for different degree types.
    /// </summary>
    private async Task CheckDuplicateNameAsync(
        string nameAr,
        string nameEn,
        DegreeType degreeType,
        Guid? excludeId = null)
    {
        var queryable = await _definitionRepository.GetQueryableAsync();

        // Check Arabic name duplication
        var duplicateAr = queryable.Where(x =>
            x.NameAr == nameAr &&
            x.DegreeType == degreeType &&
            (excludeId == null || x.Id != excludeId));

        if (await AsyncExecuter.AnyAsync(duplicateAr))
        {
            throw new BusinessException(AlumniDomainErrorCodes.Certificate.DuplicateNameAr)
                .WithData("NameAr", nameAr)
                .WithData("DegreeType", degreeType);
        }

        // Check English name duplication
        var duplicateEn = queryable.Where(x =>
            x.NameEn == nameEn &&
            x.DegreeType == degreeType &&
            (excludeId == null || x.Id != excludeId));

        if (await AsyncExecuter.AnyAsync(duplicateEn))
        {
            throw new BusinessException(AlumniDomainErrorCodes.Certificate.DuplicateNameEn)
                .WithData("NameEn", nameEn)
                .WithData("DegreeType", degreeType);
        }
    }

    /// <summary>
    /// Check if certificate definition is used in any request items.
    /// </summary>
    private async Task<bool> IsDefinitionUsedAsync(Guid definitionId)
    {
        var queryable = await _requestRepository.GetQueryableAsync();
        
        // Check if any request has items with this certificate definition
        var usageQuery = queryable.Where(r =>
            r.Items.Any(i => i.CertificateDefinitionId == definitionId));

        return await AsyncExecuter.AnyAsync(usageQuery);
    }
}
