using System;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Branches;

public interface IBranchRepository : IRepository<Branch, Guid>
{
}
