using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Syndicates;

public class Syndicate : AggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty; // Comma separated list of required doc names eg: "ID,License,GraduationCertificate"

    private Syndicate() { }

    public Syndicate(Guid id, string name, string description, string requirements)
        : base(id)
    {
        Name = name;
        Description = description;
        Requirements = requirements;
    }
}
