using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Health;

public class MedicalPartner : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MedicalPartnerType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MedicalOffer> Offers { get; private set; }

    private MedicalPartner() 
    {
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

    public void AddOffer(Guid offerId, string title, string detail, string? discountCode = null)
    {
        Offers.Add(new MedicalOffer(offerId, Id, title, detail, discountCode));
    }
}
