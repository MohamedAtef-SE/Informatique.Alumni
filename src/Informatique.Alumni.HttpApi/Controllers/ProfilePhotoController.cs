using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Controllers
{
    [Route("api/profile-photo")]
    public class ProfilePhotoController : AbpController
    {
        private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;

        public ProfilePhotoController(IBlobContainer<ProfilePictureContainer> blobContainer)
        {
            _blobContainer = blobContainer;
        }

        [HttpGet]
        [Route("{*name}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NotFound();
            }

            var exists = await _blobContainer.ExistsAsync(name);
            if (!exists)
            {
                return NotFound();
            }

            var stream = await _blobContainer.GetAsync(name);
            
            var contentType = "application/octet-stream";
            var extension = System.IO.Path.GetExtension(name)?.ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg": contentType = "image/jpeg"; break;
                case ".png": contentType = "image/png"; break;
                case ".gif": contentType = "image/gif"; break;
                case ".webp": contentType = "image/webp"; break;
            }

            return File(stream, contentType);
        }
    }
}
