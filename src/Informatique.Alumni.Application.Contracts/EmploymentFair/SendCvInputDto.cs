using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Informatique.Alumni.EmploymentFair;

public class SendCvInputDto
{
    [Required]
    public List<Guid> SelectedCvIds { get; set; } = new();

    [Required]
    [EmailAddress]
    public string CompanyEmail { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? Body { get; set; }
}
