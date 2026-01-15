using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.AcademicCalendar;

/// <summary>
/// Application service for Academic Calendar operations.
/// SRP: Focused only on calendar retrieval and Excel export.
/// Clean Code: Small, focused methods with clear intent.
/// </summary>
[Authorize]
public class AcademicCalendarAppService : ApplicationService, IAcademicCalendarAppService
{
    private readonly IStudentSystemIntegrationService _sisIntegrationService;

    public AcademicCalendarAppService(IStudentSystemIntegrationService sisIntegrationService)
    {
        _sisIntegrationService = sisIntegrationService;
    }

    /// <summary>
    /// Gets the academic calendar from SIS.
    /// Clean Architecture: Maps domain model to DTO for API response.
    /// SRP: Just orchestration, actual fetching is in integration service.
    /// </summary>
    public async Task<List<AcademicCalendarItemDto>> GetAcademicCalendarAsync()
    {
        var domainModels = await _sisIntegrationService.GetAcademicCalendarAsync();
        
        // Map domain models to DTOs (Clean Architecture layering)
        return domainModels.Select(item => new AcademicCalendarItemDto
        {
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            EventName = item.EventName,
            Description = item.Description,
            Semester = (AcademicSemester)(int)item.Semester // Enum mapping
        }).ToList();
    }

    /// <summary>
    /// Exports academic calendar as Excel file.
    /// SRP: Dedicated method for Excel generation.
    /// Clean Code: Focused, single responsibility - generate Excel only.
    /// </summary>
    public async Task<byte[]> GetAcademicCalendarExcelAsync()
    {
        // 1. Fetch data from SIS (domain models)
        var domainModels = await _sisIntegrationService.GetAcademicCalendarAsync();

        // 2. Transform to Excel-friendly format
        var excelData = domainModels
            .OrderBy(x => x.StartDate)
            .Select(item => new
            {
                StartDate = item.StartDate.ToString("yyyy-MM-dd"),
                EndDate = item.EndDate?.ToString("yyyy-MM-dd") ?? "-",
                EventName = item.EventName,
                Description = item.Description,
                Semester = item.Semester.ToString()
            })
            .ToList();

        // 3. Generate Excel using MiniExcel (Clean Code: External library for complex logic)
        using var stream = new System.IO.MemoryStream();
        await MiniExcelLibs.MiniExcel.SaveAsAsync(stream, excelData);
        
        return stream.ToArray();
    }
}
