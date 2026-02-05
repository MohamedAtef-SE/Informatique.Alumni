using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Organization;

namespace Informatique.Alumni.Organization;

public class BranchDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<CollegeDto> Colleges { get; set; } = new();
}

public class CollegeDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public List<DepartmentDto> Departments { get; set; } = new();
}

public class DepartmentDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public List<SpecializationDto> Specializations { get; set; } = new();
}

public class SpecializationDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public SpecializationType Type { get; set; }
}

public class AcademicLevelDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
}
