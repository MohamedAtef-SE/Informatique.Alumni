using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.EmploymentFair;

public class AlumniCv : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public string? Bio { get; private set; }
    public MilitaryStatus MilitaryStatus { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public int YearsOfExperience { get; private set; }
    public JobInterests JobInterests { get; private set; }
    public bool IsLookingForJob { get; private set; }

    public ICollection<CvWorkExperience> WorkExperiences { get; private set; }
    public ICollection<CvEducation> Educations { get; private set; }

    public ICollection<CvCertificate> Certificates { get; private set; }
    public ICollection<CvTrainingCourse> TrainingCourses { get; private set; }
    public ICollection<CvLanguage> Languages { get; private set; }
    public ICollection<CvSkill> Skills { get; private set; }
    public ICollection<CvAchievement> Achievements { get; private set; }
    public ICollection<CvSchoolInfo> SchoolInfos { get; private set; }

    private AlumniCv()
    {
        WorkExperiences = new Collection<CvWorkExperience>();
        Educations = new Collection<CvEducation>();
        Certificates = new Collection<CvCertificate>();
        TrainingCourses = new Collection<CvTrainingCourse>();
        Languages = new Collection<CvLanguage>();
        Skills = new Collection<CvSkill>();
        Achievements = new Collection<CvAchievement>();
        SchoolInfos = new Collection<CvSchoolInfo>();
    }

    public AlumniCv(Guid id, Guid alumniId) : base(id)
    {
        AlumniId = alumniId;
        WorkExperiences = new Collection<CvWorkExperience>();
        Educations = new Collection<CvEducation>();
        Certificates = new Collection<CvCertificate>();
        TrainingCourses = new Collection<CvTrainingCourse>();
        Languages = new Collection<CvLanguage>();
        Skills = new Collection<CvSkill>();
        Achievements = new Collection<CvAchievement>();
        SchoolInfos = new Collection<CvSchoolInfo>();
    }

    public void UpdateBasicInfo(
        string? bio, 
        MilitaryStatus militaryStatus, 
        MaritalStatus maritalStatus, 
        int yearsOfExperience, 
        JobInterests jobInterests, 
        bool isLookingForJob)
    {
        Bio = bio; // validation?
        MilitaryStatus = militaryStatus;
        MaritalStatus = maritalStatus;
        YearsOfExperience = yearsOfExperience; // validation >= 0
        JobInterests = jobInterests;
        IsLookingForJob = isLookingForJob;
    }

    public void AddWorkExperience(CvWorkExperience experience)
    {
        // "Single Current Job" rule check could be here
        if (experience.IsCurrent && WorkExperiences.Any(x => x.IsCurrent))
        {
             throw new BusinessException("EmploymentFair:OnlyOneCurrentJobAllowed"); // Or UserFriendlyException as per prompt
        }
        WorkExperiences.Add(experience);
    }
}
