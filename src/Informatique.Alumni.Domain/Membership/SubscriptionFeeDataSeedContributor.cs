using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Membership;

/// <summary>
/// Seeds the SubscriptionFee table with the current season's membership tiers.
/// This is REFERENCE DATA — runs in all environments including Production.
/// Idempotent: only inserts a fee if no active fee for the current year exists.
/// 
/// NOTE: When a new membership season starts, an admin should create the new fee
/// via the Admin UI and deactivate the previous one — NOT by modifying this seeder.
/// This seeder only ensures a valid fee exists so the platform can function on first run.
/// </summary>
public class SubscriptionFeeDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<SubscriptionFee, Guid> _feeRepository;
    private readonly IGuidGenerator _guidGenerator;

    public SubscriptionFeeDataSeedContributor(
        IRepository<SubscriptionFee, Guid> feeRepository,
        IGuidGenerator guidGenerator)
    {
        _feeRepository = feeRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var currentYear = DateTime.UtcNow.Year;

        // Idempotent: skip if an active fee for the current year already exists
        var alreadyExists = await _feeRepository.AnyAsync(f => f.Year == currentYear && f.IsActive);
        if (alreadyExists) return;

        var seasonStart = new DateTime(currentYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var seasonEnd   = new DateTime(currentYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Standard Membership — the base tier that covers most alumni
        await _feeRepository.InsertAsync(
            new SubscriptionFee(
                id:     _guidGenerator.Create(),
                name:   "Standard Membership",
                amount: 100m,
                year:   currentYear,
                start:  seasonStart,
                end:    seasonEnd
            ),
            autoSave: true
        );

        // Premium Membership — for alumni who want extra benefits
        await _feeRepository.InsertAsync(
            new SubscriptionFee(
                id:     _guidGenerator.Create(),
                name:   "Premium Membership",
                amount: 200m,
                year:   currentYear,
                start:  seasonStart,
                end:    seasonEnd
            ),
            autoSave: true
        );
    }
}
