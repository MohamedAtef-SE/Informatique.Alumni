using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Profiles;

public class AlumniProfileDto : FullAuditedEntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    
    public List<ExperienceDto> Experiences { get; set; } = new();
    public List<EducationDto> Educations { get; set; } = new();
}

public class UpdateAlumniProfileDto
{
    [MaxLength(ProfileConsts.MaxJobTitleLength)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(ProfileConsts.MaxBioLength)]
    public string? Bio { get; set; }

    [Required]
    [RegularExpression(ProfileConsts.MobileNumberRegex)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(ProfileConsts.NationalIdRegex)]
    public string NationalId { get; set; } = string.Empty;
}

public class ExperienceDto : EntityDto<Guid>
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class CreateUpdateExperienceDto
{
    [Required]
    [MaxLength(ProfileConsts.MaxPlaceLength)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(ProfileConsts.MaxJobTitleLength)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class EducationDto : EntityDto<Guid>
{
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
}

public class CreateUpdateEducationDto
{
    [Required]
    [MaxLength(ProfileConsts.MaxPlaceLength)]
    public string InstitutionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Degree { get; set; } = string.Empty;

    [Required]
    public int GraduationYear { get; set; }
}
