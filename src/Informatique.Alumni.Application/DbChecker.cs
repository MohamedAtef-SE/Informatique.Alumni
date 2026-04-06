using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni
{
    public class DbChecker : ITransientDependency
    {
        private readonly IRepository<AlumniProfile, Guid> _profileRepository;

        public DbChecker(IRepository<AlumniProfile, Guid> profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task CheckId(string idStr)
        {
            if (!Guid.TryParse(idStr, out var id)) {
                Console.WriteLine("Invalid GUID format: " + idStr);
                return;
            }

            var profileById = await _profileRepository.FindAsync(id);
            var profileByUserId = (await _profileRepository.GetQueryableAsync()).FirstOrDefault(x => x.UserId == id);

            if (profileById != null) {
                Console.WriteLine($"ID {id} is a PROFILE ID for User {profileById.UserId}");
            } else if (profileByUserId != null) {
                Console.WriteLine($"ID {id} is a USER ID for Profile {profileByUserId.Id}");
            } else {
                Console.WriteLine($"ID {id} NOT FOUND in AlumniProfiles as either Id or UserId");
            }
        }
    }
}
