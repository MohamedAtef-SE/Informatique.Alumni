using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.EmploymentFair;

public class CvFullReportDto : EntityDto<Guid>
{
    // 1. Basic Info
    public string FullName { get; set; } // From Profile
    public string Bio { get; set; }
    public string JobTitle { get; set; } // From Profile
    
    // 2. Contact Info
    public string PrimaryEmail { get; set; }
    public string PrimaryMobile { get; set; }
    public string Address { get; set; }

    // 3. Qualifications
    public List<CvEducationDto> Educations { get; set; } = new();

    // 4. School Info
    public List<CvSchoolInfoDto> SchoolInfos { get; set; } = new();

    // 5. Certificates
    public List<CvCertificateDto> Certificates { get; set; } = new();

    // 6. Training Courses
    public List<CvTrainingCourseDto> TrainingCourses { get; set; } = new();

    // 7. Languages
    public List<CvLanguageDto> Languages { get; set; } = new();

    // 8. Work Experience
    public List<CvWorkExperienceDto> WorkExperiences { get; set; } = new();

    // 9. Capabilities/Skills
    public List<CvSkillDto> Skills { get; set; } = new();

    // 10. Achievements
    public List<CvAchievementDto> Achievements { get; set; } = new();
}

public class CvEducationDto
{
    public string InstitutionName { get; set; }
    public string Degree { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CvSchoolInfoDto
{
    public string SchoolName { get; set; }
    public int Year { get; set; }
    public string Grade { get; set; }
}

public class CvCertificateDto
{
    public string Name { get; set; }
    public string Authority { get; set; }
    public DateTime Date { get; set; }
}

public class CvTrainingCourseDto
{
    public string Name { get; set; }
    public string CenterName { get; set; }
    public DateTime StartDate { get; set; }
}

public class CvLanguageDto
{
    public string Name { get; set; }
    public string ProficiencyLevel { get; set; }
}

public class CvWorkExperienceDto
{
    public string JobTitle { get; set; }
    public string CompanyName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CvSkillDto
{
    public string Name { get; set; }
    public string Proficiency { get; set; }
}

public class CvAchievementDto
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
}
