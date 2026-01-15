using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Domain service for Alumni management including SIS synchronization and onboarding.
/// Business Rules: Automatic user creation, email notification, branch scoping.
/// </summary>
public class AlumniManager : DomainService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IStudentSystemIntegrationService _sisIntegration;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IdentityUserManager _userManager;

    public AlumniManager(
        IRepository<AlumniProfile, Guid> profileRepository,
        IStudentSystemIntegrationService sisIntegration,
        IIdentityUserRepository userRepository,
        IEmailSender emailSender,
        IdentityUserManager userManager)
    {
        _profileRepository = profileRepository;
        _sisIntegration = sisIntegration;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _userManager = userManager;
    }

    /// <summary>
    /// Import Alumni from Legacy SIS and create their account.
    /// Business Rules:
    /// 1. Check if alumni already exists
    /// 2. Verify student exists in SIS
    /// 3. Create IdentityUser with username = Student ID
    /// 4. Generate secure random password
    /// 5. Send credentials to official academy email
    /// 6. Create AlumniProfile
    /// </summary>
    public async Task<AlumniProfile> ImportFromSisAsync(
        string studentId,
        string officialEmail,
        Guid branchId)
    {
        // Step 1: Check if alumni already exists
        var existingUser = await _userRepository.FindByNormalizedUserNameAsync(studentId.ToUpperInvariant());
        if (existingUser != null)
        {
            // Alumni already exists, return their profile
            var existingProfile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == existingUser.Id);
            if (existingProfile != null)
            {
                throw new BusinessException("Alumni:AlreadyExists")
                    .WithData("StudentId", studentId)
                    .WithData("ProfileId", existingProfile.Id);
            }
        }

        // Step 2: Fetch data from Legacy SIS
        // Note: Using Domain types (SisQualification) to respect layering
        var sisData = await _sisIntegration.GetStudentTranscriptAsync(studentId);
        if (sisData == null || !sisData.Any())
        {
            // Fallback: Check existence only if data return is null but logic requires check
            // But if GetStudentTranscriptAsync implies existence, we handle empty list as "Not Found" or "No Data"
            // Rule says "StudentExistsAsync" is separate, but we combine logic here for efficiency if possible
            // OR we stick to the rule: Import implies fetching data.
             throw new BusinessException("Alumni:NotFoundInSIS")
                .WithData("StudentId", studentId);
        }

        // Get latest qualification for basic info (e.g. Major)
        var latestQualification = sisData.OrderByDescending(q => q.GraduationYear).FirstOrDefault();
        if (latestQualification == null)
        {
             throw new BusinessException("Alumni:NoQualifications")
                .WithData("StudentId", studentId);
        }

        // Step 3: Create IdentityUser
        var password = GenerateSecurePassword();
        var identityUser = new IdentityUser(
            GuidGenerator.Create(),
            studentId, // Username = Student ID
            officialEmail
        );

        // Set password
        await _userManager.CreateAsync(identityUser, password);

        // Step 4: Create AlumniProfile
        var profile = new AlumniProfile(
            GuidGenerator.Create(),
            identityUser.Id,
            string.Empty, // Mobile will be added separately
            string.Empty  // National ID will be synced from SIS
        );
        profile.SetBranchId(branchId);

        // Add official email as primary contact
        var primaryEmail = new ContactEmail(
            GuidGenerator.Create(),
            profile.Id,
            officialEmail,
            isPrimary: true
        );
        profile.AddEmail(primaryEmail);

        // Step 5: Populate Education History from SIS Data
        // Business Rule: Search relies on local Education data (Year, Semester, etc.)
        foreach (var qualification in sisData)
        {
            // Determine Graduation Semester (Last semester or default to 1)
            var lastSemester = qualification.Semesters.OrderByDescending(s => s.Year).ThenByDescending(s => s.SemesterNumber).FirstOrDefault();
            var gradSemester = lastSemester?.SemesterNumber ?? 1;

            var education = new Education(
                GuidGenerator.Create(),
                profile.Id,
                qualification.College, // Using College as Institution Name
                qualification.DegreeName,
                qualification.GraduationYear
            );
            
            // Set extended details for filtering
            education.SetAcademicDetails(
                gradSemester,
                null, // CollegeId (Need mapping if available, otherwise null)
                null, // MajorId
                null  // MinorId
            );
            
            profile.AddEducation(education);
        }

        await _profileRepository.InsertAsync(profile);

        // Step 6: Send credentials via email
        await SendWelcomeEmailAsync(officialEmail, studentId, password);

        return profile;
    }

    /// <summary>
    /// Generate a secure random password.
    /// </summary>
    private string GenerateSecurePassword()
    {
        const string uppercase = "ABCDEFGH JKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*";

        var random = new Random();
        var password = new char[12];

        // Ensure at least one character from each category
        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill remaining with random mix
        var allChars = uppercase + lowercase + digits + special;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    /// <summary>
    /// Send welcome email with login credentials.
    /// </summary>
    private async Task SendWelcomeEmailAsync(
        string email,
        string username,
        string password)
    {
        var subject = "Welcome to Alumni System - Your Login Credentials";
        var body = $@"
Dear Alumni,

Welcome to the Alumni Management System!

Your account has been successfully created. Below are your login credentials:

Username: {username}
Password: {password}

Please login and change your password immediately for security reasons.

Best regards,
Alumni Relations Office
";

        await _emailSender.SendAsync(email, subject, body);
    }
}
