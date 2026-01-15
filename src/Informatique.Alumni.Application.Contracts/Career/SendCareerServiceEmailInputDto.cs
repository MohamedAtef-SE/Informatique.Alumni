
using System;
using System.ComponentModel.DataAnnotations;

namespace Informatique.Alumni.Career;

public class SendCareerServiceEmailInputDto
{
    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}
