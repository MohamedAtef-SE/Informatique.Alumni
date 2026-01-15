using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Directory;

public class AlumniDirectoryCache : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Major { get; set; }
    public string? College { get; set; }
    public int? GraduationYear { get; set; }
    public bool ShowInDirectory { get; set; }

    private AlumniDirectoryCache() { }

    public AlumniDirectoryCache(Guid id) : base(id) { }
}
