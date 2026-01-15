using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CareerServiceTypeDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateUpdateCareerServiceTypeDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CareerServiceTypeFilterDto : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}
