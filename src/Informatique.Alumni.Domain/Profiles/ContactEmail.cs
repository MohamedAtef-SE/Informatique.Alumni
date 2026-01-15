using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Contact email value object with primary flag.
/// Business Rule: Alumni can have multiple emails, one marked as primary.
/// </summary>
public class ContactEmail : Entity<Guid>
{
    public Guid AlumniProfileId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    private ContactEmail() { }

    public ContactEmail(Guid id, Guid alumniProfileId, string email, bool isPrimary = false)
        : base(id)
    {
        AlumniProfileId = alumniProfileId;
        Email = email;
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

    public void UpdateEmail(string email)
    {
        Email = email;
    }
}
