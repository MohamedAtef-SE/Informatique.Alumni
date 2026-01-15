using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Certificates;

/// <summary>
/// Certificate Definition aggregate root with localized names and degree type scoping.
/// Business Rules: Names must be unique per DegreeType, supports Arabic/English localization.
/// </summary>
public class CertificateDefinition : FullAuditedAggregateRoot<Guid>
{
    // Localized names (Business Rule #1: Mandatory, Searchable)
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    
    // Financial (Business Rule #1: Mandatory, supports decimals)
    public decimal Fee { get; private set; }
    
    // Degree Type Scoping (Business Rule #1: Mandatory for uniqueness validation)
    public DegreeType DegreeType { get; private set; }
    
    // Optional fields
    public string? Description { get; private set; }
    public string? RequiredDocuments { get; private set; } // Rich HTML content
    public bool IsActive { get; private set; }

    private CertificateDefinition() { }

    public CertificateDefinition(
        Guid id,
        string nameAr,
        string nameEn,
        decimal fee,
        DegreeType degreeType,
        string? description = null,
        string? requiredDocuments = null)
        : base(id)
    {
        SetNames(nameAr, nameEn);
        SetFee(fee);
        DegreeType = degreeType;
        Description = description;
        RequiredDocuments = requiredDocuments;
        IsActive = true;
    }

    public void Update(
        string nameAr,
        string nameEn,
        decimal fee,
        DegreeType degreeType,
        string? description = null,
        string? requiredDocuments = null)
    {
        SetNames(nameAr, nameEn);
        SetFee(fee);
        DegreeType = degreeType;
        Description = description;
        RequiredDocuments = requiredDocuments;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SetNames(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), CertificateConsts.MaxNameLength);
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), CertificateConsts.MaxNameLength);
    }

    private void SetFee(decimal fee)
    {
        if (fee < 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Certificate.InvalidFee)
                .WithData("Fee", fee);
        }
        
        Fee = fee;
    }
}
