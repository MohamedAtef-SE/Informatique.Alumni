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
    public async Task<IActionResult> DownloadAsync(string name)
    {
        var bytes = await _blobContainer.GetAllBytesOrNullAsync(name);
        if (bytes == null)
        {
            return NotFound();
        }

        var contentType = "application/octet-stream";
        if (name.EndsWith(".pdf")) contentType = "application/pdf";
        else if (name.EndsWith(".jpg") || name.EndsWith(".jpeg")) contentType = "image/jpeg";
        else if (name.EndsWith(".png")) contentType = "image/png";

        return File(bytes, contentType, name);
    }
}
