using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Certificates;

/// <summary>
/// Output DTO for Certificate Definition with localized names.
/// </summary>
public class CertificateDefinitionDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public DegreeType DegreeType { get; set; }
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Input DTO for creating Certificate Definition.
/// Business Rules: NameAr and NameEn must be unique per DegreeType.
/// </summary>
public class CreateCertificateDefinitionDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public DegreeType DegreeType { get; set; }
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
}

/// <summary>
/// Input DTO for updating Certificate Definition.
/// Business Rules: NameAr and NameEn must be unique per DegreeType (excluding self).
/// </summary>
public class UpdateCertificateDefinitionDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public DegreeType DegreeType { get; set; }
    public string? Description { get; set; }
    public string? RequiredDocuments { get; set; }
    public bool IsActive { get; set; }
}
