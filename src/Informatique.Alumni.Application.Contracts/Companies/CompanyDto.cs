using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Companies;

public class CompanyDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string LogoBlobName { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? Industry { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
