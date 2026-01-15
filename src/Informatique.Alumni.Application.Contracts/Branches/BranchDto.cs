using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Branches;

public class BranchDto : EntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Address { get; set; }
}

public class CreateUpdateBranchDto
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Address { get; set; }
}
