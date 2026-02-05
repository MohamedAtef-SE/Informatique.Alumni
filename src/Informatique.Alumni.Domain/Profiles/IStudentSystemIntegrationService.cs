using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Domain interface for Legacy Student Information System (SIS) integration.
/// Simplified interface to avoid DTO dependencies in Domain layer.
/// </summary>
public interface IStudentSystemIntegrationService : ITransientDependency
{
    /// <summary>
    /// Fetch complete academic transcript from Legacy SIS.
    /// Returns domain-compliant structure for business logic processing.
    /// </summary>
    /// <param name="studentId">Student ID from legacy system</param>
    /// <returns>List of qualifications with nested semester/course data</returns>
    Task<List<SisQualification>> GetStudentTranscriptAsync(string studentId);
    
    /// <summary>
    /// Search expected graduates from Legacy SIS (Proxy).
    /// </summary>
    Task<SisPagedResult<SisExpectedGraduate>> GetExpectedGraduatesAsync(SisExpectedGraduateFilter input);
    
    /// <summary>
    /// Gets the academic calendar from the legacy SIS.
    /// Read-only operation - calendar is authoritative in SIS.
    /// DIP: Abstraction allows multiple SIS implementations.
    /// Clean Architecture: Returns domain model, not DTO.
    /// </summary>
    Task<List<Informatique.Alumni.AcademicCalendar.AcademicCalendarItem>> GetAcademicCalendarAsync();
}
