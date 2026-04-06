using System;
using System.ComponentModel.DataAnnotations;

namespace Informatique.Alumni.Colleges;

public class CreateUpdateCollegeDto
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;
    
    public Guid? BranchId { get; set; }
    
    public string? ExternalId { get; set; }
}
