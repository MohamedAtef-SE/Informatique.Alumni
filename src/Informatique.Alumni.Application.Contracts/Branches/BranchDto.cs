using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Branches;

public class BranchDto : EntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Address { get; set; }
    public Guid? PresidentId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInPage { get; set; }
    public string? FacebookPage { get; set; }
    public string? WhatsAppGroup { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class CreateUpdateBranchDto
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Address { get; set; }
    public Guid? PresidentId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInPage { get; set; }
    public string? FacebookPage { get; set; }
    public string? WhatsAppGroup { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
