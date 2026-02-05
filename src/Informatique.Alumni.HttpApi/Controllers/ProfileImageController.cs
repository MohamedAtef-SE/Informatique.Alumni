using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;
using Informatique.Alumni.Profiles;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Controllers;

[Route("api/app/profile/image")]
public class ProfileImageController : AbpController
{
    private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;

    public ProfileImageController(
        IBlobContainer<ProfilePictureContainer> blobContainer,
        IRepository<AlumniProfile, Guid> profileRepository)
    {
        _blobContainer = blobContainer;
        _profileRepository = profileRepository;
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> GetAsync(string blobName)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            return BadRequest();
        }

        try 
        {
            // Check if blob exists
            var exists = await _blobContainer.ExistsAsync(blobName);
            if (!exists)
            {
                return NotFound();
            }

            var stream = await _blobContainer.GetAllBytesOrNullAsync(blobName);
            if (stream == null)
            {
                return NotFound();
            }

            // Determine content type based on extension
            var contentType = "application/octet-stream";
            var extension = System.IO.Path.GetExtension(blobName).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
                case ".webp":
                    contentType = "image/webp";
                    break;
            }

            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error serving profile image: {BlobName}", blobName);
            return StatusCode(500);
        }
    }

    [HttpPost("upload")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<string>> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or missing.");
        }

        try
        {
            // Get the current user's ID from ABP's CurrentUser
            var userId = CurrentUser.GetId();
            
            // Generate a unique blob name
            var blobName = $"{userId}/{Guid.NewGuid()}{System.IO.Path.GetExtension(file.FileName)}";
            
            // Save to blob storage
            await using var stream = file.OpenReadStream();
            await _blobContainer.SaveAsync(blobName, stream);
            
            // Construct the photo URL
            var photoUrl = $"/api/app/profile/image?blobName={blobName}";
            
            // Update the profile with the new photo URL
            var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                profile.SetPhotoUrl(photoUrl);
                await _profileRepository.UpdateAsync(profile);
            }
            
            Logger.LogInformation("Profile photo uploaded successfully: {BlobName} for user {UserId}", blobName, userId);
            
            return Ok(photoUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading profile photo");
            return StatusCode(500, "Failed to upload photo.");
        }
    }
}
