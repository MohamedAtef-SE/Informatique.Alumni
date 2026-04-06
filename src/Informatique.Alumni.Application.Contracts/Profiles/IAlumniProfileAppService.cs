using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Microsoft.AspNetCore.Http;

namespace Informatique.Alumni.Profiles;

public interface IAlumniProfileAppService : IApplicationService
{
    // Graduate User - My Profile (Core)
    Task<AlumniMyProfileDto> GetMyProfileAsync();
    Task<AlumniMyProfileDto> UpdateMyProfileAsync(UpdateMyProfileDto input);
    
    // Photo Upload/Management
    Task<string> UploadPhotoAsync(IFormFile file);
    
    // Wallet Management
    Task<AlumniMyProfileDto> TopUpWalletAsync(decimal amount);
    Task<List<WalletActivityDto>> GetWalletActivityAsync();
}
