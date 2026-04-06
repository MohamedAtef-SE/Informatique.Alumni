using System;
using System.IO;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;

namespace Informatique.Alumni.Controllers;

[Authorize]
[Route("api/app/file")]
public class FileController : AbpController
{
    private readonly IBlobContainer<DocumentContainer> _blobContainer;

    public FileController(IBlobContainer<DocumentContainer> blobContainer)
    {
        _blobContainer = blobContainer;
    }

    [HttpGet]
    [Route("download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadAsync([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest();

        // Decode URL-encoded blob name (handles the '/' inside the GUID path)
        var blobName = Uri.UnescapeDataString(name);

        var bytes = await _blobContainer.GetAllBytesOrNullAsync(blobName);
        if (bytes == null)
            return NotFound();

        var extension = Path.GetExtension(blobName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf"  => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"  => "image/png",
            _       => "application/octet-stream"
        };

        // Return inline so the browser opens (views) it rather than downloading it
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{Path.GetFileName(blobName)}\"";
        return File(bytes, contentType);
    }
}
