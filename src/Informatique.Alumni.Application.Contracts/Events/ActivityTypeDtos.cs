using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

public class ActivityTypeDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateActivityTypeDto
{
    [Required]
    [MaxLength(EventConsts.MaxTitleLength)] // Assuming you have this constant
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(EventConsts.MaxTitleLength)]
    public string NameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateActivityTypeDto
{
    [Required]
    [MaxLength(EventConsts.MaxTitleLength)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(EventConsts.MaxTitleLength)]
    public string NameEn { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class ActivityTypeFilterDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
