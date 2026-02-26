using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Profiles;

public class AlumniProfile : FullAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string JobTitle { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string MobileNumber { get; private set; } = string.Empty;
    public string NationalId { get; private set; } = string.Empty;
    public bool ShowInDirectory { get; private set; } = true;
    public decimal WalletBalance { get; private set; } = 0m;
    public bool IsVip { get; private set; } = false;
    public Guid BranchId { get; private set; } // For "Branch Scoping" security
    public string? PhotoUrl { get; private set; } // For "Personal Photo" update
    public Guid? NationalityId { get; private set; }

    public int ViewCount { get; private set; } = 0;

    // Admin Lifecycle Fields
    public AlumniStatus Status { get; private set; } = AlumniStatus.Pending;
    public bool IsNotable { get; private set; } = false;
    public IdCardStatus IdCardStatus { get; private set; } = IdCardStatus.None;
    public string? RejectionReason { get; private set; }
    
    // Note: Educations collection handles degrees
    
    private readonly List<Experience> _experiences = new();
    public IReadOnlyCollection<Experience> Experiences => _experiences.AsReadOnly();
    
    private readonly List<Education> _educations = new();
    public IReadOnlyCollection<Education> Educations => _educations.AsReadOnly();
    
    // Contact Information Collections
    private readonly List<ContactEmail> _emails = new();
    public IReadOnlyCollection<ContactEmail> Emails => _emails.AsReadOnly();
    
    private readonly List<ContactMobile> _mobiles = new();
    public IReadOnlyCollection<ContactMobile> Mobiles => _mobiles.AsReadOnly();
    
    private readonly List<ContactPhone> _phones = new();
    public IReadOnlyCollection<ContactPhone> Phones => _phones.AsReadOnly();

    private AlumniProfile() 
    {
    }

    public AlumniProfile(Guid id, Guid userId, string mobileNumber, string nationalId)
        : base(id)
    {
        UserId = Check.NotDefaultOrNull<Guid>(userId, nameof(userId));
        MobileNumber = Check.NotNullOrWhiteSpace(mobileNumber, nameof(mobileNumber), ProfileConsts.MaxMobileLength);
        NationalId = Check.NotNullOrWhiteSpace(nationalId, nameof(nationalId), ProfileConsts.MaxNationalIdLength);
        
        // Basic mobile number validation (should start with + or digit)
        if (!mobileNumber.StartsWith("+") && !char.IsDigit(mobileNumber[0]))
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.InvalidMobileFormat)
                .WithData("MobileNumber", mobileNumber);
        }
    }

    public void DeductWallet(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.NegativeDeduction)
                .WithData("Amount", amount);
        }
        
        if (WalletBalance < amount)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.InsufficientBalance)
                .WithData("WalletBalance", WalletBalance)
                .WithData("RequestedAmount", amount);
        }
        
        WalletBalance -= amount;
    }

    public void AddCredit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.InvalidCreditAmount)
                .WithData("Amount", amount);
        }
        
        WalletBalance += amount;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void UpdateBasicInfo(string mobileNumber, string? bio = null, string? jobTitle = null)
    {
        MobileNumber = Check.NotNullOrWhiteSpace(mobileNumber, nameof(mobileNumber), ProfileConsts.MaxMobileLength);
        
        if (!mobileNumber.StartsWith("+") && !char.IsDigit(mobileNumber[0]))
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.InvalidMobileFormat)
                .WithData("MobileNumber", mobileNumber);
        }
        
        if (bio != null)
        {
            Bio = Check.Length(bio, nameof(bio), ProfileConsts.MaxBioLength);
        }
        
        if (jobTitle != null)
        {
            JobTitle = Check.Length(jobTitle, nameof(jobTitle), ProfileConsts.MaxJobTitleLength);
        }
    }

    public void UpdateJobTitle(string jobTitle)
    {
        JobTitle = Check.Length(jobTitle, nameof(jobTitle), ProfileConsts.MaxJobTitleLength);
    }

    public void UpdateBio(string? bio)
    {
        Bio = bio != null ? Check.Length(bio, nameof(bio), ProfileConsts.MaxBioLength) : null;
    }

    public void ToggleDirectoryVisibility()
    {
        ShowInDirectory = !ShowInDirectory;
    }
    
    public void SetBranchId(Guid branchId)
    {
        BranchId = branchId;
    }
    
    public void SetPhotoUrl(string photoUrl)
    {
        PhotoUrl = photoUrl;
    }

    public string? Address { get; private set; } // Editable Address
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? Company { get; private set; }
    public string? FacebookUrl { get; private set; }
    public string? LinkedinUrl { get; private set; }

    public void UpdateAddress(string? address, string? city = null, string? country = null)
    {
        Address = address;
        if (city != null) City = city;
        if (country != null) Country = country;
    }

    public void UpdateProfessionalInfo(string? company, string? jobTitle)
    {
        if (company != null) Company = company;
        if (jobTitle != null) JobTitle = Check.Length(jobTitle, nameof(jobTitle), ProfileConsts.MaxJobTitleLength);
    }

    public void UpdateSocialLinks(string? facebookUrl, string? linkedinUrl)
    {
        FacebookUrl = facebookUrl;
        LinkedinUrl = linkedinUrl;
    }
    
    public void SetVip(bool isVip)
    {
        IsVip = isVip;
    }

    public void SetUserId(Guid userId)
    {
        UserId = Check.NotDefaultOrNull<Guid>(userId, nameof(userId));
    }
    
    // Contact Management Methods
    public void AddEmail(ContactEmail email)
    {
        _emails.Add(email);
    }
    
    public void SetPrimaryEmail(Guid emailId)
    {
        // Unmark all as non-primary
        foreach (var email in _emails)
        {
            email.UnmarkAsPrimary();
        }
        
        // Mark selected as primary
        var selectedEmail = _emails.FirstOrDefault(e => e.Id == emailId);
        selectedEmail?.MarkAsPrimary();
    }
    
    public void RemoveEmail(Guid emailId)
    {
        var email = _emails.FirstOrDefault(e => e.Id == emailId);
        if (email != null)
        {
            _emails.Remove(email);
        }
    }
    
    public void AddMobile(ContactMobile mobile)
    {
        _mobiles.Add(mobile);
    }
    
    public void SetPrimaryMobile(Guid mobileId)
    {
        // Unmark all as non-primary
        foreach (var mobile in _mobiles)
        {
            mobile.UnmarkAsPrimary();
        }
        
        // Mark selected as primary
        var selectedMobile = _mobiles.FirstOrDefault(m => m.Id == mobileId);
        selectedMobile?.MarkAsPrimary();
    }
    
    public void RemoveMobile(Guid mobileId)
    {
        var mobile = _mobiles.FirstOrDefault(m => m.Id == mobileId);
        if (mobile != null)
        {
            _mobiles.Remove(mobile);
        }
    }
    
    public void AddPhone(ContactPhone phone)
    {
        _phones.Add(phone);
    }
    
    public void RemovePhone(Guid phoneId)
    {
        var phone = _phones.FirstOrDefault(p => p.Id == phoneId);
        if (phone != null)
        {
            _phones.Remove(phone);
        }
    }



    public void AddExperience(Experience experience)
    {
        Check.NotNull(experience, nameof(experience));
        
        // Prevent duplicates
        if (_experiences.Any(e => e.CompanyName == experience.CompanyName && 
                                  e.JobTitle == experience.JobTitle && 
                                  e.StartDate == experience.StartDate))
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.DuplicateExperience);
        }
        
        _experiences.Add(experience);
    }

    public void RemoveExperience(Guid experienceId)
    {
        var experience = _experiences.FirstOrDefault(e => e.Id == experienceId);
        if (experience == null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.ExperienceNotFound)
                .WithData("ExperienceId", experienceId);
        }
        
        _experiences.Remove(experience);
    }

    public void AddEducation(Education education)
    {
        Check.NotNull(education, nameof(education));
        
        // Prevent duplicates
        if (_educations.Any(e => e.InstitutionName == education.InstitutionName && 
                                 e.Degree == education.Degree && 
                                 e.GraduationYear == education.GraduationYear))
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.DuplicateEducation);
        }
        
        _educations.Add(education);
    }

    public void RemoveEducation(Guid educationId)
    {
        var education = _educations.FirstOrDefault(e => e.Id == educationId);
        if (education == null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.EducationNotFound)
                .WithData("EducationId", educationId);
        }
        
        _educations.Remove(education);
    }

    // ── Admin Lifecycle Methods ──

    public void Approve()
    {
        if (Status == AlumniStatus.Active)
        {
            throw new BusinessException("Alumni:AlreadyActive");
        }

        Status = AlumniStatus.Active;
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        Check.NotNullOrWhiteSpace(reason, nameof(reason));
        Status = AlumniStatus.Rejected;
        RejectionReason = reason;
    }

    public void Ban()
    {
        if (Status == AlumniStatus.Banned)
        {
            throw new BusinessException("Alumni:AlreadyBanned");
        }

        Status = AlumniStatus.Banned;
    }

    public void MarkAsNotable(bool isNotable)
    {
        IsNotable = isNotable;
    }

    public void UpdateIdCardStatus(IdCardStatus status)
    {
        IdCardStatus = status;
    }
}

