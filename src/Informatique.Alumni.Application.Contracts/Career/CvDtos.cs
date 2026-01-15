using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CurriculumVitaeDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public string? Summary { get; set; }
    public bool IsLookingForJob { get; set; }
    public CvStatus Status { get; set; }

    public List<CvEducationDto> Educations { get; set; } = new();
    public List<CvExperienceDto> Experiences { get; set; } = new();
    public List<CvSkillDto> Skills { get; set; } = new();
    public List<CvLanguageDto> Languages { get; set; } = new();
    public List<CvCertificationDto> Certifications { get; set; } = new();
    public List<CvProjectDto> Projects { get; set; } = new();
    public List<CvAwardDto> Awards { get; set; } = new();
    public List<CvVolunteerWorkDto> VolunteerWorks { get; set; } = new();
    public List<CvReferenceDto> References { get; set; } = new();
    public List<CvPublicationDto> Publications { get; set; } = new();
    public List<CvInterestDto> Interests { get; set; } = new();
    public List<CvSocialLinkDto> SocialLinks { get; set; } = new();
    public List<CvCourseDto> Courses { get; set; } = new();
    public List<CvPracticalTrainingDto> PracticalTrainings { get; set; } = new();
}

public class CvEducationDto : EntityDto<Guid> { public string Institution { get; set; } = string.Empty; public string Degree { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } }
public class CvExperienceDto : EntityDto<Guid> { public string Company { get; set; } = string.Empty; public string Position { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public string? Description { get; set; } }
public class CvSkillDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string? ProficiencyLevel { get; set; } }
public class CvLanguageDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string? FluencyLevel { get; set; } }
public class CvCertificationDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string Issuer { get; set; } = string.Empty; public DateTime? Date { get; set; } }
public class CvProjectDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; public string? Description { get; set; } public string? Link { get; set; } }
public class CvAwardDto : EntityDto<Guid> { public string Title { get; set; } = string.Empty; public DateTime? Date { get; set; } public string? Description { get; set; } }
public class CvVolunteerWorkDto : EntityDto<Guid> { public string Organization { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } }
public class CvReferenceDto : EntityDto<Guid> { public string ContactName { get; set; } = string.Empty; public string Company { get; set; } = string.Empty; public string? Email { get; set; } }
public class CvPublicationDto : EntityDto<Guid> { public string Title { get; set; } = string.Empty; public string? Journal { get; set; } public DateTime? Date { get; set; } }
public class CvInterestDto : EntityDto<Guid> { public string Name { get; set; } = string.Empty; }
public class CvSocialLinkDto : EntityDto<Guid> { public string Platform { get; set; } = string.Empty; public string Url { get; set; } = string.Empty; }
public class CvCourseDto : EntityDto<Guid> { public string Title { get; set; } = string.Empty; public string? Provider { get; set; } public int? Hours { get; set; } }
public class CvPracticalTrainingDto : EntityDto<Guid> { public string Field { get; set; } = string.Empty; public string? Duration { get; set; } public string? Location { get; set; } }

public class JobDto : FullAuditedEntityDto<Guid>
{
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClosingDate { get; set; }
}

public class JobApplicationDto : FullAuditedEntityDto<Guid>
{
    public Guid JobId { get; set; }
    public Guid AlumniId { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string CvSnapshotBlobName { get; set; } = string.Empty;
}
