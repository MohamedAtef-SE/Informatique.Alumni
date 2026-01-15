using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Career;

public class CurriculumVitae : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; set; }
    public string? Summary { get; set; }
    public bool IsLookingForJob { get; set; }
    public CvStatus Status { get; set; }

    // 14 Child Collections
    public ICollection<CvEducation> Educations { get; set; }
    public ICollection<CvExperience> Experiences { get; set; }
    public ICollection<CvSkill> Skills { get; set; }
    public ICollection<CvLanguage> Languages { get; set; }
    public ICollection<CvCertification> Certifications { get; set; }
    public ICollection<CvProject> Projects { get; set; }
    public ICollection<CvAward> Awards { get; set; }
    public ICollection<CvVolunteerWork> VolunteerWorks { get; set; }
    public ICollection<CvReference> References { get; set; }
    public ICollection<CvPublication> Publications { get; set; }
    public ICollection<CvInterest> Interests { get; set; }
    public ICollection<CvSocialLink> SocialLinks { get; set; }
    public ICollection<CvCourse> Courses { get; set; }
    public ICollection<CvPracticalTraining> PracticalTrainings { get; set; }

    private CurriculumVitae()
    {
        Educations = new List<CvEducation>();
        Experiences = new List<CvExperience>();
        Skills = new List<CvSkill>();
        Languages = new List<CvLanguage>();
        Certifications = new List<CvCertification>();
        Projects = new List<CvProject>();
        Awards = new List<CvAward>();
        VolunteerWorks = new List<CvVolunteerWork>();
        References = new List<CvReference>();
        Publications = new List<CvPublication>();
        Interests = new List<CvInterest>();
        SocialLinks = new List<CvSocialLink>();
        Courses = new List<CvCourse>();
        PracticalTrainings = new List<CvPracticalTraining>();
    }

    public CurriculumVitae(Guid id, Guid alumniId) : base(id)
    {
        AlumniId = alumniId;
        Status = CvStatus.Draft;
        IsLookingForJob = false;
        
        Educations = new List<CvEducation>();
        Experiences = new List<CvExperience>();
        Skills = new List<CvSkill>();
        Languages = new List<CvLanguage>();
        Certifications = new List<CvCertification>();
        Projects = new List<CvProject>();
        Awards = new List<CvAward>();
        VolunteerWorks = new List<CvVolunteerWork>();
        References = new List<CvReference>();
        Publications = new List<CvPublication>();
        Interests = new List<CvInterest>();
        SocialLinks = new List<CvSocialLink>();
        Courses = new List<CvCourse>();
        PracticalTrainings = new List<CvPracticalTraining>();
    }
}

// 14 Child Entities
public class CvEducation : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Institution { get; set; } = string.Empty; public string Degree { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } }
public class CvExperience : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Company { get; set; } = string.Empty; public string Position { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public string? Description { get; set; } }
public class CvSkill : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Name { get; set; } = string.Empty; public string? ProficiencyLevel { get; set; } }
public class CvLanguage : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Name { get; set; } = string.Empty; public string? FluencyLevel { get; set; } }
public class CvCertification : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Name { get; set; } = string.Empty; public string Issuer { get; set; } = string.Empty; public DateTime? Date { get; set; } }
public class CvProject : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Name { get; set; } = string.Empty; public string? Description { get; set; } public string? Link { get; set; } }
public class CvAward : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Title { get; set; } = string.Empty; public DateTime? Date { get; set; } public string? Description { get; set; } }
public class CvVolunteerWork : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Organization { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } }
public class CvReference : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string ContactName { get; set; } = string.Empty; public string Company { get; set; } = string.Empty; public string? Email { get; set; } }
public class CvPublication : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Title { get; set; } = string.Empty; public string? Journal { get; set; } public DateTime? Date { get; set; } }
public class CvInterest : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Name { get; set; } = string.Empty; }
public class CvSocialLink : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Platform { get; set; } = string.Empty; public string Url { get; set; } = string.Empty; }
public class CvCourse : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Title { get; set; } = string.Empty; public string? Provider { get; set; } public int? Hours { get; set; } }
public class CvPracticalTraining : Entity<Guid> { public Guid CurriculumVitaeId { get; set; } public string Field { get; set; } = string.Empty; public string? Duration { get; set; } public string? Location { get; set; } }
