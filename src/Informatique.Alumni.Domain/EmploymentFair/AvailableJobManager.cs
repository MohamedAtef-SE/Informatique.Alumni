using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.EmploymentFair;

public class AvailableJobManager : DomainService
{
    private readonly IRepository<AvailableJob, Guid> _availableJobRepository;

    public AvailableJobManager(IRepository<AvailableJob, Guid> availableJobRepository)
    {
        _availableJobRepository = availableJobRepository;
    }

    public async Task<AvailableJob> CreateAsync(
        string jobTitle,
        string companyName,
        string address,
        string specialization,
        string briefDescription,
        string requirements,
        int? availableOpportunitiesCount = null,
        string? companyLogo = null)
    {
        var job = new AvailableJob(
            GuidGenerator.Create(),
            jobTitle,
            companyName,
            address,
            specialization,
            briefDescription,
            requirements,
            availableOpportunitiesCount,
            companyLogo
        );

        return await _availableJobRepository.InsertAsync(job);
    }

    public async Task<AvailableJob> UpdateAsync(
        Guid id,
        string jobTitle,
        string companyName,
        string address,
        string specialization,
        string briefDescription,
        string requirements,
        int? availableOpportunitiesCount,
        string? companyLogo)
    {
        var job = await _availableJobRepository.GetAsync(id);

        job.Update(
            jobTitle,
            companyName,
            address,
            specialization,
            briefDescription,
            requirements,
            availableOpportunitiesCount,
            companyLogo
        );

        return await _availableJobRepository.UpdateAsync(job);
    }
    
    public async Task ToggleActiveAsync(Guid id, bool isActive)
    {
        var job = await _availableJobRepository.GetAsync(id);
        job.ToggleActive(isActive);
        await _availableJobRepository.UpdateAsync(job);
    }
}
