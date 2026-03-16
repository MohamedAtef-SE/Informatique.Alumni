using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Currencies;

/// <summary>
/// Lookup table for supported currencies.
/// Prices are stored in USD. ExchangeRateFromUSD is updated daily by ExchangeRateSyncWorker.
/// </summary>
public class Currency : Entity<Guid>
{
    /// <summary>ISO 4217 code, e.g. "USD", "EGP", "EUR"</summary>
    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>e.g. "$", "ج.م"</summary>
    public string Symbol { get; private set; } = default!;

    /// <summary>Flag emoji, e.g. "🇺🇸"</summary>
    public string FlagEmoji { get; private set; } = default!;

    /// <summary>How many units of this currency equal 1 USD. Base (USD) = 1.0</summary>
    public decimal ExchangeRateFromUSD { get; private set; } = 1m;

    public bool IsBase { get; private set; }

    public DateTime? LastSyncedAt { get; private set; }

    protected Currency() { }

    public Currency(Guid id, string code, string name, string symbol, string flagEmoji,
        decimal exchangeRateFromUSD, bool isBase = false)
        : base(id)
    {
        Code            = Check.NotNullOrWhiteSpace(code,   nameof(code),   CurrencyConsts.MaxCodeLength);
        Name            = Check.NotNullOrWhiteSpace(name,   nameof(name),   CurrencyConsts.MaxNameLength);
        Symbol          = Check.NotNullOrWhiteSpace(symbol, nameof(symbol), CurrencyConsts.MaxSymbolLength);
        FlagEmoji       = flagEmoji;
        ExchangeRateFromUSD = exchangeRateFromUSD > 0
            ? exchangeRateFromUSD
            : throw new ArgumentException("Exchange rate must be positive.", nameof(exchangeRateFromUSD));
        IsBase = isBase;
    }

    /// <summary>Called by ExchangeRateSyncWorker after fetching latest rates.</summary>
    public void UpdateRate(decimal newRate)
    {
        if (IsBase) return; // Base currency rate is always 1.0
        ExchangeRateFromUSD = newRate > 0
            ? newRate
            : throw new ArgumentException("Exchange rate must be positive.");
        LastSyncedAt = DateTime.UtcNow;
    }
}
