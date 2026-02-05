using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Profiles;

public interface IAlumniProfileAppService : IApplicationService
{
    Task<AlumniProfileDto> GetMineAsync();
    Task<AlumniProfileDto> UpdateMineAsync(UpdateAlumniProfileDto input);
    
    Task<AlumniProfileDto> AddExperienceAsync(CreateUpdateExperienceDto input);
    Task<AlumniProfileDto> UpdateExperienceAsync(Guid id, CreateUpdateExperienceDto input);
    Task<AlumniProfileDto> RemoveExperienceAsync(Guid id);
    
    Task<AlumniProfileDto> AddEducationAsync(CreateUpdateEducationDto input);
    Task<AlumniProfileDto> UpdateEducationAsync(Guid id, CreateUpdateEducationDto input);
    Task<AlumniProfileDto> RemoveEducationAsync(Guid id);

    Task<AlumniProfileDto> GetByUserIdAsync(Guid userId);

    // Graduate User - My Profile (Phase 1)
    Task<AlumniMyProfileDto> GetMyProfileAsync();
    Task<AlumniMyProfileDto> UpdateMyProfileAsync(UpdateMyProfileDto input);
    
    // Photo Upload
    Task<string> UploadPhotoAsync(Microsoft.AspNetCore.Http.IFormFile file);
    
    // Photo Retrieval
    Task<Volo.Abp.Content.IRemoteStreamContent> GetPhotoAsync(string name);
}
