using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
using Volo.Abp.Timing;
using Informatique.Alumni.BlobContainers;

namespace Informatique.Alumni.Health;

/// <summary>
/// Domain service for managing healthcare offer files.
/// DDD: Orchestrates file validation, blob storage, and entity creation.
/// SRP: Each method has single responsibility (< 15 lines).
/// Clean Code: Clear naming, small focused methods.
/// </summary>
public class HealthcareOfferManager : DomainService
{
    private readonly IBlobContainer<HealthcareBlobContainer> _blobContainer;
    private readonly IClock _clock;

    // Allowed file extensions (Business Rule)
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

    public HealthcareOfferManager(
        IBlobContainer<HealthcareBlobContainer> blobContainer,
        IClock clock)
    {
        _blobContainer = blobContainer;
        _clock = clock;
    }

    /// <summary>
    /// Creates a new healthcare offer with file upload.
    /// Business Rules:
    /// 1. Validate file extension (PDF/JPG/JPEG/PNG only)
    /// 2. Save file to blob storage
    /// 3. Auto-set UploadDate to current time
    /// SRP: Orchestrates validation and creation.
    /// </summary>
    public async Task<HealthcareOffer> CreateAsync(
        string title,
        Stream fileStream,
        string fileName)
    {
        // 1. Validate file extension BEFORE uploading
        ValidateFileExtension(fileName);

        // 2. Generate unique blob name
        var blobName = GenerateBlobName(fileName);

        // 3. Save to blob storage
        await _blobContainer.SaveAsync(blobName, fileStream, overrideExisting: true);

        // 4. Create entity (UploadDate auto-set via IClock)
        var offer = new HealthcareOffer(
            GuidGenerator.Create(),
            title,
            blobName,
            _clock
        );

        return offer;
    }

    /// <summary>
    /// Updates an existing healthcare offer.
    /// Business Rules:
    /// 1. Title can always be updated
    /// 2. File can be replaced (old file is deleted)
    /// 3. UploadDate never changes (immutable)
    /// Clean Code: Optional file replacement pattern.
    /// </summary>
    public async Task UpdateAsync(
        HealthcareOffer offer,
        string newTitle,
        Stream? newFileStream = null,
        string? newFileName = null)
    {
        Check.NotNull(offer, nameof(offer));

        // 1. Update title
        offer.SetTitle(newTitle);

        // 2. Replace file if provided
        if (newFileStream != null && !string.IsNullOrWhiteSpace(newFileName))
        {
            // Validate new file extension
            ValidateFileExtension(newFileName);

            // Delete old file from blob storage
            await _blobContainer.DeleteAsync(offer.FileBlobName);

            // Upload new file
            var newBlobName = GenerateBlobName(newFileName);
            await _blobContainer.SaveAsync(newBlobName, newFileStream, overrideExisting: true);

            // Update entity
            offer.ReplaceFile(newBlobName);
        }
    }

    /// <summary>
    /// Deletes a healthcare offer and its associated file.
    /// Clean Code: Cleanup responsibility in domain service.
    /// </summary>
    public async Task DeleteAsync(HealthcareOffer offer)
    {
        Check.NotNull(offer, nameof(offer));

        // Delete file from blob storage
        await _blobContainer.DeleteAsync(offer.FileBlobName);
    }

    /// <summary>
    /// Gets the file stream for viewing.
    /// Business Rule: Employee can view files after uploading.
    /// </summary>
    public async Task<Stream> GetFileStreamAsync(HealthcareOffer offer)
    {
        Check.NotNull(offer, nameof(offer));
        return await _blobContainer.GetAsync(offer.FileBlobName);
    }

    // ============ VALIDATION METHODS ============

    /// <summary>
    /// Validates file extension against allowed formats.
    /// Business Rule: Only PDF, JPG, JPEG, PNG allowed.
    /// Clean Code: Case-insensitive comparison, clear error message.
    /// </summary>
    private void ValidateFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new BusinessException("Healthcare:InvalidFileFormat")
                .WithData("FileName", fileName)
                .WithData("Extension", extension)
                .WithData("AllowedFormats", string.Join(", ", AllowedExtensions));
        }
    }

    /// <summary>
    /// Generates a unique blob name to avoid conflicts.
    /// Format: {Guid}_{sanitizedOriginalFileName}
    /// Clean Code: Prevents file name collisions.
    /// </summary>
    private string GenerateBlobName(string originalFileName)
    {
        var sanitizedFileName = Path.GetFileName(originalFileName); // Remove path
        var uniqueId = Guid.NewGuid().ToString("N"); // No hyphens
        return $"{uniqueId}_{sanitizedFileName}";
    }
}
