using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Timing;

namespace Informatique.Alumni.Health;

/// <summary>
/// Entity for healthcare offer files managed by employees.
/// DDD: Encapsulation with private setters, auto-date generation.
/// Clean Code: Immutable UploadDate, focused validation.
/// </summary>
public class HealthcareOffer : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// Title of the healthcare offer.
    /// Example: "Dental Clinic 20% Discount", "Eye Care Package"
    /// </summary>
    public string Title { get; private set; } = null!;
    
    /// <summary>
    /// Blob storage name for the uploaded file.
    /// Format: {Guid}_{originalFileName}
    /// </summary>
    public string FileBlobName { get; private set; } = null!;
    
    /// <summary>
    /// Date and time when the offer was uploaded.
    /// Business Rule: Auto-generated at creation, never changes.
    /// Clean Code: Immutable - set once in constructor.
    /// </summary>
    public DateTime UploadDate { get; private set; }

    // EF Core constructor
    private HealthcareOffer()
    {
    }

    /// <summary>
    /// Constructor for creating a new healthcare offer.
    /// Business Rule: UploadDate is auto-set to current time.
    /// SRP: Initialization only, file handling in Manager.
    /// </summary>
    public HealthcareOffer(
        Guid id,
        string title,
        string fileBlobName,
        IClock clock) : base(id)
    {
        SetTitle(title);
        FileBlobName = Check.NotNullOrWhiteSpace(fileBlobName, nameof(fileBlobName), 512);
        UploadDate = clock.Now; // Business Rule: Auto-generated, immutable
    }

    /// <summary>
    /// Updates the offer title.
    /// Business Rule: Employee can edit title anytime.
    /// Clean Code: Focused validation method.
    /// </summary>
    public void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), HealthConsts.MaxOfferTitleLength);
    }

    /// <summary>
    /// Replaces the file with a new one.
    /// Business Rule: Employee can replace file, but UploadDate stays the same.
    /// Note: Old file deletion happens in HealthcareOfferManager.
    /// </summary>
    public void ReplaceFile(string newFileBlobName)
    {
        FileBlobName = Check.NotNullOrWhiteSpace(newFileBlobName, nameof(newFileBlobName), 512);
        // Note: UploadDate intentionally NOT changed - reflects original upload
    }
}
