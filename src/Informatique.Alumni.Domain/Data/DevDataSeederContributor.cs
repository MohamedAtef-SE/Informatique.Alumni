using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Career;
using Informatique.Alumni.Magazine;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Benefits;
using Informatique.Alumni.Trips;
using Informatique.Alumni.Gallery;
using Informatique.Alumni.Health;
using Informatique.Alumni.Syndicates;
using Informatique.Alumni.Certificates;
using Informatique.Alumni.Events;
using Informatique.Alumni.BlobContainers;
using Volo.Abp.BlobStoring;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Data;

/// <summary>
/// Seeds a development user with all Alumni permissions and comprehensive demo data.
/// Only runs in Development environment.
/// </summary>
public class DevDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IConfiguration _configuration;
    
    // Repositories
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IRepository<Informatique.Alumni.Events.AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<Informatique.Alumni.Magazine.BlogPost, Guid> _postRepository;
    private readonly IRepository<Informatique.Alumni.Magazine.Magazine, Guid> _magazineRepository;
    private readonly IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<SisQualification, Guid> _qualificationRepository;
    private readonly IRepository<SisExpectedGraduate, Guid> _expectedGraduateRepository;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<SubscriptionFee, Guid> _feeRepository;
    private readonly IRepository<AcademicGrant, Guid> _grantRepository;
    private readonly IRepository<CommercialDiscount, Guid> _discountRepository;
    private readonly IRepository<DiscountCategory, Guid> _discountCategoryRepository;
    private readonly IRepository<AlumniTrip, Guid> _tripRepository;
    private readonly IRepository<TripRequest, Guid> _tripRequestRepository;
    private readonly IRepository<GalleryAlbum, Guid> _albumRepository;
    private readonly IRepository<MedicalPartner, Guid> _medicalPartnerRepository;
    private readonly IRepository<Syndicate, Guid> _syndicateRepository;
    private readonly IRepository<CertificateDefinition, Guid> _certificateDefinitionRepository;
    private readonly IRepository<CertificateRequest, Guid> _certificateRequestRepository;
    private readonly IRepository<CareerService, Guid> _careerServiceRepository;
    private readonly IRepository<Informatique.Alumni.Career.CareerServiceType, Guid> _careerServiceTypeRepository;
    private readonly IRepository<Informatique.Alumni.Branches.Branch, Guid> _branchRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _eventRegistrationRepository;
    private readonly IBlobContainer<MagazineBlobContainer> _magazineBlobContainer;
    private readonly IRepository<Informatique.Alumni.Guidance.GuidanceSessionRule, Guid> _ruleRepository;

    private const string DevUserName = "devuser";
    private const string RealUserName = "real_alumni";
    private const string DevUserEmail = "devuser@alumni.local";
    private const string RealUserEmail = "real@alumni.local";
    private const string DefaultPassword = "Dev@123456";
    private const string AlumniGroupName = "Alumni";

    public DevDataSeederContributor(
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IPermissionManager permissionManager,
        IPermissionDefinitionManager permissionDefinitionManager,
        IGuidGenerator guidGenerator,
        IConfiguration configuration,
        IRepository<Job, Guid> jobRepository,
        IRepository<Informatique.Alumni.Events.AssociationEvent, Guid> eventRepository,
        IRepository<Informatique.Alumni.Magazine.BlogPost, Guid> postRepository,
        IRepository<Informatique.Alumni.Magazine.Magazine, Guid> magazineRepository,
        IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> profileRepository,
        IRepository<SisQualification, Guid> qualificationRepository,
        IRepository<SisExpectedGraduate, Guid> expectedGraduateRepository,
        IRepository<AssociationRequest, Guid> requestRepository,
        IRepository<SubscriptionFee, Guid> feeRepository,
        IRepository<AcademicGrant, Guid> grantRepository,
        IRepository<CommercialDiscount, Guid> discountRepository,
        IRepository<DiscountCategory, Guid> discountCategoryRepository,
        IRepository<AlumniTrip, Guid> tripRepository,
        IRepository<TripRequest, Guid> tripRequestRepository,
        IRepository<GalleryAlbum, Guid> albumRepository,
        IRepository<MedicalPartner, Guid> medicalPartnerRepository,
        IRepository<Syndicate, Guid> syndicateRepository,
        IRepository<CertificateDefinition, Guid> certificateDefinitionRepository,
        IRepository<CertificateRequest, Guid> certificateRequestRepository,
        IRepository<CareerService, Guid> careerServiceRepository,
        IRepository<Informatique.Alumni.Career.CareerServiceType, Guid> careerServiceTypeRepository,
        IRepository<Informatique.Alumni.Branches.Branch, Guid> branchRepository,
        IRepository<AlumniEventRegistration, Guid> eventRegistrationRepository,
        IBlobContainer<MagazineBlobContainer> magazineBlobContainer,
        IRepository<Informatique.Alumni.Guidance.GuidanceSessionRule, Guid> ruleRepository)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _permissionManager = permissionManager;
        _permissionDefinitionManager = permissionDefinitionManager;
        _guidGenerator = guidGenerator;
        _configuration = configuration;
        _jobRepository = jobRepository;
        _eventRepository = eventRepository;
        _postRepository = postRepository;
        _magazineRepository = magazineRepository;
        _profileRepository = profileRepository;
        _qualificationRepository = qualificationRepository;
        _expectedGraduateRepository = expectedGraduateRepository;
        _requestRepository = requestRepository;
        _feeRepository = feeRepository;
        _grantRepository = grantRepository;
        _discountRepository = discountRepository;
        _discountCategoryRepository = discountCategoryRepository;
        _tripRepository = tripRepository;
        _tripRequestRepository = tripRequestRepository;
        _albumRepository = albumRepository;
        _medicalPartnerRepository = medicalPartnerRepository;
        _syndicateRepository = syndicateRepository;
        _certificateDefinitionRepository = certificateDefinitionRepository;
        _certificateRequestRepository = certificateRequestRepository;
        _careerServiceRepository = careerServiceRepository;
        _careerServiceTypeRepository = careerServiceTypeRepository;
        _branchRepository = branchRepository;
        _eventRegistrationRepository = eventRegistrationRepository;
        _magazineBlobContainer = magazineBlobContainer;
        _ruleRepository = ruleRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Only run in Development
        var environment = _configuration["App:Environment"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(environment) && !string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase)) return;

        await SeedDevUserAsync();
        
        // --- Single Real Alumni (Legacy) ---
        var realUser = await SeedRealAlumniAsync();

        // --- NEW: Recreate Multiple Alumni (1-5) ---
        await SeedMultipleAlumniAsync();

        await SeedJobsAsync();
        await SeedEventsAsync();
        await SeedMagazineAsync();
        await SeedPdfMagazinesAsync();
        await SeedSisDataAsync();
        
        // New comprehensive seeding
        await SeedBenefitsAsync();
        await SeedTripsAsync();
        await SeedServicesAsync();
        await SeedGalleryAsync();
        await SeedGuidanceRulesAsync();
        
        await CreateAlumniRoleAsync();
        
        // Advanced user-specific seeding
        if (realUser != null)
        {
            try 
            {
                await SeedRealAlumniExtrasAsync(realUser.Id);
            }
            catch (Exception ex)
            {
                 // Log or ignore to ensure main seeding succeeds
                 Console.WriteLine($"SeedRealAlumniExtrasAsync failed: {ex.Message}");
            }
        }
        
        // Seed advisors for Guidance tab
        await SeedAdvisorsAsync();
    }

    private async Task SeedDevUserAsync()
    {
        var existingUser = await _userRepository.FindByNormalizedUserNameAsync(_userManager.NormalizeName(DevUserName));
        if (existingUser != null)
        {
             await EnsureDevUserProfileAsync(existingUser.Id);
             await GrantAllAlumniPermissionsToUserAsync(existingUser.Id);
             return;
        }

        var userId = _guidGenerator.Create();
        var devUser = new IdentityUser(userId, DevUserName, DevUserEmail);
        devUser.SetIsActive(true);

        var result = await _userManager.CreateAsync(devUser, DefaultPassword);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create dev user: {string.Join(", ", result.Errors)}");
        }

        var adminRole = await _roleRepository.FindByNormalizedNameAsync(_roleManager.NormalizeKey("admin"));
        if (adminRole != null) await _userManager.AddToRoleAsync(devUser, "admin");

        await EnsureDevUserProfileAsync(userId);
        await GrantAllAlumniPermissionsToUserAsync(userId);
    }

    private async Task<IdentityUser> SeedRealAlumniAsync()
    {
        var existingUser = await _userRepository.FindByNormalizedUserNameAsync(_userManager.NormalizeName(RealUserName));
        Guid userId;
        IdentityUser user;
        
        if (existingUser != null)
        {
            userId = existingUser.Id;
            user = existingUser;
             if (await _userManager.HasPasswordAsync(existingUser)) await _userManager.RemovePasswordAsync(existingUser);
             await _userManager.AddPasswordAsync(existingUser, DefaultPassword);
        }
        else
        {
            userId = _guidGenerator.Create();
            user = new IdentityUser(userId, RealUserName, RealUserEmail);
            user.SetIsActive(true);
            var res = await _userManager.CreateAsync(user, DefaultPassword);
            if (!res.Succeeded) throw new Exception("Failed to create real user");
        }
        
        // Assign to Alumni Role
        await _userManager.AddToRoleAsync(user, "alumni");

        // 1. Ensure Profile
        var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null)
        {
            profile = new Informatique.Alumni.Profiles.AlumniProfile(_guidGenerator.Create(), userId, "+201012345678", "29001011234567");
            profile.UpdateBasicInfo("+201012345678", "Software Engineer via SQL", "Works at Tech Corp");
            profile.UpdateAddress("123 Giza St.", "Cairo", "Egypt");
            profile.UpdateProfessionalInfo("Tech Corp", "Software Engineer via SQL");
            profile.UpdateSocialLinks("https://facebook.com/alumni", "https://linkedin.com/in/alumni");
            profile.UpdateSocialLinks("https://facebook.com/alumni", "https://linkedin.com/in/alumni");
            profile.SetPhotoUrl("https://ui-avatars.com/api/?name=Real+Alumni&background=random");
            
            // Add Education to ensure visibility in Directory
            profile.AddEducation(new Informatique.Alumni.Profiles.Education(
                _guidGenerator.Create(), 
                profile.Id, 
                "Cairo University", 
                "BSc Computer Science", 
                2024
            ));
            
            await _profileRepository.InsertAsync(profile);
            
            // Assign Default Branch
            var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
            if (branch != null) 
            {
                profile.SetBranchId(branch.Id);
                await _profileRepository.UpdateAsync(profile);
            }
        }
        else 
        {
            // Update existing profile if it lacks new fields
             profile.UpdateAddress("123 Giza St.", "Cairo", "Egypt");
             profile.UpdateProfessionalInfo("Tech Corp", "Software Engineer via SQL");
             profile.UpdateSocialLinks("https://facebook.com/alumni", "https://linkedin.com/in/alumni");
             
             // Ensure at least one education exists
             if (!profile.Educations.Any())
             {
                 profile.AddEducation(new Informatique.Alumni.Profiles.Education(
                    _guidGenerator.Create(), 
                    profile.Id, 
                    "Cairo University", 
                    "BSc Computer Science", 
                    2024
                ));
             }

             // Assign Default Branch if missing
             if (profile.BranchId == Guid.Empty)
             {
                 var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
                 if (branch != null) profile.SetBranchId(branch.Id);
             }
             
             await _profileRepository.UpdateAsync(profile);
        }

        // 2. Ensure Active Membership
        var fee = await _feeRepository.FirstOrDefaultAsync(x => x.Name == "Standard Membership");
        if (fee == null)
        {
            fee = new SubscriptionFee(
                _guidGenerator.Create(), 
                "Standard Membership", 
                100, 
                DateTime.UtcNow.Year, 
                DateTime.UtcNow.AddDays(-10), 
                DateTime.UtcNow.AddYears(1)
            );
            await _feeRepository.InsertAsync(fee);
        }

        var req = await _requestRepository.FirstOrDefaultAsync(x => x.AlumniId == profile.Id);
        if (req == null)
        {
            req = new AssociationRequest(
                _guidGenerator.Create(),
                profile.Id,
                fee.Id,
                Guid.NewGuid().ToString(),
                Guid.NewGuid(), 
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddYears(1),
                Informatique.Alumni.Membership.DeliveryMethod.OfficePickup,
                0, 0, 100, null
            );
            req.MarkAsPaid();
            req.Approve();
            await _requestRepository.InsertAsync(req);
        }
        else
        {
             // Ensure it is valid
             if (req.ValidityEndDate < DateTime.UtcNow)
             {
                 req.ExtendValidity(DateTime.UtcNow.AddYears(1));
                 await _requestRepository.UpdateAsync(req);
             }
             if (req.Status != MembershipRequestStatus.Approved)
             {
                 req.Approve();
                 await _requestRepository.UpdateAsync(req);
             }
        }

        // 3. Seed Transcript
        var transcript = await _qualificationRepository.FirstOrDefaultAsync(x => x.StudentId == profile.Id);
        if (transcript == null)
        {
            transcript = new SisQualification(_guidGenerator.Create())
            {
                StudentId = profile.Id,
                QualificationId = "Q-REAL-01",
                DegreeName = "Bachelor of Computer Science",
                Major = "Software Engineering",
                College = "FCI",
                GraduationYear = 2024,
                CumulativeGPA = 3.6m
            };
            
            var semester = new SisSemester(_guidGenerator.Create())
            {
                 SemesterName = "Spring 2024",
                 Year = 2024,
                 SemesterGPA = 3.8m,
                 QualificationId = transcript.Id
            };
            semester.Courses.Add(new SisCourse(_guidGenerator.Create()) { CourseName = "Database Systems II", Grade = "A", Credits = 3 });
            semester.Courses.Add(new SisCourse(_guidGenerator.Create()) { CourseName = "Distributed Systems", Grade = "A-", Credits = 3 });
            
            transcript.Semesters.Add(semester);
            await _qualificationRepository.InsertAsync(transcript);
        }
        
        await GrantAllAlumniPermissionsToUserAsync(userId);
        return user;
    }

    private async Task EnsureDevUserProfileAsync(Guid userId)
    {
        var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null)
        {
            profile = new Informatique.Alumni.Profiles.AlumniProfile(
                _guidGenerator.Create(),
                userId,
                "+201000000000",
                "12345678901234"
            );
            profile.UpdateBasicInfo("+201000000000", "Full Stack Developer", "Senior Developer at Alumni Portal");
            profile.SetPhotoUrl("https://ui-avatars.com/api/?name=Dev+User&background=random");
            
            // Assign Default Branch
            var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
            if (branch != null) profile.SetBranchId(branch.Id);
            
            await _profileRepository.InsertAsync(profile);
        }
        else 
        {
             // Update existing dev user if needed
             if (profile.BranchId == Guid.Empty)
             {
                 var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
                 if (branch != null) 
                 {
                     profile.SetBranchId(branch.Id);
                     await _profileRepository.UpdateAsync(profile);
                 }
             }
        }
    }

    private async Task SeedSisDataAsync()
    {
         if (await _expectedGraduateRepository.GetCountAsync() > 0) return;
         
         var grad = new SisExpectedGraduate(_guidGenerator.Create())
         {
             StudentId = "ST-2024-001",
             NameEn = "John Doe",
             NameAr = "جون دو",
             GPA = 3.9m,
             MajorName = "CS",
             NationalId = "299010100000",
             BirthDate = new DateTime(2000, 1, 1)
         };
         await _expectedGraduateRepository.InsertAsync(grad);
    }

    private async Task SeedJobsAsync()
    {
        if (await _jobRepository.GetCountAsync() > 0) return;

        var jobs = new[]
        {
            new Job(_guidGenerator.Create(), _guidGenerator.Create(), "Senior Frontend Engineer", "We are looking for a React expert.") { },
            new Job(_guidGenerator.Create(), _guidGenerator.Create(), "Product Designer", "Join our creative team to design world-class apps.") { },
            new Job(_guidGenerator.Create(), _guidGenerator.Create(), "Backend Lead", "Lead our .NET and Cloud infrastructure teams.") { },
            new Job(_guidGenerator.Create(), _guidGenerator.Create(), "AI Researcher", "Work on cutting edge ML models.") { }
        };

        foreach (var job in jobs)
        {
             job.UpdateRequirements("- 5+ years experience\n- Fluent in Typescript/C#");
             job.SetClosingDate(DateTime.UtcNow.AddDays(30));
             await _jobRepository.InsertAsync(job);
        }
    }

    private async Task SeedEventsAsync()
    {
        if (await _eventRepository.GetCountAsync() > 0) return;

        var events = new List<Informatique.Alumni.Events.AssociationEvent>
        {
             new Informatique.Alumni.Events.AssociationEvent(
                _guidGenerator.Create(), "ملتقى التكنولوجيا 2026", "Tech Summit 2026", "EVT-001",
                "Annual technology summit for alumni.", "Grand Hall", "Cairo, Egypt",
                null, true, 150.0m, DateTime.UtcNow.AddDays(5), null
            ),
             new Informatique.Alumni.Events.AssociationEvent(
                _guidGenerator.Create(), "عشاء التعارف", "Networking Dinner", "EVT-002",
                "A casual dinner to meet fellow graduates.", "Nile View Hotel", "Giza, Egypt",
                null, false, null, DateTime.UtcNow.AddDays(10), null
            ),
             new Informatique.Alumni.Events.AssociationEvent(
                _guidGenerator.Create(), "ورشة عمل الذكاء الاصطناعي", "AI Workshop", "EVT-003",
                "Hands-on workshop on generative AI.", "Lab 3, Faculty of Computing", "Campus",
                null, true, 50.0m, DateTime.UtcNow.AddDays(2), null
            )
        };

        foreach (var evt in events)
        {
            evt.AddTimeslot(_guidGenerator.Create(), DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(10).AddHours(4), 100);
            evt.Publish();
            await _eventRepository.InsertAsync(evt);
        }
    }

    private async Task SeedMagazineAsync()
    {
        if (await _postRepository.GetCountAsync() > 0) return;

        var posts = new[]
        {
            new Informatique.Alumni.Magazine.BlogPost(_guidGenerator.Create(), "Alumni Achievements 2025", "Celebrating our graduates success stories...", _guidGenerator.Create(), "News") { IsPublished = true },
            new Informatique.Alumni.Magazine.BlogPost(_guidGenerator.Create(), "Campus Renovation Update", "New labs are opening soon.", _guidGenerator.Create(), "Campus") { IsPublished = true },
             new Informatique.Alumni.Magazine.BlogPost(_guidGenerator.Create(), "Career Fair Recap", "Over 50 companies participated...", _guidGenerator.Create(), "Events") { IsPublished = true }
        };

        foreach (var post in posts)
        {
            await _postRepository.InsertAsync(post);
        }
    }

    private async Task SeedBenefitsAsync()
    {
        if (await _grantRepository.GetCountAsync() > 0) return;

        // Grants
        await _grantRepository.InsertAsync(new AcademicGrant(_guidGenerator.Create(), "منحة الماجستير", "Master's Degree Grant", "Full", 100));
        await _grantRepository.InsertAsync(new AcademicGrant(_guidGenerator.Create(), "منحة التفوق", "Excellence Grant", "Partial", 50));

        // Discount Categories
        var catShopping = new DiscountCategory(_guidGenerator.Create(), "تسوق", "Shopping", "cart-icon");
        var catFood = new DiscountCategory(_guidGenerator.Create(), "طعام", "Food & Beverage", "food-icon");
        
        await _discountCategoryRepository.InsertAsync(catShopping);
        await _discountCategoryRepository.InsertAsync(catFood);

        // Commercial Discounts
        await _discountRepository.InsertAsync(new CommercialDiscount(
            _guidGenerator.Create(), catShopping.Id, "Jarir Bookstore", "10% Off Books", "Get 10% off all technical books.", 10, "ALUMNI10", DateTime.UtcNow.AddYears(1)));
        
        await _discountRepository.InsertAsync(new CommercialDiscount(
            _guidGenerator.Create(), catFood.Id, "Starbucks", "Free Coffee Size Upgrade", "Upgrade your coffee size for free.", 0, null, DateTime.UtcNow.AddYears(1)));
    }

    private async Task SeedTripsAsync()
    {
        if (await _tripRepository.GetCountAsync() > 0) return;

        var trip1 = new AlumniTrip(
            _guidGenerator.Create(), Guid.Empty, "رحلة الأقصر وأسوان", "Luxor & Aswan Trip", TripType.Internal,
            DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(1).AddDays(4), TimeSpan.Zero,
            DateTime.UtcNow.AddDays(15), DateTime.UtcNow.AddDays(20), "Luxor", 0, true, 50);
        
        trip1.Activate();
        await _tripRepository.InsertAsync(trip1);
    }

    private async Task SeedServicesAsync()
    {
        // 1. Certificates
        if (await _certificateDefinitionRepository.GetCountAsync() == 0)
        {
            var cert1 = new CertificateDefinition(_guidGenerator.Create(), "شهادة تخرج", "Graduation Certificate", 50, DegreeType.Undergraduate);
            var cert2 = new CertificateDefinition(_guidGenerator.Create(), "بيان درجات", "Transcript", 75, DegreeType.Undergraduate);
            await _certificateDefinitionRepository.InsertAsync(cert1);
            await _certificateDefinitionRepository.InsertAsync(cert2);
        }

        // 2. Syndicates
        if (await _syndicateRepository.GetCountAsync() == 0)
        {
            var syn = new Syndicate(_guidGenerator.Create(), "نقابة المهندسين", "Engineers Syndicate", "ID, Graduation Certificate");
            await _syndicateRepository.InsertAsync(syn);
        }

        // 3. Health
        if (await _medicalPartnerRepository.GetCountAsync() == 0)
        {
            var partner = new MedicalPartner(_guidGenerator.Create(), "Al-Salam Hospital", MedicalPartnerType.Hospital, "Cairo, Maadi", "19999");
            partner.AddOffer(_guidGenerator.Create(), "General Checkup", "50% Discount on Checkups", "CHECK50");
            await _medicalPartnerRepository.InsertAsync(partner);
        }

        // 4. Career Services
        await SeedCareerServicesAsync();
    }

    private async Task SeedCareerServicesAsync()
    {
        // Ensure Types
        if (await _careerServiceTypeRepository.GetCountAsync() == 0)
        {
            await _careerServiceTypeRepository.InsertAsync(new Informatique.Alumni.Career.CareerServiceType(_guidGenerator.Create(), "ورش عمل", "Workshops"));
            await _careerServiceTypeRepository.InsertAsync(new Informatique.Alumni.Career.CareerServiceType(_guidGenerator.Create(), "ارشاد مهني", "Counseling"));
        }

        var workshopType = await _careerServiceTypeRepository.FirstOrDefaultAsync(x => x.NameEn == "Workshops");
        var counselingType = await _careerServiceTypeRepository.FirstOrDefaultAsync(x => x.NameEn == "Counseling");
        var branch = await _branchRepository.FirstOrDefaultAsync();
        var branchId = branch?.Id ?? Guid.Empty;

        // Create services if none exist
        if (await _careerServiceRepository.GetCountAsync() == 0)
        {
            if (workshopType != null)
            {
                 var s1 = new CareerService(
                     _guidGenerator.Create(), 
                     "كتابة السيرة الذاتية", 
                     "CV Writing Workshop", 
                     "WS-CV-01", 
                     "Learn how to craft a perfect CV.", 
                     false, 0, DateTime.UtcNow.AddMonths(1), 
                     workshopType.Id, branchId
                 );
                 
                 s1.Timeslots.Add(new CareerServiceTimeslot(
                     _guidGenerator.Create(), s1.Id,
                     DateTime.UtcNow.AddDays(7), TimeSpan.FromHours(9), TimeSpan.FromHours(12),
                     "Dr. Ahmed Hassan", "Training Room 1", "Campus Building A", 30
                 ));
                 s1.Timeslots.Add(new CareerServiceTimeslot(
                     _guidGenerator.Create(), s1.Id,
                     DateTime.UtcNow.AddDays(14), TimeSpan.FromHours(14), TimeSpan.FromHours(17),
                     "Eng. Sara Mohamed", "Training Room 2", "Campus Building B", 25
                 ));
                 
                 await _careerServiceRepository.InsertAsync(s1);
            }
            
            if (counselingType != null)
            {
                 var s2 = new CareerService(
                     _guidGenerator.Create(), 
                     "مقابلة وهمية", 
                     "Mock Interview", 
                     "CS-MOCK-01", 
                     "Practice interview with HR experts.", 
                     true, 150, DateTime.UtcNow.AddMonths(1), 
                     counselingType.Id, branchId
                 );
                 
                 s2.Timeslots.Add(new CareerServiceTimeslot(
                     _guidGenerator.Create(), s2.Id,
                     DateTime.UtcNow.AddDays(5), TimeSpan.FromHours(10), TimeSpan.FromHours(11),
                     "HR Expert - Marwa Ali", "Meeting Room 1", "Career Center", 10
                 ));
                 s2.Timeslots.Add(new CareerServiceTimeslot(
                     _guidGenerator.Create(), s2.Id,
                     DateTime.UtcNow.AddDays(12), TimeSpan.FromHours(15), TimeSpan.FromHours(16),
                     "HR Expert - Khaled Ibrahim", "Meeting Room 2", "Career Center", 8
                 ));
                 
                 await _careerServiceRepository.InsertAsync(s2);
            }
        }
        else
        {
            // Add timeslots to existing services that don't have any
            var services = await _careerServiceRepository.GetListAsync(includeDetails: true);
            
            foreach (var service in services)
            {
                // Only add if Timeslots collection exists but is empty
                // If null, the entity was loaded without navigation - skip it
                if (service.Timeslots != null && service.Timeslots.Count == 0)
                {
                    service.Timeslots.Add(new CareerServiceTimeslot(
                        _guidGenerator.Create(), service.Id,
                        DateTime.UtcNow.AddDays(5), TimeSpan.FromHours(10), TimeSpan.FromHours(11),
                        "Session Lecturer", "Training Room", "Campus Center", 20
                    ));
                    service.Timeslots.Add(new CareerServiceTimeslot(
                        _guidGenerator.Create(), service.Id,
                        DateTime.UtcNow.AddDays(10), TimeSpan.FromHours(14), TimeSpan.FromHours(16),
                        "Guest Speaker", "Conference Hall", "Main Building", 30
                    ));
                    await _careerServiceRepository.UpdateAsync(service);
                }
            }
        }
    }

    private async Task SeedGalleryAsync()
    {
        if (await _albumRepository.GetCountAsync() > 0) return;

        var album = new GalleryAlbum(_guidGenerator.Create(), "Graduation 2024", DateTime.UtcNow.AddMonths(-6), "Photos from the graduation ceremony.");
        album.AddMediaItem(_guidGenerator.Create(), "https://picsum.photos/800/600", GalleryMediaType.Photo, "grad1.jpg");
        album.AddMediaItem(_guidGenerator.Create(), "https://picsum.photos/800/601", GalleryMediaType.Photo, "grad2.jpg");
        
        await _albumRepository.InsertAsync(album);
    }

    private async Task SeedRealAlumniExtrasAsync(Guid userId)
    {
        var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null) return;

        // Register for an event
        if (await _eventRegistrationRepository.GetCountAsync() == 0)
        {
            var evt = await _eventRepository.FirstOrDefaultAsync();
            if (evt != null)
            {
                // Constructor: Guid id, Guid alumniId, Guid eventId, string ticketCode, Guid? timeslotId, string? paymentMethod, decimal? paidAmount
                var reg = new AlumniEventRegistration(
                    _guidGenerator.Create(), 
                    profile.Id,
                    evt.Id,
                    Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    null,
                    "Online",
                    150.0m
                );
                await _eventRegistrationRepository.InsertAsync(reg);
            }
        }

// Removed invalid seed for certificate request

    }


    private async Task SeedGuidanceRulesAsync()
    {
        // Find MAIN branch
        var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
        
        // Fallback: If not found by name, take the first one
        if (branch == null)
        {
             branch = await _branchRepository.FirstOrDefaultAsync();
        }

        // If still null (No branches at all), we can't seed rules.
        if (branch == null) return;

        // Check if rule exists FOR THIS BRANCH specifically
        if (await _ruleRepository.AnyAsync(r => r.BranchId == branch.Id)) return;

        // Create Rule: 09:00 - 17:00, 60 mins
        var rule = new Informatique.Alumni.Guidance.GuidanceSessionRule(
            _guidGenerator.Create(),
            branch.Id,
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            60
        );

        // Add Days: Sunday to Thursday
        rule.AddWeekDay(_guidGenerator.Create(), DayOfWeek.Sunday);
        rule.AddWeekDay(_guidGenerator.Create(), DayOfWeek.Monday);
        rule.AddWeekDay(_guidGenerator.Create(), DayOfWeek.Tuesday);
        rule.AddWeekDay(_guidGenerator.Create(), DayOfWeek.Wednesday);
        rule.AddWeekDay(_guidGenerator.Create(), DayOfWeek.Thursday);

        await _ruleRepository.InsertAsync(rule);
    }

    private async Task GrantAllAlumniPermissionsToUserAsync(Guid userId)
    {
        var allPermissions = await GetAllAlumniPermissionsAsync();
        foreach (var permission in allPermissions)
        {
            await _permissionManager.SetForUserAsync(userId, permission, true);
        }
    }

    private async Task<List<string>> GetAllAlumniPermissionsAsync()
    {
        var permissions = new List<string>();
        var groups = await _permissionDefinitionManager.GetGroupsAsync();
        var alumniGroup = groups.FirstOrDefault(g => g.Name == AlumniGroupName);
        if (alumniGroup == null) return permissions;

        foreach (var permission in alumniGroup.Permissions)
        {
            permissions.Add(permission.Name);
            AddChildPermissions(permission, permissions);
        }
        return permissions;
    }

    private void AddChildPermissions(PermissionDefinition permission, List<string> permissions)
    {
        foreach (var child in permission.Children)
        {
            permissions.Add(child.Name);
            AddChildPermissions(child, permissions);
        }
    }

    private async Task CreateAlumniRoleAsync()
    {
        var roleName = "alumni";
        var role = await _roleRepository.FindByNormalizedNameAsync(_roleManager.NormalizeKey(roleName));
        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), roleName);
            await _roleManager.CreateAsync(role);
        }

        // Grant essential permissions to the 'alumni' role
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Events.Register, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Events.Default, true); // View Events
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Default, true); // Access to Profile Service
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Edit, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Manage, true); // Consider if manage is too high? Maybe just Edit/ViewAll?
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Search, true); // REQUIRED for Directory
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Directory.Search, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Gallery.Default, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Career.Register, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Careers.CvManage, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Careers.JobApply, true); // Apply for jobs
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Guidance.BookSession, true); // Book Guidance Sessions
        // Add more as needed
    }

    /// <summary>
    /// Seeds VIP alumni profiles who appear as advisors in the Guidance tab dropdown.
    /// Advisors are determined by IsVip = true.
    /// </summary>
    private async Task SeedAdvisorsAsync()
    {
        try
        {
            // Check if advisors already exist by counting VIP profiles
            var allProfiles = await _profileRepository.GetListAsync();
            var existingVips = allProfiles.Where(p => p.IsVip).ToList();
            if (existingVips.Count >= 3) return;

        var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
        var branchId = branch?.Id ?? Guid.Empty;

        // Advisor data with realistic names, titles, and specializations
        var advisorData = new[]
        {
            new { UserName = "dr_ahmed_hassan", Email = "ahmed.hassan@alumni.local", Name = "Dr. Ahmed Hassan", JobTitle = "Career Counselor", Bio = "15+ years experience in career development and HR consulting." },
            new { UserName = "eng_sara_mohamed", Email = "sara.mohamed@alumni.local", Name = "Eng. Sara Mohamed", JobTitle = "Tech Industry Mentor", Bio = "Software architect with experience at Google and Microsoft." },
            new { UserName = "dr_khaled_ibrahim", Email = "khaled.ibrahim@alumni.local", Name = "Dr. Khaled Ibrahim", JobTitle = "Business Coach", Bio = "MBA professor and startup advisor." }
        };

        foreach (var adv in advisorData)
        {
            // 1. Create User if not exists
            var existingUser = await _userRepository.FindByNormalizedUserNameAsync(_userManager.NormalizeName(adv.UserName));
            Guid userId;
            
            if (existingUser != null)
            {
                userId = existingUser.Id;
            }
            else
            {
                userId = _guidGenerator.Create();
                var user = new IdentityUser(userId, adv.UserName, adv.Email);
                user.SetIsActive(true);
                
                // Set Name extra property for display
                user.ExtraProperties["Name"] = adv.Name;
                
                await _userManager.CreateAsync(user, DefaultPassword);
            }

            // 2. Create or Update Profile
            var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new Informatique.Alumni.Profiles.AlumniProfile(
                    _guidGenerator.Create(),
                    userId,
                    $"+20100{Guid.NewGuid().ToString().Substring(0, 7).Replace("-", "")}",
                    $"299010{Guid.NewGuid().ToString().Substring(0, 8).Replace("-", "")}"
                );
                profile.UpdateBasicInfo(profile.MobileNumber, adv.Bio, adv.JobTitle);
                profile.SetPhotoUrl($"https://ui-avatars.com/api/?name={adv.Name.Replace(" ", "+")}&background=random&size=200");
                profile.SetBranchId(branchId);
                profile.SetVip(true); // This makes them appear in Advisor dropdown
                
                await _profileRepository.InsertAsync(profile);
            }
            else if (!profile.IsVip)
            {
                // Ensure existing profile is VIP
                profile.SetVip(true);
                profile.UpdateBasicInfo(profile.MobileNumber, adv.Bio, adv.JobTitle);
                await _profileRepository.UpdateAsync(profile);
            }
        }
        
        Console.WriteLine("--- SEEDED ADVISORS (VIP Profiles) ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SeedAdvisorsAsync failed: {ex.Message}");
        }
    }

    private async Task SeedPdfMagazinesAsync()
    {
        var dummyPdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A }; // Minimal PDF header
        await _magazineBlobContainer.SaveAsync("issue_2025_01.pdf", dummyPdfContent, overrideExisting: true);
        await _magazineBlobContainer.SaveAsync("issue_2024_12.pdf", dummyPdfContent, overrideExisting: true);

        if (await _magazineRepository.GetCountAsync() > 0) return;

        var issue1 = new Informatique.Alumni.Magazine.Magazine(
            _guidGenerator.Create(), 
            "Alumni Tech Trends 2025 (Dynamic DB)", 
            DateTime.UtcNow.AddMonths(-1), 
            "issue_2025_01.pdf", 
            Informatique.Alumni.Magazine.MagazineFileType.Pdf
        );
        issue1.SetIssueDate(DateTime.UtcNow.AddMonths(-1));

        var issue2 = new Informatique.Alumni.Magazine.Magazine(
            _guidGenerator.Create(), 
            "Campus Life Winter Edition (Dynamic DB)", 
            DateTime.UtcNow.AddMonths(-2), 
            "issue_2024_12.pdf", 
            Informatique.Alumni.Magazine.MagazineFileType.Pdf
        );
        issue2.SetIssueDate(DateTime.UtcNow.AddMonths(-2));

        await _magazineRepository.InsertAsync(issue1);
        await _magazineRepository.InsertAsync(issue2);
    }
    private async Task SeedMultipleAlumniAsync()
    {
        for (int i = 1; i <= 5; i++)
        {
            var userName = $"real_alumni{i}";
            var email = $"real{i}@alumni.local";
            
            // 1. Check if exists (Update instead of Delete to avoid FK issues)
            var existingUser = await _userRepository.FindByNormalizedUserNameAsync(_userManager.NormalizeName(userName));
            IdentityUser user;

            if (existingUser != null)
            {
                user = existingUser;
                // Update Active Status
                bool isActive = i <= 3;
                user.SetIsActive(isActive);
                
                // Reset Password 
                // if (await _userManager.HasPasswordAsync(user)) await _userManager.RemovePasswordAsync(user);
                // await _userManager.AddPasswordAsync(user, DefaultPassword);
                
                await _userManager.UpdateAsync(user);
            }
            else
            {
                // 2. Create User
                var userId = _guidGenerator.Create();
                user = new IdentityUser(userId, userName, email);
                
                bool isActive = i <= 3;
                user.SetIsActive(isActive);
                
                var res = await _userManager.CreateAsync(user, DefaultPassword);
                if (!res.Succeeded) continue; 
            }

            // 3. Assign Role
            if (!await _userManager.IsInRoleAsync(user, "alumni"))
            {
                await _userManager.AddToRoleAsync(user, "alumni");
            }

            // 4. Ensure Profile
            var isActiveProfile = i <= 3;
            var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (profile == null)
            {
                profile = new Informatique.Alumni.Profiles.AlumniProfile(_guidGenerator.Create(), user.Id, $"+2010{i:D8}", $"2900101{i:D7}");
                profile.UpdateBasicInfo($"+2010{i:D8}", $"Alumni {i}", isActiveProfile ? "Active Member" : "Inactive Member");
                profile.SetPhotoUrl($"https://ui-avatars.com/api/?name=Alumni+{i}&background=random");
                profile.AddEducation(new Informatique.Alumni.Profiles.Education(_guidGenerator.Create(), profile.Id, "Cairo University", "CS", 2024));
                
                // Assign Default Branch
                var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
                if (branch != null) profile.SetBranchId(branch.Id);

                await _profileRepository.InsertAsync(profile);
            }
            else
            {
                // Assign Default Branch if missing
                if (profile.BranchId == Guid.Empty)
                {
                    var branch = await _branchRepository.FirstOrDefaultAsync(b => b.Name == "Makram Ebeid (HQ)");
                    if (branch != null) profile.SetBranchId(branch.Id);
                }
                
                profile.UpdateBasicInfo($"+2010{i:D8}", $"Alumni {i}", isActiveProfile ? "Active Member" : "Inactive Member");
                await _profileRepository.UpdateAsync(profile);
            }

            // 5. Create Membershipp
            if (isActiveProfile)
            {
                var fee = await _feeRepository.FirstOrDefaultAsync(x => x.Name == "Standard Membership");
                if (fee != null)
                {
                    var req = await _requestRepository.FirstOrDefaultAsync(x => x.AlumniId == profile.Id);
                    if (req == null)
                    {
                        req = new AssociationRequest(
                            _guidGenerator.Create(),
                            profile.Id,
                            fee.Id,
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid(), 
                            DateTime.UtcNow.AddDays(-10),
                            DateTime.UtcNow.AddYears(1),
                            Informatique.Alumni.Membership.DeliveryMethod.OfficePickup,
                            0, 0, 100, null
                        );
                        req.MarkAsPaid();
                        req.Approve();
                        await _requestRepository.InsertAsync(req);
                    }
                    else
                    {
                        // Ensure Approved
                        if (req.Status != MembershipRequestStatus.Approved)
                        {
                            req.Approve();
                            await _requestRepository.UpdateAsync(req);
                        }
                    }
                }
            }
        }
    }
}
