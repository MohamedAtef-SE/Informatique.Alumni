using System;

namespace Informatique.Alumni.Health;

/// <summary>
/// DTO for healthcare offer list display (graduate view).
/// Clean Code: Read-only properties for immutability.
/// </summary>
public class HealthcareOfferListDto
{
    /// <summary>
    /// Unique identifier of the healthcare offer.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Title of the healthcare offer.
    /// Example: "Dental Clinic 20% Discount"
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// URL for downloading the offer file.
    /// Can be API endpoint or presigned blob URL.
    /// </summary>
    public string DownloadUrl { get; set; } = null!;
    
    /// <summary>
    /// Date when the offer was uploaded by the administrator.
    /// </summary>
    public DateTime UploadDate { get; set; }
}
