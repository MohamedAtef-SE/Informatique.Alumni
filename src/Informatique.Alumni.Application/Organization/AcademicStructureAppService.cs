using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Organization;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Organization;

[Authorize]
public class AcademicStructureAppService : ApplicationService, IAcademicStructureAppService
{
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Department, Guid> _departmentRepository;
    private readonly IRepository<Specialization, Guid> _specializationRepository;

    public AcademicStructureAppService(
        IRepository<Branch, Guid> branchRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Department, Guid> departmentRepository,
        IRepository<Specialization, Guid> specializationRepository)
    {
        _branchRepository = branchRepository;
        _collegeRepository = collegeRepository;
        _departmentRepository = departmentRepository;
        _specializationRepository = specializationRepository;
    }

    public async Task<List<BranchDto>> GetHierarchyAsync()
    {
        // 1. Fetch all data using separate queries (Optimized to avoid N+1 and massive Cartesian product of Includes)
        var branches = await _branchRepository.GetListAsync();
        var colleges = await _collegeRepository.GetListAsync();
        var departments = await _departmentRepository.GetListAsync();
        var specializations = await _specializationRepository.GetListAsync();

        // 2. Stitch Data in Memory
        
        // Group Specializations by Department
        var specsByDept = specializations.ToLookup(s => s.DepartmentId);

        // Group Departments by College
        var deptsByCollege = departments.ToLookup(d => d.CollegeId);

        // Group Colleges by Branch
        // Note: College entity has BranchId? Need to verify College entity. 
        // I added 'BranchId' to College in previous step.
        // Wait, 'Branch' entity has 'Colleges' collection, but 'College' has 'BranchId'.
        // Data linkage implies College.BranchId is key.
        var collegesByBranch = colleges.Where(c => c.BranchId.HasValue).ToLookup(c => c.BranchId!.Value);

        // 3. Map to DTOs
        var result = branches.Select(b => new BranchDto
        {
            Id = b.Id,
            Name = b.Name,
            Code = b.Code,
            Colleges = collegesByBranch[b.Id].Select(c => new CollegeDto
            {
                Id = c.Id,
                Name = c.Name,
                Departments = deptsByCollege[c.Id].Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Specializations = specsByDept[d.Id].Select(s => new SpecializationDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Type = s.Type
                    }).ToList()
                }).ToList()
            }).ToList()
        }).ToList();

        return result;
    }
}
