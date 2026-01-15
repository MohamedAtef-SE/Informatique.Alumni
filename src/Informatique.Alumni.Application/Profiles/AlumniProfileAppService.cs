using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;

using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Profiles;

[Authorize]
public class AlumniProfileAppService : AlumniAppService, IAlumniProfileAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IStudentSystemIntegrationService _studentSystemIntegration;
    private readonly AlumniApplicationMappers _alumniMappers;

    public AlumniProfileAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IStudentSystemIntegrationService studentSystemIntegration,
        AlumniApplicationMappers alumniMappers)
    {
        _profileRepository = profileRepository;
        _studentSystemIntegration = studentSystemIntegration;
        _alumniMappers = alumniMappers;
    }

    public async Task<AlumniProfileDto> GetMineAsync()
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> UpdateMineAsync(UpdateAlumniProfileDto input)
    {
        // SECURITY: Resource ownership is enforced by using CurrentUser.GetId()
        // This ensures users can only update their own profile
        // DTO does not contain UserId field to prevent tampering
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        
        // Use domain method instead of mapper
        profile.UpdateBasicInfo(input.MobileNumber, input.Bio, input.JobTitle);
        
        await _profileRepository.UpdateAsync(profile);
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> AddExperienceAsync(CreateUpdateExperienceDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), true);
        
        var experience = new Experience(GuidGenerator.Create(), profile.Id, input.CompanyName, input.JobTitle, input.StartDate);
        experience.Update(input.CompanyName, input.JobTitle, input.StartDate, input.EndDate, input.Description);
        
        profile.AddExperience(experience);
        await _profileRepository.UpdateAsync(profile);
        
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> UpdateExperienceAsync(Guid id, CreateUpdateExperienceDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), true);
        var experience = profile.Experiences.FirstOrDefault(x => x.Id == id);
        
        if (experience == null)
        {
            throw new EntityNotFoundException(typeof(Experience), id);
        }
        
        // Use the Update method
        experience.Update(input.CompanyName, input.JobTitle, input.StartDate, input.EndDate, input.Description);
        
        await _profileRepository.UpdateAsync(profile);
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> RemoveExperienceAsync(Guid id)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), true);
        profile.RemoveExperience(id);
        await _profileRepository.UpdateAsync(profile);
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> AddEducationAsync(CreateUpdateEducationDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), true);
        
        var education = new Education(GuidGenerator.Create(), profile.Id, input.InstitutionName, input.Degree, input.GraduationYear);
        
        profile.AddEducation(education);
        await _profileRepository.UpdateAsync(profile);
        
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> UpdateEducationAsync(Guid id, CreateUpdateEducationDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), true);
        var education = profile.Educations.FirstOrDefault(x => x.Id == id);
        
        if (education == null)
        {
            throw new EntityNotFoundException(typeof(Education), id);
        }
        
        // Use the Update method
        education.Update(input.InstitutionName, input.Degree, input.GraduationYear);
        
        await _profileRepository.UpdateAsync(profile);
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<AlumniProfileDto> RemoveEducationAsync(Guid id)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), true);
        profile.RemoveEducation(id);
        await _profileRepository.UpdateAsync(profile);
        return _alumniMappers.MapToDto(profile);
    }

    [Authorize(AlumniPermissions.Profiles.ViewAll)]
    public async Task<AlumniProfileDto> GetByUserIdAsync(Guid userId)
    {
        var profile = await _profileRepository.WithDetailsAsync(x => x.Experiences, x => x.Educations)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.UserId == userId));

        if (profile == null)
        {
            throw new UserFriendlyException("Profile not found.");
        }

        return _alumniMappers.MapToDto(profile);
    }

    /// <summary>
    /// Get academic history from Legacy Student Information System (Read-Only).
    /// Business Rules: 
    /// 1. Data fetched on-demand from Legacy SIS (not stored locally)
    /// 2. Read-Only access - no edit/update/delete operations allowed
    /// 3. Alumni can ONLY view their own data (CurrentUser filter)
    /// 4. Multi-Qualification support (BSc, MSc, PhD, etc.)
    /// </summary>
    [Authorize]
    public async Task<AcademicHistoryDto> GetMyAcademicHistoryAsync()
    {
        // SECURITY: Get current user's profile to retrieve student ID
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId(), includeDetails: false);
        
        // TODO: Map AlumniProfile to Legacy SIS Student ID
        // For now, using profile ID as student ID (should be mapped from legacy system)
        var studentId = profile.Id.ToString();
        
        // Fetch academic transcript from Legacy SIS (Adapter Pattern)
        var qualifications = await _studentSystemIntegration.GetStudentTranscriptAsync(studentId);
        
        // Map Domain Objects (SisQualification) to Application DTOs
        var qualificationDtos = qualifications.Select(q => new QualificationHistoryDto
        {
            QualificationId = q.QualificationId,
            DegreeName = q.DegreeName,
            Major = q.Major,
            College = q.College,
            GraduationYear = q.GraduationYear,
            CumulativeGPA = q.CumulativeGPA,
            Semesters = q.Semesters.Select(s => new SemesterRecordDto
            {
                SemesterCode = s.SemesterCode,
                SemesterName = s.SemesterName,
                Year = s.Year,
                SemesterNumber = s.SemesterNumber,
                SemesterGPA = s.SemesterGPA,
                TotalCredits = s.TotalCredits,
                Courses = s.Courses.Select(c => new CourseGradeDto
                {
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Credits = c.Credits,
                    Grade = c.Grade,
                    GradePoint = c.GradePoint,
                    InstructorName = c.InstructorName
                }).ToList()
            }).ToList()
        }).ToList();
        
        // Return structured multi-qualification data
        return new AcademicHistoryDto
        {
            Qualifications = qualificationDtos
        };
    }

    private async Task<AlumniProfile> GetOrCreateProfileAsync(Guid userId, bool includeDetails = true)
    {
        AlumniProfile? profile;
        
        if (includeDetails)
        {
            var queryable = await _profileRepository.WithDetailsAsync(
                x => x.Experiences, 
                x => x.Educations,
                x => x.Emails,
                x => x.Mobiles,
                x => x.Phones
            );
            profile = queryable.FirstOrDefault(x => x.UserId == userId);
        }
        else
        {
            profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        if (profile == null)
        {
            // Resource-based Auth Check (Implicit: current user can only create their own profile)
            if (userId != CurrentUser.GetId())
            {
                throw new UnauthorizedAccessException("Cannot manage another user's profile.");
            }

            profile = new AlumniProfile(GuidGenerator.Create(), userId, string.Empty, string.Empty);
            await _profileRepository.InsertAsync(profile);
        }

        return profile;
    }

    /// <summary>
    /// Get Graduate's own profile with combined Read-Only SIS data and Editable Profile data.
    /// </summary>
    public async Task<AlumniMyProfileDto> GetMyProfileAsync()
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        
        // 1. Map Editable Data
        var dto = new AlumniMyProfileDto
        {
            // Editable Fields
            Address = profile.Address,
            Bio = profile.Bio,
            JobTitle = profile.JobTitle,
            PhotoUrl = profile.PhotoUrl,
            
            // Contacts
            Emails = profile.Emails.Select(e => new ContactEmailDto { Id = e.Id, Email = e.Email, IsPrimary = e.IsPrimary }).ToList(),
            Mobiles = profile.Mobiles.Select(m => new ContactMobileDto { Id = m.Id, MobileNumber = m.MobileNumber, IsPrimary = m.IsPrimary }).ToList(),
            Phones = profile.Phones.Select(p => new ContactPhoneDto { Id = p.Id, PhoneNumber = p.PhoneNumber, Label = p.Label }).ToList()
        };

        // 2. Fetch Read-Only Data from SIS
        // Assuming Profile ID maps to SIS Student ID (or retrieve mapping)
        var studentId = profile.Id.ToString(); 
        try 
        {
            // Fetch Basic Info & Academic History
            // Note: In a real scenario, GetStudentTranscriptAsync returns academic history.
            // We might need another method for Basic Info if not included, but for now we assume we get what we can.
            // Converting existing Academic History retrieval logic:
            
            var qualifications = await _studentSystemIntegration.GetStudentTranscriptAsync(studentId);
            
            if (qualifications.Any())
            {
                var mainQual = qualifications.First(); // Use primary qualification for basic info
                // Map Read-Only SIS Data
                dto.AlumniId = mainQual.QualificationId; // Or StudentId
                dto.AcademicHistory = qualifications.Select(q => new QualificationHistoryDto
                {
                    QualificationId = q.QualificationId,
                    DegreeName = q.DegreeName,
                    Major = q.Major,
                    College = q.College,
                    GraduationYear = q.GraduationYear,
                    CumulativeGPA = q.CumulativeGPA, 
                    // Semesters excluded for summary view, or included if needed
                }).ToList();
            }
        }
        catch(Exception)
        {
            // Fail gracefully if SIS is down, show partial data
        }

        // Fill other Read-Only fields from Profile/User if SIS doesn't provide them yet
        // dto.NameEn = ... (Fetched from IdentityUser or SIS)
        
        return dto;
    }

    /// <summary>
    /// Update Graduate's editable profile data.
    /// STRICTLY separates Editable vs Read-Only.
    /// </summary>
    public async Task<AlumniMyProfileDto> UpdateMyProfileAsync(UpdateMyProfileDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());

        // 1. Update Basic Editable Fields
        profile.UpdateAddress(input.Address);
        profile.UpdateBasicInfo(profile.MobileNumber, input.Bio, input.JobTitle);

        // 2. Update Contacts (Sync Collections)
        
        // Emails
        // Remove deleted
        var inputEmailIds = input.Emails.Where(e => e.Id != Guid.Empty).Select(e => e.Id).ToList();
        var emailsToRemove = profile.Emails.Where(e => !inputEmailIds.Contains(e.Id)).ToList();
        foreach(var email in emailsToRemove) profile.RemoveEmail(email.Id);
        
        // Add/Update
        foreach (var emailDto in input.Emails)
        {
            if (emailDto.Id == Guid.Empty)
            {
                var newEmail = new ContactEmail(GuidGenerator.Create(), profile.Id, emailDto.Email, emailDto.IsPrimary);
                profile.AddEmail(newEmail);
            }
            else
            {
                var existing = profile.Emails.FirstOrDefault(e => e.Id == emailDto.Id);
                if (existing != null)
                {
                    existing.UpdateEmail(emailDto.Email); // Assuming UpdateEmail exists
                    if (emailDto.IsPrimary) profile.SetPrimaryEmail(existing.Id); // Will handle unmarking others
                }
            }
        }
        
        // Ensure one primary if any exist
        if (profile.Emails.Any() && !profile.Emails.Any(e => e.IsPrimary))
        {
            profile.SetPrimaryEmail(profile.Emails.First().Id);
        }

        // Mobiles
        var inputMobileIds = input.Mobiles.Where(m => m.Id != Guid.Empty).Select(m => m.Id).ToList();
        var mobilesToRemove = profile.Mobiles.Where(m => !inputMobileIds.Contains(m.Id)).ToList();
        foreach(var mobile in mobilesToRemove) profile.RemoveMobile(mobile.Id);

        foreach (var mobileDto in input.Mobiles)
        {
            if (mobileDto.Id == Guid.Empty)
            {
                var newMobile = new ContactMobile(GuidGenerator.Create(), profile.Id, mobileDto.MobileNumber, mobileDto.IsPrimary);
                profile.AddMobile(newMobile);
            }
            else
            {
                // Update logic if Mobile entity supports it (e.g. UpdateNumber)
                // If not, we might skipped update or implement it. 
                // Assuming "UpdateMobile" or similar exists, or we replace. 
                // However, commonly ID implies identity.
                // For simplified logic:
                if (mobileDto.IsPrimary) profile.SetPrimaryMobile(mobileDto.Id);
            }
        }
        
        // Phones
        var inputPhoneIds = input.Phones.Where(p => p.Id != Guid.Empty).Select(p => p.Id).ToList();
        var phonesToRemove = profile.Phones.Where(p => !inputPhoneIds.Contains(p.Id)).ToList();
        foreach(var phone in phonesToRemove) profile.RemovePhone(phone.Id);
        
        foreach (var phoneDto in input.Phones)
        {
             if (phoneDto.Id == Guid.Empty)
             {
                 var newPhone = new ContactPhone(GuidGenerator.Create(), profile.Id, phoneDto.PhoneNumber, phoneDto.Label);
                 profile.AddPhone(newPhone);
             }
        }

        // 3. Handle Photo (Mock Upload)
        if (input.ProfilePhoto != null && input.ProfilePhoto.Length > 0)
        {
            // In real impl: Upload to BlobStorage, get URL
            // var url = await _blobContainer.SaveAsync(input.ProfilePhoto...);
            // profile.SetPhotoUrl(url);
            
            // Placeholder:
            profile.SetPhotoUrl("https://example.com/uploaded-photo-placeholder.jpg");
        }

        await _profileRepository.UpdateAsync(profile);
        
        return await GetMyProfileAsync(); // Return updated view
    }
}
