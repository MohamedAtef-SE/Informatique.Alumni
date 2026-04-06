using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Majors;

public class MajorDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public Guid CollegeId { get; set; }
}
