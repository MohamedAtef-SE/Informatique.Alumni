using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Branches;

public class BranchManager : DomainService
{
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<College, Guid> _collegeRepository;

    public BranchManager(
        IRepository<Branch, Guid> branchRepository,
        IRepository<College, Guid> collegeRepository)
    {
        _branchRepository = branchRepository;
        _collegeRepository = collegeRepository;
    }

    public async Task<Branch> CreateBranchAsync(string name, string externalId, string code)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.NotNullOrWhiteSpace(externalId, nameof(externalId));
        Check.NotNullOrWhiteSpace(code, nameof(code));

        if (await _branchRepository.AnyAsync(b => b.Name == name))
        {
            throw new BusinessException("BranchNameAlreadyExists")
                .WithData("Name", name);
        }

        return new Branch(GuidGenerator.Create(), name, code, null) 
        { 
            ExternalId = externalId 
        };
    }

    public async Task AddCollegeAsync(Branch branch, string collegeName, string collegeExternalId)
    {
        Check.NotNull(branch, nameof(branch));
        Check.NotNullOrWhiteSpace(collegeName, nameof(collegeName));
        Check.NotNullOrWhiteSpace(collegeExternalId, nameof(collegeExternalId));

        if (branch.Id == Guid.Empty)
        {
             // If branch is new (not persisted), we rely on collection check if possible or logic relies on persisted branch.
             // Usually Manager creates entities for saved roots.
             // If Colleges collection is loaded, we can check memory.
             // But strict scoped uniqueness usually checks DB for safety.
        }

        // Validate Scoped Uniqueness (Name exists in *this* branch?)
        if (await _collegeRepository.AnyAsync(c => c.BranchId == branch.Id && c.Name == collegeName))
        {
            throw new BusinessException("CollegeNameAlreadyExistsInBranch")
                .WithData("CollegeName", collegeName)
                .WithData("BranchName", branch.Name);
        }

        // We create the college. 
        // NOTE: Since College is an AR, we might save it directly or add to Branch collection.
        // If we add to Branch.Colleges, EF Core handles it if configured.
        // Given Branch has `ICollection<College> Colleges`, we can add it there.
        var college = new College(GuidGenerator.Create(), collegeName, branch.Id, collegeExternalId);
        
        if (branch.Colleges == null) 
        {
             // Should ideally be initialized in Branch constructor or via repo include.
             // If null, we can't add to collection.
             // But valid DDD approach: Create College with BranchId and Save College Repo.
             await _collegeRepository.InsertAsync(college);
        }
        else
        {
            branch.Colleges.Add(college);
        }
    }
}
