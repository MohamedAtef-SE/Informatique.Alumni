using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Informatique.Alumni.Directory;

public class CacheSyncHandler : 
    ILocalEventHandler<EntityChangedEventData<AlumniProfile>>,
    ILocalEventHandler<EntityDeletedEventData<AlumniProfile>>,
    ITransientDependency
{
    private readonly IRepository<AlumniDirectoryCache, Guid> _cacheRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly IRepository<College, Guid> _collegeRepository;

    public CacheSyncHandler(
        IRepository<AlumniDirectoryCache, Guid> cacheRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Major, Guid> majorRepository,
        IRepository<College, Guid> collegeRepository)
    {
        _cacheRepository = cacheRepository;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _majorRepository = majorRepository;
        _collegeRepository = collegeRepository;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityChangedEventData<AlumniProfile> eventData)
    {
        var profile = eventData.Entity;
        
        // Use idiomatic async/await instead of ContinueWith for better readability and exception handling
        var profileQuery = await _profileRepository.WithDetailsAsync(x => x.Experiences, x => x.Educations);
        var profileWithDetails = profileQuery.FirstOrDefault(x => x.Id == profile.Id);
            
        if (profileWithDetails == null) return;

        var user = await _userRepository.FindAsync(profile.UserId);
        if (user == null) return;

        // Atomic check: Ensure we only ever have one cache entry per user
        var cache = await _cacheRepository.FirstOrDefaultAsync(x => x.UserId == profile.UserId);
        if (cache == null)
        {
            cache = new AlumniDirectoryCache(Guid.NewGuid()) { UserId = profile.UserId };
            // Background: The Unique Index in the DB will prevent duplicates if a race condition occurs here
            await _cacheRepository.InsertAsync(cache);
        }

        // Flatten data
        cache.FullName = $"{user.Name} {user.Surname}".Trim();
        cache.Email = user.Email;
        cache.JobTitle = profileWithDetails.JobTitle;
        cache.ShowInDirectory = profileWithDetails.ShowInDirectory;
        cache.IsVip = profileWithDetails.IsVip;
        cache.PhotoUrl = profileWithDetails.PhotoUrl;

        var latestExp = profileWithDetails.Experiences.OrderByDescending(x => x.StartDate).FirstOrDefault();
        cache.Company = latestExp?.CompanyName;

        var latestEdu = profileWithDetails.Educations.OrderByDescending(x => x.GraduationYear).FirstOrDefault();
        if (latestEdu != null)
        {
            cache.GraduationYear = latestEdu.GraduationYear;
            
            // Fetch Names for College and Major
            if (latestEdu.CollegeId.HasValue)
            {
                var college = await _collegeRepository.FindAsync(latestEdu.CollegeId.Value);
                cache.College = college?.Name;
            }
            
            if (latestEdu.MajorId.HasValue)
            {
                var major = await _majorRepository.FindAsync(latestEdu.MajorId.Value);
                cache.Major = major?.Name;
            }
        }

        await _cacheRepository.UpdateAsync(cache);
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityDeletedEventData<AlumniProfile> eventData)
    {
        var profile = eventData.Entity;
        
        // Remove from cache when profile is deleted
        var cache = await _cacheRepository.FirstOrDefaultAsync(x => x.UserId == profile.UserId);
        if (cache != null)
        {
            await _cacheRepository.DeleteAsync(cache);
        }
    }
}
