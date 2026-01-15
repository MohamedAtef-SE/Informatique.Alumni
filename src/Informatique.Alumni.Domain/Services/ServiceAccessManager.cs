
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Services;

public class ServiceAccessManager : DomainService, IServiceAccessManager
{
    public Task<bool> CanCreateAdvisingRequestAsync(Guid alumniId)
    {
        // CORE: Check Card Status
        // For now, returning true as per additive implementation rules (Placeholder)
        // In real implementation, this would check AlumniProfile.CardStatus or Membership module.
        return Task.FromResult(true);
    }
}
