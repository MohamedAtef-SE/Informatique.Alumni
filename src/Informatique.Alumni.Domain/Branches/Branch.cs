using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Informatique.Alumni;

namespace Informatique.Alumni.Branches;

public class Branch : FullAuditedAggregateRoot<Guid>, IHasCollege
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? Address { get; private set; }
    public Guid? CollegeId => Id;

    protected Branch()
    {
    }

    public Branch(Guid id, string name, string code, string? address = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), BranchConsts.MaxNameLength);
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), BranchConsts.MaxCodeLength);
        Address = address;
    }

    public void Update(string name, string code, string? address = null)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), BranchConsts.MaxNameLength);
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), BranchConsts.MaxCodeLength);
        Address = address;
    }
}

