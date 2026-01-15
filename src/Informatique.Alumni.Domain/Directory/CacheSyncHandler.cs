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
    ITransientDependency
{
    private readonly IRepository<AlumniDirectoryCache, Guid> _cacheRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;

    public CacheSyncHandler(
        IRepository<AlumniDirectoryCache, Guid> cacheRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<AlumniProfile, Guid> profileRepository)
    {
        _cacheRepository = cacheRepository;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityChangedEventData<AlumniProfile> eventData)
    {
        var profile = eventData.Entity;
        
        // Ensure we have details for the cache
        var profileWithDetails = await _profileRepository.WithDetailsAsync(x => x.Experiences, x => x.Educations)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == profile.Id));
            
        if (profileWithDetails == null) return;

        var user = await _userRepository.GetAsync(profile.UserId);
        
        var cache = await _cacheRepository.FirstOrDefaultAsync(x => x.UserId == profile.UserId);
        if (cache == null)
        {
            cache = new AlumniDirectoryCache(Guid.NewGuid()) { UserId = profile.UserId };
            await _cacheRepository.InsertAsync(cache);
        }

        // Flatten data
        cache.FullName = $"{user.Name} {user.Surname}".Trim();
        cache.Email = user.Email;
        cache.JobTitle = profileWithDetails.JobTitle;
        cache.ShowInDirectory = profileWithDetails.ShowInDirectory;

        var latestExp = profileWithDetails.Experiences.OrderByDescending(x => x.StartDate).FirstOrDefault();
        cache.Company = latestExp?.CompanyName;

        var latestEdu = profileWithDetails.Educations.OrderByDescending(x => x.GraduationYear).FirstOrDefault();
        cache.Major = latestEdu?.Degree;
        cache.College = latestEdu?.InstitutionName;
        cache.GraduationYear = latestEdu?.GraduationYear;

        await _cacheRepository.UpdateAsync(cache);
    }
}
