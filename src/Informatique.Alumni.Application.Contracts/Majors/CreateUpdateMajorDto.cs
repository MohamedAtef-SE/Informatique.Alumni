using System;
using System.ComponentModel.DataAnnotations;

namespace Informatique.Alumni.Majors;

public class CreateUpdateMajorDto
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CollegeId { get; set; }
}
