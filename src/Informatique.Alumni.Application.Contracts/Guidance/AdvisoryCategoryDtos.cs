using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Guidance;

public class AdvisoryCategoryDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUpdateAdvisoryCategoryDto
{
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdvisoryCategoryFilterDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
