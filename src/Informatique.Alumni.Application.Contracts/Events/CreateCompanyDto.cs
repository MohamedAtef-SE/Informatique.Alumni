using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Informatique.Alumni.Events;

public class CreateCompanyDto
{
    [Required]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    public string NameEn { get; set; } = string.Empty;

    public string? WebsiteUrl { get; set; }

    [Required]
    public IRemoteStreamContent Logo { get; set; } = default!;
}
