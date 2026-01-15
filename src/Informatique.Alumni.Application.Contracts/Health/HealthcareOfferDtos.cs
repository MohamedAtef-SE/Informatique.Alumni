using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Health;

/// <summary>
/// DTO for healthcare offer (employee view - full details).
/// </summary>
public class HealthcareOfferDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; } = null!;
    public string FileBlobName { get; set; } = null!;
    public DateTime UploadDate { get; set; }
}

/// <summary>
/// Input DTO for creating a new healthcare offer.
/// </summary>
public class CreateHealthcareOfferDto
{
    [Required]
    [StringLength(HealthConsts.MaxOfferTitleLength)]
    public string Title { get; set; } = null!;

    [Required]
    public string FileName { get; set; } = null!;

    [Required]
    public byte[] FileContent { get; set; } = null!;
}

/// <summary>
/// Input DTO for updating a healthcare offer.
/// File content is optional.
/// </summary>
public class UpdateHealthcareOfferDto
{
    [Required]
    [StringLength(HealthConsts.MaxOfferTitleLength)]
    public string Title { get; set; } = null!;

    public string? FileName { get; set; }

    public byte[]? FileContent { get; set; }
}

/// <summary>
/// Input for getting healthcare offers with pagination and filtering.
/// </summary>
public class GetHealthcareOffersInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Search filter for offer titles.
    /// </summary>
    public string? Filter { get; set; }
}
