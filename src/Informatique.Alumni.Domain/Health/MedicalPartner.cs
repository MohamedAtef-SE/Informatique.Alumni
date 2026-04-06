using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Health;

public class MedicalPartner : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MedicalPartnerType Type { get; set; } // Legacy - Will be derived from MedicalCategory.BaseType
    public Guid? MedicalCategoryId { get; set; }
    public virtual MedicalCategory? MedicalCategory { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? Website { get; set; }
    
    // New Fields for Premium Look
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Category { get; set; } // Legacy - Detailed category e.g. "Dental", "Derma"
    public string? Email { get; set; }
    public string? HotlineNumber { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = true;

    public ICollection<MedicalOffer> Offers { get; private set; }

    private MedicalPartner() 
    {
        Name = string.Empty;
        Address = string.Empty;
        ContactNumber = string.Empty;
        Offers = new List<MedicalOffer>();
    }

    public MedicalPartner(Guid id, string name, MedicalPartnerType type, string address, string contactNumber)
        : base(id)
    {
        Name = name;
        Type = type;
        Address = address;
        ContactNumber = contactNumber;
        Offers = new List<MedicalOffer>();
    }

    public void SetPremiumDetails(string? logoUrl, string? city, string? region, string? category, string? email, string? hotline, bool isVerified = false)
    {
        LogoUrl = logoUrl;
        City = city;
        Region = region;
        Category = category;
        Email = email;
        HotlineNumber = hotline;
        IsVerified = isVerified;
    }

    public void AddOffer(Guid offerId, string title, string detail, string? discountCode = null, int? discountPercentage = null)
    {
        Offers.Add(new MedicalOffer(offerId, Id, title, detail, discountCode) { DiscountPercentage = discountPercentage });
    }
}
