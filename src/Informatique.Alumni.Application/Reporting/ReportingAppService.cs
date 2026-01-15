using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Informatique.Alumni.Directory;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Reporting;

[Authorize(AlumniPermissions.Reporting.Default)]
public class ReportingAppService : AlumniAppService, IReportingAppService
{
    private readonly IRepository<AlumniDirectoryCache, Guid> _cacheRepository;

    public ReportingAppService(IRepository<AlumniDirectoryCache, Guid> cacheRepository)
    {
        _cacheRepository = cacheRepository;
    }

    [Authorize(AlumniPermissions.Reporting.BasicReport)]
    public async Task<byte[]> GetBasicDataReportAsync()
    {
        var data = await _cacheRepository.GetListAsync();
        
        // CSV Generation as a simple example for placeholder
        var csv = new StringBuilder();
        csv.AppendLine("FullName,Email,JobTitle,Company,Major,College,GraduationYear");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.FullName},{item.Email},{item.JobTitle},{item.Company},{item.Major},{item.College},{item.GraduationYear}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    [Authorize(AlumniPermissions.Reporting.GraduatesReport)]
    public async Task<byte[]> GetExpectedGraduatesReportAsync(int year)
    {
        var data = await _cacheRepository.GetListAsync(x => x.GraduationYear == year);
        
        var csv = new StringBuilder();
        csv.AppendLine("FullName,Email,Major,College");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.FullName},{item.Email},{item.Major},{item.College}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
