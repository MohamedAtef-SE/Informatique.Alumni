using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Contact phone (landline) value object.
/// Business Rule: Alumni can have multiple landline phone numbers.
/// </summary>
public class ContactPhone : Entity<Guid>
{
    public Guid AlumniProfileId { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? Label { get; private set; } // e.g., "Home", "Work"

    private ContactPhone() { }

    public ContactPhone(Guid id, Guid alumniProfileId, string phoneNumber, string? label = null)
        : base(id)
    {
        AlumniProfileId = alumniProfileId;
        PhoneNumber = phoneNumber;
        Label = label;
    }

    public void UpdatePhone(string phoneNumber, string? label = null)
    {
        PhoneNumber = phoneNumber;
        Label = label;
    }
}
