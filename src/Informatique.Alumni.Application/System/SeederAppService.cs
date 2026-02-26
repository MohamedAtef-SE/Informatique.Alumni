using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity; // Required for CheckErrors() extension method
using Informatique.Alumni.Branches;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Seeding;

[AllowAnonymous]
public class SeederAppService : ApplicationService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly IRepository<SubscriptionFee, Guid> _feeRepository;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IGuidGenerator _guidGenerator;

    public SeederAppService(
        IdentityUserManager userManager,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Major, Guid> majorRepository,
        IRepository<SubscriptionFee, Guid> feeRepository,
        IRepository<AssociationRequest, Guid> requestRepository,
        IGuidGenerator guidGenerator)
    {
        _userManager = userManager;
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _branchRepository = branchRepository;
        _collegeRepository = collegeRepository;
        _majorRepository = majorRepository;
        _feeRepository = feeRepository;
        _requestRepository = requestRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task<List<string>> SeedAlumniAsync()
    {
        var results = new List<string>();

        // 1. Ensure Prerequisites
        var branch = await _branchRepository.FirstOrDefaultAsync();
        if (branch == null)
        {
            // Fix: Branch constructor takes (id, name, code, address)
            branch = new Branch(_guidGenerator.Create(), "Main Branch", "MAIN", "Cairo, Address");
            await _branchRepository.InsertAsync(branch);
        }

        var college = await _collegeRepository.FirstOrDefaultAsync();
        if (college == null) 
        {
             // Create a default college for seeding
             college = new College(_guidGenerator.Create(), "Seed College", branch.Id, null);
             await _collegeRepository.InsertAsync(college);
        }
        
        var major = await _majorRepository.FirstOrDefaultAsync();
        
        var fee = await _feeRepository.FirstOrDefaultAsync();
        if (fee == null)
        {
             // Fix: SubscriptionFee constructor takes (id, name, amount, year, start, end)
             // Amount is decimal (100m)
             fee = new Informatique.Alumni.Membership.SubscriptionFee(
                 _guidGenerator.Create(), 
                 "Seed Fee", 
                 100m, 
                 2025, 
                 DateTime.Now.AddYears(-1).Date, 
                 DateTime.Now.AddYears(1).Date
             );
             await _feeRepository.InsertAsync(fee);
        }

        // 2. Loop
        for (int i = 1; i <= 5; i++)
        {
            var username = $"real_alumni{i}";
            var email = $"real_alumni{i}@example.com";
            var password = "Dev@123456";
            bool isActiveMember = i <= 3;

            try
            {
                // A. Cleanup Existing
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    await _profileRepository.DeleteAsync(x => x.UserId == existingUser.Id);
                    await _requestRepository.DeleteAsync(x => x.AlumniId == existingUser.Id);
                    await _userManager.DeleteAsync(existingUser);
                }

                // B. Create User
                // Use explicit namespace or Volo.Abp.Identity.IdentityUser
                var user = new Volo.Abp.Identity.IdentityUser(_guidGenerator.Create(), username, email);
                (await _userManager.CreateAsync(user, password)).CheckErrors();

                // C. Create Profile
                var nationalId = "2" + DateTime.Now.ToString("yyyyMMdd") + i.ToString("00000");
                var mobile = $"+2010{i}1234567";
                
                var profile = new AlumniProfile(_guidGenerator.Create(), user.Id, mobile, nationalId);
                profile.UpdateBasicInfo(mobile, $"Software Engineer {i}", $"Bio for {username}");
                
                var primaryMobile = new ContactMobile(_guidGenerator.Create(), profile.Id, mobile, true);
                profile.AddMobile(primaryMobile);
                profile.SetPrimaryMobile(primaryMobile.Id);

                var primaryEmail = new ContactEmail(_guidGenerator.Create(), profile.Id, email, true);
                profile.AddEmail(primaryEmail);
                profile.SetPrimaryEmail(primaryEmail.Id);

                profile.SetBranchId(branch.Id);
                profile.UpdateProfessionalInfo("Tech Corp", $"Senior Dev {i}");
                
                // Fix: Experience constructor (id, profileId, company, title, start) + Update()
                var exp = new Experience(
                    _guidGenerator.Create(),
                    profile.Id,
                    "Previous Corp",
                    "Junior Dev",
                    DateTime.Now.AddYears(-5)
                );
                exp.Update("Previous Corp", "Junior Dev", DateTime.Now.AddYears(-5), DateTime.Now.AddYears(-2), "Built legacy systems");
                
                profile.AddExperience(exp);

                await _profileRepository.InsertAsync(profile);

                // D. Add Education
                var gradYear = 2020 + i;
                // Fix: college.Name (not NameAr)
                var education = new Education(
                    _guidGenerator.Create(), 
                    profile.Id, 
                    college.Name,
                    "Bachelor of Computer Science", 
                    gradYear
                );
                education.SetAcademicDetails(1, college.Id, major?.Id, null);
                
                await _educationRepository.InsertAsync(education);

                // E. Membership (Association Request)
                if (isActiveMember)
                {
                    var requestId = _guidGenerator.Create();
                    // Fix: DeliveryMethod.OfficePickup (from MembershipEnums.cs)
                    var request = new AssociationRequest(
                        requestId,
                        user.Id,
                        fee.Id,
                        requestId.ToString(),
                        branch.Id,
                        DateTime.Now.AddMonths(-1),
                        DateTime.Now.AddYears(1),
                        DeliveryMethod.OfficePickup,
                        0,
                        0,
                        fee.Amount,
                        null
                    );
                    
                    request.MarkAsPaid();
                    request.Approve(); 
                    
                    await _requestRepository.InsertAsync(request);
                    results.Add($"{username} [ACTIVE] Created.");
                }
                else
                {
                     results.Add($"{username} [INACTIVE] Created.");
                }
            }
            catch (Exception ex)
            {
                results.Add($"{username} Failed: {ex.Message}");
            }
        }

        return results;
    }
}
