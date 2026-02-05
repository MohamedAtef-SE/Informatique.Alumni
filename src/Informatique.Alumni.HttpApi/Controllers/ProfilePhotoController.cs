using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace Informatique.Alumni.Controllers;

/// <summary>
/// Dedicated controller for profile photo serving.
/// ABP conventional controllers don't correctly pick up catch-all route parameters on app service methods.
/// </summary>
[Route("api/app/alumni-profile")]
public class ProfilePhotoController : AbpController
{
    private readonly IAlumniProfileAppService _profileAppService;

    public ProfilePhotoController(IAlumniProfileAppService profileAppService)
    {
        _profileAppService = profileAppService;
    }

    /// <summary>
    /// Serve profile photo.
    /// Route: GET /api/app/alumni-profile/photo/{profileId}/{fileName}
    /// [AllowAnonymous] enables basic <img> tags to load without Bearer tokens.
    /// </summary>
    [HttpGet("photo/{profileId}/{fileName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhoto(string profileId, string fileName)
    {
        var blobName = $"{profileId}/{fileName}";
        var content = await _profileAppService.GetPhotoAsync(blobName);
        
        return File(content.GetStream(), content.ContentType ?? "application/octet-stream", content.FileName);
    }
}
