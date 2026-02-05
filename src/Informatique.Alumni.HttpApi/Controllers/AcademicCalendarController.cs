using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace Informatique.Alumni.Controllers;

[RemoteService]
[Route("api/app/academic-calendar")]
public class AcademicCalendarController : AbpController
{
    private readonly IStudentSystemIntegrationService _studentSystemIntegrationService;

    public AcademicCalendarController(IStudentSystemIntegrationService studentSystemIntegrationService)
    {
        _studentSystemIntegrationService = studentSystemIntegrationService;
    }

    [HttpGet]
    public async Task<List<Informatique.Alumni.AcademicCalendar.AcademicCalendarItem>> GetAsync()
    {
        return await _studentSystemIntegrationService.GetAcademicCalendarAsync();
    }
}
