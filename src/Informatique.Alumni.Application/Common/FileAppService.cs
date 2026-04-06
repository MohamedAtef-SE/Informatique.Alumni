using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Users;

namespace Informatique.Alumni.Common;

[Authorize]
public class FileAppService : AlumniAppService, IFileAppService
{
    private readonly IBlobContainer<DocumentContainer> _blobContainer;

    public FileAppService(IBlobContainer<DocumentContainer> blobContainer)
    {
        _blobContainer = blobContainer;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("No file provided.");
        }

        // Validate file type (Images and PDFs)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Validate file size (max 10MB)
        const int maxSize = 10 * 1024 * 1024;
        if (file.Length > maxSize)
        {
            throw new UserFriendlyException("File size exceeds 10MB limit.");
        }

        // Generate blob name
        var blobName = $"{CurrentUser.GetId()}/{Guid.NewGuid()}{extension}";

        // Upload to blob storage
        using var stream = file.OpenReadStream();
        await _blobContainer.SaveAsync(blobName, stream, overrideExisting: true);

        return $"/api/app/file/download?name={Uri.EscapeDataString(blobName)}";
    }
}

