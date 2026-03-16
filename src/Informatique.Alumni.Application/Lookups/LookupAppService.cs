using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Currencies;
using Informatique.Alumni.Lookups;
using Informatique.Alumni.Trips;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Lookups;

[AllowAnonymous]
public class LookupAppService : AlumniAppService, ILookupAppService
{
    private readonly IRepository<Currency, Guid> _currencyRepository;

    public LookupAppService(IRepository<Currency, Guid> currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<List<CurrencyDto>> GetCurrenciesAsync()
    {
        var currencies = await _currencyRepository.GetListAsync();
        return currencies
            .OrderBy(c => c.IsBase ? 0 : 1)
            .ThenBy(c => c.Code)
            .Select(c => new CurrencyDto
            {
                Id                 = c.Id,
                Code               = c.Code,
                Name               = c.Name,
                Symbol             = c.Symbol,
                FlagEmoji          = c.FlagEmoji,
                ExchangeRateFromUSD = c.ExchangeRateFromUSD,
                IsBase             = c.IsBase,
                LastSyncedAt       = c.LastSyncedAt
            })
            .ToList();
    }

    public Task<List<LookupItemDto>> GetTripTypesAsync()
    {
        var items = Enum.GetValues<TripType>()
            .Select(t => new LookupItemDto
            {
                Value = ((int)t).ToString(),
                Label = t.ToString()
            })
            .ToList();
        return Task.FromResult(items);
    }
}
