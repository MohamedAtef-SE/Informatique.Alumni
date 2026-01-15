
using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Syndicates;

public class SyndicateRequirementDto : EntityDto<Guid>
{
    public string CollegeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequirementsContent { get; set; } = string.Empty;
}

public class SyndicateRequirementFilterDto : PagedResultRequestDto
{
    public string? CollegeName { get; set; }
}
