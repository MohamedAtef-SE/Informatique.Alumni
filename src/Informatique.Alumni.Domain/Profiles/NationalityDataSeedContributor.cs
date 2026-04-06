using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Seeds the Nationality lookup table with a comprehensive set of nationalities.
/// This is REFERENCE DATA — runs in all environments including Production.
/// Idempotent: skips if nationalities already exist.
/// </summary>
public class NationalityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Nationality, Guid> _nationalityRepository;
    private readonly IGuidGenerator _guidGenerator;

    public NationalityDataSeedContributor(
        IRepository<Nationality, Guid> nationalityRepository,
        IGuidGenerator guidGenerator)
    {
        _nationalityRepository = nationalityRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Idempotent: skip if already seeded
        if (await _nationalityRepository.GetCountAsync() > 0) return;

        var nationalities = new[]
        {
            // Arab World
            "Egyptian",
            "Saudi Arabian",
            "Emirati",
            "Kuwaiti",
            "Qatari",
            "Bahraini",
            "Omani",
            "Jordanian",
            "Lebanese",
            "Syrian",
            "Iraqi",
            "Libyan",
            "Tunisian",
            "Algerian",
            "Moroccan",
            "Sudanese",
            "Yemeni",

            // International
            "American",
            "British",
            "Canadian",
            "Australian",
            "French",
            "German",
            "Turkish",
            "Pakistani",
            "Indian",
            "Chinese",
            "Malaysian",
            "Other",
        };

        foreach (var name in nationalities)
        {
            await _nationalityRepository.InsertAsync(
                new Nationality(_guidGenerator.Create(), name),
                autoSave: true
            );
        }
    }
}
