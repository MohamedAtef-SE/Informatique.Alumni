using System;
using System.Threading.Tasks;
using Informatique.Alumni.Currencies;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Data;

/// <summary>
/// Seeds the Currency lookup table with default supported currencies.
/// Prices are stored in USD; ExchangeRateFromUSD is updated daily by ExchangeRateSyncWorker.
/// </summary>
public class CurrencyDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Currency, Guid> _currencyRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CurrencyDataSeeder(
        IRepository<Currency, Guid> currencyRepository,
        IGuidGenerator guidGenerator)
    {
        _currencyRepository = currencyRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Skip if already seeded
        if (await _currencyRepository.GetCountAsync() > 0) return;

        var currencies = new[]
        {
            // IsBase = true → ExchangeRateSyncWorker will skip this one (always 1.0)
            new Currency(_guidGenerator.Create(), "USD", "US Dollar",        "$",   "🇺🇸", 1.0m,    isBase: true),
            new Currency(_guidGenerator.Create(), "EGP", "Egyptian Pound",   "ج.م", "🇪🇬", 49.90m,  isBase: false),
            new Currency(_guidGenerator.Create(), "EUR", "Euro",             "€",   "🇪🇺", 0.923m,  isBase: false),
            new Currency(_guidGenerator.Create(), "GBP", "British Pound",    "£",   "🇬🇧", 0.793m,  isBase: false),
            new Currency(_guidGenerator.Create(), "SAR", "Saudi Riyal",      "﷼",   "🇸🇦", 3.750m,  isBase: false),
            new Currency(_guidGenerator.Create(), "AED", "UAE Dirham",       "د.إ", "🇦🇪", 3.672m,  isBase: false),
            new Currency(_guidGenerator.Create(), "KWD", "Kuwaiti Dinar",    "د.ك", "🇰🇼", 0.308m,  isBase: false),
        };

        foreach (var currency in currencies)
        {
            await _currencyRepository.InsertAsync(currency, autoSave: true);
        }
    }
}
