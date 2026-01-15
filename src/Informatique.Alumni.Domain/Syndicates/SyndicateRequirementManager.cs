
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Syndicates;

public class SyndicateRequirementManager : DomainService
{
    private readonly IRepository<SyndicateRequirement, Guid> _repository;

    public SyndicateRequirementManager(IRepository<SyndicateRequirement, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<SyndicateRequirement> CreateAsync(string collegeName, string description, string requirementsContent)
    {
        Check.NotNullOrWhiteSpace(collegeName, nameof(collegeName));
        Check.NotNullOrWhiteSpace(description, nameof(description));
        Check.NotNullOrWhiteSpace(requirementsContent, nameof(requirementsContent));

        // Uniqueness Logic (One Set Per College)
        if (await _repository.AnyAsync(x => x.CollegeName == collegeName))
        {
            throw new UserFriendlyException($"Syndicate requirements for college '{collegeName}' already exist.");
        }

        var syndicateRequirement = new SyndicateRequirement(
            GuidGenerator.Create(),
            collegeName,
            description,
            requirementsContent
        );

        return await _repository.InsertAsync(syndicateRequirement);
    }

    public async Task<SyndicateRequirement> UpdateAsync(Guid id, string collegeName, string description, string requirementsContent)
    {
        Check.NotNullOrWhiteSpace(collegeName, nameof(collegeName));
        Check.NotNullOrWhiteSpace(description, nameof(description));
        Check.NotNullOrWhiteSpace(requirementsContent, nameof(requirementsContent));

        var syndicateRequirement = await _repository.GetAsync(id);

        // Uniqueness Logic (Exclude current ID)
        if (await _repository.AnyAsync(x => x.CollegeName == collegeName && x.Id != id))
        {
            throw new UserFriendlyException($"Syndicate requirements for college '{collegeName}' already exist.");
        }

        syndicateRequirement.Update(collegeName, description, requirementsContent);
        return await _repository.UpdateAsync(syndicateRequirement);
    }
}
