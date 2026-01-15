using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Directory;

public class AlumniDirectoryDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Major { get; set; }
    public string? College { get; set; }
    public int? GraduationYear { get; set; }
}

public class AlumniSearchRequestDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Major { get; set; }
    public string? College { get; set; }
    public int? GraduationYear { get; set; }
}
