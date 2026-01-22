using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Benefits;

public class AcademicGrantDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public string Type { get; set; }
    public double Percentage { get; set; }
}

public class CreateUpdateAcademicGrantDto
{
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public string Type { get; set; }
    public double Percentage { get; set; }
}

public class CommercialDiscountDto : FullAuditedEntityDto<Guid>
{
    public Guid CategoryId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public string? PromoCode { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class CreateUpdateCommercialDiscountDto
{
    public Guid CategoryId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public string? PromoCode { get; set; }
    public DateTime ValidUntil { get; set; }
}
