
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Services;

public interface IServiceAccessManager : IDomainService
{
    Task<bool> CanCreateAdvisingRequestAsync(Guid alumniId);
}
