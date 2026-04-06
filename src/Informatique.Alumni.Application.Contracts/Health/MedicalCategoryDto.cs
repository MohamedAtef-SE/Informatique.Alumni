using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Health;

public class MedicalCategoryDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public MedicalPartnerType BaseType { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUpdateMedicalCategoryDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public MedicalPartnerType BaseType { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GetMedicalCategoriesInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
}
