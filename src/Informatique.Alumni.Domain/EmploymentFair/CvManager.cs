using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Emailing;

namespace Informatique.Alumni.EmploymentFair;

public class CvManager : DomainService
{
    private readonly IRepository<AlumniWorkStatus, Guid> _workStatusRepository;
    private readonly IRepository<CvSentHistory, Guid> _sentHistoryRepository;
    private readonly IRepository<AlumniCv, Guid> _cvRepository;
    private readonly IEmailSender _emailSender;

    public CvManager(
        IRepository<AlumniWorkStatus, Guid> workStatusRepository,
        IRepository<CvSentHistory, Guid> sentHistoryRepository,
        IRepository<AlumniCv, Guid> cvRepository,
        IEmailSender emailSender)
    {
        _workStatusRepository = workStatusRepository;
        _sentHistoryRepository = sentHistoryRepository;
        _cvRepository = cvRepository;
        _emailSender = emailSender;
    }

    public async Task UpdateWorkStatusAsync(Guid alumniId, CvWorkStatus status)
    {
        var workStatus = await _workStatusRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId);

        if (workStatus == null)
        {
            workStatus = new AlumniWorkStatus(GuidGenerator.Create(), alumniId, status);
            await _workStatusRepository.InsertAsync(workStatus);
        }
        else
        {
            workStatus.UpdateStatus(status);
            await _workStatusRepository.UpdateAsync(workStatus);
        }
    }

    public async Task SendCvToCompanyAsync(List<Guid> selectedCvIds, string companyEmail, string subject, string body)
    {
        Check.NotNullOrEmpty(selectedCvIds, nameof(selectedCvIds));
        Check.NotNullOrWhiteSpace(companyEmail, nameof(companyEmail));
        Check.NotNullOrWhiteSpace(subject, nameof(subject));

        var cvs = await _cvRepository.GetListAsync(x => selectedCvIds.Contains(x.Id));

        foreach (var cv in cvs)
        {
            // Logic to send email (Mocked/Queued)
            await _emailSender.QueueAsync(
                to: companyEmail,
                subject: subject,
                body: body ?? $"Attached is the CV for Alumni: {cv.AlumniId}" // In real world, attachment logic would be here
            );

            await _sentHistoryRepository.InsertAsync(new CvSentHistory(
                GuidGenerator.Create(),
                cv.AlumniId, 
                companyEmail, 
                subject, 
                DateTime.Now
            ));
        }
    }

    public async Task UpdateCvAsync(UpdateCvInput input)
    {
        Check.NotNull(input, nameof(input));

        // 1. Fetch Existing CV or Create New
        var queryable = await _cvRepository.WithDetailsAsync(
            x => x.WorkExperiences,
            x => x.Educations,
            x => x.Certificates,
            x => x.TrainingCourses,
            x => x.Languages,
            x => x.Skills,
            x => x.Achievements,
            x => x.SchoolInfos
        );
        
        var cv = await AsyncExecuter.FirstOrDefaultAsync(queryable, x => x.AlumniId == input.AlumniId);
        
        if (cv == null)
        {
            cv = new AlumniCv(GuidGenerator.Create(), input.AlumniId);
            await _cvRepository.InsertAsync(cv);
        }

        // 2. Update Basic Info
        cv.UpdateBasicInfo(
            input.Bio,
            input.MilitaryStatus,
            input.MaritalStatus,
            input.YearsOfExperience,
            input.JobInterests,
            input.IsLookingForJob
        );

        // 3. Update Child Collections
        UpdateWorkExperiences(cv, input.WorkExperiences);
        UpdateEducations(cv, input.Educations);
        UpdateCertificates(cv, input.Certificates);
        UpdateTrainingCourses(cv, input.TrainingCourses);
        UpdateLanguages(cv, input.Languages);
        UpdateSkills(cv, input.Skills);
        UpdateAchievements(cv, input.Achievements);
        UpdateSchoolInfos(cv, input.SchoolInfos);

        // 4. Save
        await _cvRepository.UpdateAsync(cv);
    }

    private void UpdateWorkExperiences(AlumniCv cv, List<CvWorkExperienceInput> inputs)
    {
        if (inputs == null) return;

        // Validation: Single Current Job
        if (inputs.Count(x => x.IsCurrent) > 1)
        {
            throw new UserFriendlyException("You can only list one current job.");
        }

        SyncCollection(
            cv.WorkExperiences,
            inputs,
            (entity, input) => entity.Update(input.JobTitle, input.CompanyName, input.JobType, input.StartDate, input.EndDate, input.IsCurrent, input.Description),
            input => new CvWorkExperience(GuidGenerator.Create(), input.JobTitle, input.CompanyName, input.JobType, input.StartDate, input.EndDate, input.IsCurrent, input.Description)
        );
    }

    private void UpdateEducations(AlumniCv cv, List<CvEducationInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.Educations,
            inputs,
            (entity, input) => 
            {
                ValidateDates(input.StartDate, input.EndDate);
                entity.InstitutionName = input.InstitutionName;
                entity.Degree = input.Degree;
                entity.StartDate = input.StartDate;
                entity.EndDate = input.EndDate;
            },
            input => 
            {
                ValidateDates(input.StartDate, input.EndDate);
                return new CvEducation(GuidGenerator.Create()) 
                { 
                    InstitutionName = input.InstitutionName, 
                    Degree = input.Degree, 
                    StartDate = input.StartDate, 
                    EndDate = input.EndDate 
                };
            }
        );
    }

    private void UpdateCertificates(AlumniCv cv, List<CvCertificateInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.Certificates,
            inputs,
            (entity, input) => 
            {
                ValidateDatePastOrPresent(input.Date, "Certificate Date");
                entity.Name = input.Name;
                entity.Authority = input.Authority;
                entity.Date = input.Date;
            },
            input => 
            {
                ValidateDatePastOrPresent(input.Date, "Certificate Date");
                return new CvCertificate(GuidGenerator.Create()) 
                { 
                    Name = input.Name, 
                    Authority = input.Authority, 
                    Date = input.Date 
                };
            }
        );
    }

    private void UpdateTrainingCourses(AlumniCv cv, List<CvTrainingCourseInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.TrainingCourses,
            inputs,
            (entity, input) => 
            {
                ValidateDates(input.StartDate, input.EndDate);
                entity.Name = input.Name;
                entity.CenterName = input.CenterName;
                entity.StartDate = input.StartDate;
                entity.EndDate = input.EndDate;
            },
            input => 
            {
                ValidateDates(input.StartDate, input.EndDate);
                return new CvTrainingCourse(GuidGenerator.Create()) 
                { 
                    Name = input.Name, 
                    CenterName = input.CenterName, 
                    StartDate = input.StartDate, 
                    EndDate = input.EndDate 
                };
            }
        );
    }

    private void UpdateLanguages(AlumniCv cv, List<CvLanguageInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.Languages,
            inputs,
            (entity, input) => 
            {
                entity.Name = input.Name;
                entity.ProficiencyLevel = input.ProficiencyLevel;
            },
            input => new CvLanguage(GuidGenerator.Create()) { Name = input.Name, ProficiencyLevel = input.ProficiencyLevel }
        );
    }

    private void UpdateSkills(AlumniCv cv, List<CvSkillInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.Skills,
            inputs,
            (entity, input) => 
            {
                entity.Name = input.Name;
                entity.Proficiency = input.Proficiency;
            },
            input => new CvSkill(GuidGenerator.Create()) { Name = input.Name, Proficiency = input.Proficiency }
        );
    }

    private void UpdateAchievements(AlumniCv cv, List<CvAchievementInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.Achievements,
            inputs,
            (entity, input) => 
            {
                ValidateDatePastOrPresent(input.Date, "Achievement Date");
                entity.Title = input.Title;
                entity.Date = input.Date;
                entity.Description = input.Description;
            },
            input => 
            {
                ValidateDatePastOrPresent(input.Date, "Achievement Date");
                return new CvAchievement(GuidGenerator.Create())
                { 
                    Title = input.Title, 
                    Date = input.Date, 
                    Description = input.Description 
                };
            }
        );
    }

    private void UpdateSchoolInfos(AlumniCv cv, List<CvSchoolInfoInput> inputs)
    {
        if (inputs == null) return;
        SyncCollection(
            cv.SchoolInfos,
            inputs,
            (entity, input) => 
            {
                entity.SchoolName = input.SchoolName;
                entity.Year = input.Year;
                entity.Grade = input.Grade;
            },
            input => new CvSchoolInfo(GuidGenerator.Create()) { SchoolName = input.SchoolName, Year = input.Year, Grade = input.Grade }
        );
    }

    // Generic Sync Helper
    private void SyncCollection<TEntity, TInput>(
        ICollection<TEntity> collection,
        List<TInput> inputs,
        Action<TEntity, TInput> updateAction,
        Func<TInput, TEntity> createFunc)
        where TEntity : Volo.Abp.Domain.Entities.Entity<Guid>
        where TInput : ICvInput
    {
        var inputIds = inputs.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
        
        // Remove
        var toRemove = collection.Where(x => !inputIds.Contains(x.Id)).ToList();
        foreach (var item in toRemove)
        {
            collection.Remove(item);
        }

        // Add/Update
        foreach (var input in inputs)
        {
            if (input.Id.HasValue)
            {
                var existing = collection.FirstOrDefault(x => x.Id == input.Id.Value);
                if (existing != null)
                {
                    updateAction(existing, input);
                }
            }
            else
            {
                collection.Add(createFunc(input));
            }
        }
    }

    private void ValidateDates(DateTime startDate, DateTime? endDate)
    {
        if (startDate > DateTime.Now) throw new UserFriendlyException("Start Date cannot be in the future.");
        if (endDate.HasValue && endDate.Value > DateTime.Now) throw new UserFriendlyException("End Date cannot be in the future.");
        if (endDate.HasValue && endDate.Value < startDate) throw new UserFriendlyException("End Date must be greater than Start Date.");
    }

    private void ValidateDatePastOrPresent(DateTime date, string fieldName)
    {
        if (date > DateTime.Now) throw new UserFriendlyException($"{fieldName} cannot be in the future.");
    }
}

// Interfaces and Inputs

public interface ICvInput
{
    Guid? Id { get; }
}

public class UpdateCvInput
{
    public Guid AlumniId { get; set; }
    public string? Bio { get; set; }
    public MilitaryStatus MilitaryStatus { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public int YearsOfExperience { get; set; }
    public JobInterests JobInterests { get; set; }
    public bool IsLookingForJob { get; set; }
    
    public List<CvWorkExperienceInput> WorkExperiences { get; set; } = new();
    public List<CvEducationInput> Educations { get; set; } = new();
    public List<CvCertificateInput> Certificates { get; set; } = new();
    public List<CvTrainingCourseInput> TrainingCourses { get; set; } = new();
    public List<CvLanguageInput> Languages { get; set; } = new();
    public List<CvSkillInput> Skills { get; set; } = new();
    public List<CvAchievementInput> Achievements { get; set; } = new();
    public List<CvSchoolInfoInput> SchoolInfos { get; set; } = new();
}

public class CvWorkExperienceInput : ICvInput
{
    public Guid? Id { get; set; }
    public string JobTitle { get; set; }
    public string CompanyName { get; set; }
    public JobType JobType { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class CvEducationInput : ICvInput
{
    public Guid? Id { get; set; }
    public string InstitutionName { get; set; }
    public string Degree { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CvCertificateInput : ICvInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string Authority { get; set; }
    public DateTime Date { get; set; }
}

public class CvTrainingCourseInput : ICvInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string CenterName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CvLanguageInput : ICvInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string ProficiencyLevel { get; set; }
}

public class CvSkillInput : ICvInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string Proficiency { get; set; }
}

public class CvAchievementInput : ICvInput
{
    public Guid? Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
}

public class CvSchoolInfoInput : ICvInput
{
    public Guid? Id { get; set; }
    public string SchoolName { get; set; }
    public int Year { get; set; }
    public string Grade { get; set; }
}
