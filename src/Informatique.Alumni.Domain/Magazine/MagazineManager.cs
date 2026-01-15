using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
using Volo.Abp.Timing;
using Informatique.Alumni.BlobContainers;

namespace Informatique.Alumni.Magazine;

public class MagazineManager : DomainService
{
    private readonly IBlobContainer<MagazineBlobContainer> _blobContainer;
    private readonly IClock _clock;

    public MagazineManager(
        IBlobContainer<MagazineBlobContainer> blobContainer,
        IClock clock)
    {
        _blobContainer = blobContainer;
        _clock = clock;
    }

    public async Task<Magazine> CreateAsync(
        string title,
        DateTime? issueDate,
        Stream fileStream,
        string fileName)
    {
        Check.NotNullOrWhiteSpace(title, nameof(title));
        Check.NotNull(fileStream, nameof(fileStream));
        Check.NotNullOrWhiteSpace(fileName, nameof(fileName));

        // 1. Validate File Extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!MagazineConsts.AllowedFileExtensions.Contains(extension))
        {
            throw new BusinessException("Magazine:InvalidFileFormat")
                .WithData("AllowedExtensions", string.Join(", ", MagazineConsts.AllowedFileExtensions));
        }

        // Determine File Type
        var fileType = extension == ".pdf" ? MagazineFileType.Pdf : MagazineFileType.Image;

        // 2. Upload to Blob Storage
        var blobName = $"{GuidGenerator.Create()}{extension}";
        await _blobContainer.SaveAsync(blobName, fileStream);

        // 3. Create Entity
        var date = issueDate ?? _clock.Now;
        
        return new Magazine(
            GuidGenerator.Create(),
            title,
            date,
            blobName,
            fileType
        );
    }

    public async Task UpdateAsync(
        Magazine magazine,
        string title,
        Stream? newFileStream = null,
        string? newFileName = null)
    {
        Check.NotNull(magazine, nameof(magazine));
        Check.NotNullOrWhiteSpace(title, nameof(title));

        // Update Title
        magazine.SetTitle(title);

        // Update File if provided
        if (newFileStream != null && !string.IsNullOrWhiteSpace(newFileName))
        {
            // 1. Validate New File
            var extension = Path.GetExtension(newFileName).ToLowerInvariant();
            if (!MagazineConsts.AllowedFileExtensions.Contains(extension))
            {
                throw new BusinessException("Magazine:InvalidFileFormat")
                    .WithData("AllowedExtensions", string.Join(", ", MagazineConsts.AllowedFileExtensions));
            }

            var fileType = extension == ".pdf" ? MagazineFileType.Pdf : MagazineFileType.Image;

            // 2. Delete Old Blob
            if (!string.IsNullOrEmpty(magazine.FileBlobName))
            {
                await _blobContainer.DeleteAsync(magazine.FileBlobName);
            }

            // 3. Save New Blob
            var blobName = $"{GuidGenerator.Create()}{extension}";
            await _blobContainer.SaveAsync(blobName, newFileStream);

            // 4. Update Entity
            magazine.SetFile(blobName, fileType);
        }
    }
}
