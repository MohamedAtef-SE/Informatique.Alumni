using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Companies;

public interface ICompanyAppService : IApplicationService
{
    Task<PagedResultDto<CompanyDto>> GetListAsync([FromQuery] CompanyFilterDto input);
    Task<CompanyDto> GetAsync(Guid id);
    Task<Guid> CreateAsync([FromForm] CreateCompanyDto input);
    Task UpdateAsync(Guid id, [FromForm] UpdateCompanyDto input);
    Task DeleteAsync(Guid id);
}
