using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Health;

public class MedicalPartnerDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? MedicalCategoryId { get; set; }
    public string? MedicalCategoryName { get; set; }
    public MedicalPartnerType? MedicalCategoryBaseType { get; set; }

    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Category { get; set; }
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? HotlineNumber { get; set; }
    public List<MedicalOfferDto> Offers { get; set; } = new();
}

public class MedicalOfferDto : FullAuditedEntityDto<Guid>
{
    public Guid MedicalPartnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DiscountCode { get; set; } // Gated field
    public int? DiscountPercentage { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUpdateMedicalPartnerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MedicalPartnerType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? Website { get; set; }
    
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Category { get; set; } // Legacy
    public string? Email { get; set; }
    public string? HotlineNumber { get; set; }
    public Guid? MedicalCategoryId { get; set; }
    public bool IsVerified { get; set; }
}

public class CreateUpdateMedicalOfferDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DiscountCode { get; set; }
    public int? DiscountPercentage { get; set; }
}
