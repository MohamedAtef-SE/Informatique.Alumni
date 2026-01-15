using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Events;

public class Company : AggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }

    private Company() { }

    public Company(Guid id, string name, string? logo = null, string? website = null, string? industry = null)
        : base(id)
    {
        Name = name;
        Logo = logo;
        Website = website;
        Industry = industry;
    }
}
