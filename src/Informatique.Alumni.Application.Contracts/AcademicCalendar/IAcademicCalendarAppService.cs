using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.AcademicCalendar;

/// <summary>
/// Application service for Academic Calendar operations.
/// Follows SRP: Handles only calendar retrieval and export.
/// </summary>
public interface IAcademicCalendarAppService : IApplicationService
{
    /// <summary>
    /// Gets the academic calendar from the legacy SIS system.
    /// Read-only operation - no Create/Update/Delete allowed.
    /// </summary>
    Task<List<AcademicCalendarItemDto>> GetAcademicCalendarAsync();

    /// <summary>
    /// Exports the academic calendar as an Excel file.
    /// SRP: Dedicated method for Excel generation.
    /// </summary>
    Task<byte[]> GetAcademicCalendarExcelAsync();
}
