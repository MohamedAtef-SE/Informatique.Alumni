using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Contact mobile value object with primary flag.
/// Business Rule: Alumni can have multiple mobiles, one marked as primary.
/// </summary>
public class ContactMobile : Entity<Guid>
{
    public Guid AlumniProfileId { get; private set; }
    public string MobileNumber { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    private ContactMobile() { }

    public ContactMobile(Guid id, Guid alumniProfileId, string mobileNumber, bool isPrimary = false)
        : base(id)
    {
        AlumniProfileId = alumniProfileId;
        MobileNumber = mobileNumber;
        IsPrimary = isPrimary;
    }

    public void MarkAsPrimary()
    {
        IsPrimary = true;
    }

    public void UnmarkAsPrimary()
    {
        IsPrimary = false;
    }

    public void UpdateNumber(string mobileNumber)
    {
        MobileNumber = mobileNumber;
    }
}
