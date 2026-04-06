using System;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Informatique.Alumni;

public class AlumniProfileTestSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IGuidGenerator _guidGenerator;

    public AlumniProfileTestSeeder(
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var users = await _userRepository.GetListAsync();
        foreach (var user in users)
        {
            var existingProfile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (existingProfile == null)
            {
                var nationalId = "2" + _guidGenerator.Create().ToString().Substring(0, 13);
                if (nationalId.Length < 14) nationalId = nationalId.PadRight(14, '0');

                var phone = "010" + _guidGenerator.Create().ToString().Substring(0, 8);
                if (phone.Length < 11) phone = phone.PadRight(11, '0');

                var profile = new AlumniProfile(
                    _guidGenerator.Create(),
                    user.Id,
                    phone.Substring(0, 11),
                    nationalId.Substring(0, 14));
                
                // Allow using test profiles
                profile.Approve();

                await _profileRepository.InsertAsync(profile, autoSave: true);
            }
        }
    }
}
