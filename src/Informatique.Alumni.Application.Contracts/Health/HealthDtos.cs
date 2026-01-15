using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Health;

public class MedicalPartnerDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MedicalPartnerType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public List<MedicalOfferDto> Offers { get; set; } = new();
}

public class MedicalOfferDto : FullAuditedEntityDto<Guid>
{
    public Guid MedicalPartnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DiscountCode { get; set; } // Gated field
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
}

public class CreateUpdateMedicalOfferDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DiscountCode { get; set; }
}
