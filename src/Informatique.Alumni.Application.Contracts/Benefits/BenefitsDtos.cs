
using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Benefits;

public class AcademicDiscountDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public AcademicDiscountType Type { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class DiscountOfferListDto : EntityDto<Guid>
{
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class AcademicDiscountFilterDto
{
    public AcademicDiscountType? Type { get; set; }
}
