using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Guidance;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Data;

public class GuidanceDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<GuidanceSessionRule, Guid> _ruleRepository;
    private readonly IGuidGenerator _guidGenerator;

    public GuidanceDataSeedContributor(
        IRepository<Branch, Guid> branchRepository,
        IRepository<GuidanceSessionRule, Guid> ruleRepository,
        IGuidGenerator guidGenerator)
    {
        _branchRepository = branchRepository;
        _ruleRepository = ruleRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var branches = await _branchRepository.GetListAsync();
        
        foreach (var branch in branches)
        {
            if (await _ruleRepository.AnyAsync(r => r.BranchId == branch.Id))
            {
                continue;
            }

            // Create default rules for each branch: 9 AM - 5 PM, 30 mins
            var rule = new GuidanceSessionRule(
                _guidGenerator.Create(),
                branch.Id,
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                30
            );

            // Add standard work days (Mon-Fri)
            var workDays = new[] { 
                DayOfWeek.Monday, 
                DayOfWeek.Tuesday, 
                DayOfWeek.Wednesday, 
                DayOfWeek.Thursday, 
                DayOfWeek.Friday 
            };

            foreach (var day in workDays)
            {
                rule.AddWeekDay(_guidGenerator.Create(), day);
            }

            await _ruleRepository.InsertAsync(rule);
            Console.WriteLine($"--- SEEDED GUIDANCE RULES FOR BRANCH: {branch.Name} ---");
        }
    }
}
