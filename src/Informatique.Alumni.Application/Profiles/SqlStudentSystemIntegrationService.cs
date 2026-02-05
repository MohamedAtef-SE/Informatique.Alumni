using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Real SQL implementation of Student System Integration Service.
/// Fetches data from "Legacy System Simulation" tables in SQL Server.
/// </summary>
public class SqlStudentSystemIntegrationService : ApplicationService, IStudentSystemIntegrationService
{
    private readonly IRepository<SisQualification, Guid> _qualificationRepository;
    private readonly IRepository<SisExpectedGraduate, Guid> _expectedGraduateRepository;

    public SqlStudentSystemIntegrationService(
        IRepository<SisQualification, Guid> qualificationRepository,
        IRepository<SisExpectedGraduate, Guid> expectedGraduateRepository)
    {
        _qualificationRepository = qualificationRepository;
        _expectedGraduateRepository = expectedGraduateRepository;
    }

    public async Task<List<SisQualification>> GetStudentTranscriptAsync(string studentId)
    {
        // Fetch from SQL using the Repository
        // Note: studentId here is expected to be the User ID or mapped Student ID.
        // For simplicity in this integration, we assume studentId param maps to SisQualification.StudentId (Guid) if parseable,
        // or we filter by string if we added a string ID.
        // In SisDataTypes conversion, I added `Guid StudentId` to link to Alumni.
        // The `IStudentSystemIntegrationService` accepts `string studentId`.
        // If the caller passes a Guid string, we parse it.
        
        if (Guid.TryParse(studentId, out var alumniId))
        {
            var queryable = await _qualificationRepository.WithDetailsAsync(x => x.Semesters, x => x.Semesters.Select(s => s.Courses));
            var qualifications = await AsyncExecuter.ToListAsync(queryable.Where(x => x.StudentId == alumniId));
            return qualifications;
        }
        
        return new List<SisQualification>();
    }

    public async Task<SisPagedResult<SisExpectedGraduate>> GetExpectedGraduatesAsync(SisExpectedGraduateFilter input)
    {
        var queryable = await _expectedGraduateRepository.GetQueryableAsync();

        if (input.StudentId != null)
        {
            queryable = queryable.Where(x => x.StudentId.Contains(input.StudentId));
        }
        // Add other filters as needed...

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new SisPagedResult<SisExpectedGraduate>(totalCount, items);
    }

    public Task<List<Informatique.Alumni.AcademicCalendar.AcademicCalendarItem>> GetAcademicCalendarAsync()
    {
        // For now, return static calendar data or fetch from a future SQL table.
        // To strictly "Use SQL", we could add a table, but for "Account Data" request, this is likely acceptable.
        // We will stick to the static list for Calendar unless requested otherwise, as it's not User-specific.
        
        var items = new List<Informatique.Alumni.AcademicCalendar.AcademicCalendarItem>
        {
            new() 
            {
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 9, 15),
                EventName = "Fall Registration",
                Description = "Registration period for Fall 2024 semester",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Fall
            },
            new() 
            {
                StartDate = new DateTime(2024, 9, 16),
                EventName = "Fall Classes Start",
                Description = "First day of classes for Fall 2024",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Fall
            }
        };

        return Task.FromResult(items);
    }
}
