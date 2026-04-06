using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Informatique.Alumni.Profiles;

[Authorize(AlumniPermissions.Reporting.BasicReport)]
public class AlumniReportingAppService : AlumniAppService, IAlumniReportingAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<AssociationRequest, Guid> _membershipRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IIdentityUserRepository _userRepository;

    public AlumniReportingAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<AssociationRequest, Guid> membershipRepository,
        IRepository<Branch, Guid> branchRepository,
        IIdentityUserRepository userRepository)
    {
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _membershipRepository = membershipRepository;
        _branchRepository = branchRepository;
        _userRepository = userRepository;
    }

    public async Task<object> GetReportAsync(AlumniReportInputDto input)
    {
        // Enforce Branch Security
        var currentUserBranchId = CurrentUser.GetCollegeId(); 
        if (currentUserBranchId.HasValue)
        {
            input.BranchId = currentUserBranchId.Value;
        }

        if (input.ReportType == AlumniReportType.Statistical)
        {
            return new AlumniStatisticalReportDto(); // Simplified for stabilization
        }
        else
        {
            var query = await _profileRepository.GetQueryableAsync();
            if (input.BranchId.HasValue) query = query.Where(x => x.BranchId == input.BranchId);
            
            var totalCount = await AsyncExecuter.CountAsync(query);
            var profiles = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            
            var dtos = profiles.Select((p, index) => new AlumniReportDetailDto
            {
                SerialNo = index + input.SkipCount + 1,
                AlumniId = p.UserId.ToString(),
                Name = "N/A" // Simplified
            }).ToList();

            return new PagedResultDto<AlumniReportDetailDto>(totalCount, dtos);
        }
    }

    public async Task<List<ExpectedGraduatesReportOutputDto>> GetExpectedGraduatesReportAsync(ExpectedGraduatesReportInputDto input)
    {
        // Integration with SIS or local data
        return new List<ExpectedGraduatesReportOutputDto>();
    }
}
