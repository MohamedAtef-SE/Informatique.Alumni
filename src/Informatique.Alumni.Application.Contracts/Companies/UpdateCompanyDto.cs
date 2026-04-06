using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Informatique.Alumni.Companies;

public class UpdateCompanyDto
{
    [Required]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    public string NameEn { get; set; } = string.Empty;

    public string? WebsiteUrl { get; set; }

    public string? Industry { get; set; }

    public string? Description { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    // Optional for update: send only if changing the logo
    public IRemoteStreamContent? Logo { get; set; }
}
