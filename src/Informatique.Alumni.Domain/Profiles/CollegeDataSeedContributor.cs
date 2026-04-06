using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Seeds the College lookup table with Ain Shams University faculties.
/// This is REFERENCE DATA — runs in all environments including Production.
/// Idempotent: skips if colleges already exist.
/// </summary>
public class CollegeDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CollegeDataSeedContributor(
        IRepository<College, Guid> collegeRepository,
        IGuidGenerator guidGenerator)
    {
        _collegeRepository = collegeRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Idempotent: skip if already seeded
        if (await _collegeRepository.GetCountAsync() > 0) return;

        var colleges = new[]
        {
            // Ain Shams University — Real Faculties
            "Engineering",
            "Computer Science & Information Systems",
            "Commerce",
            "Law",
            "Medicine",
            "Dentistry",
            "Pharmacy",
            "Science",
            "Arts",
            "Education",
            "Agriculture",
            "Nursing",
            "Physical Education",
            "Languages",
            "Mass Communication",
        };

        foreach (var name in colleges)
        {
            await _collegeRepository.InsertAsync(
                new College(_guidGenerator.Create(), name),
                autoSave: true
            );
        }
    }
}
