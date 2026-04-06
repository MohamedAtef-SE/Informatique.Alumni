using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Colleges;

public class CollegeDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? ExternalId { get; set; }
}
