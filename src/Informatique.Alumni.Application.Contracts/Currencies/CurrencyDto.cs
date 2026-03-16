using System;

namespace Informatique.Alumni.Currencies;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Symbol { get; set; } = default!;
    public string FlagEmoji { get; set; } = default!;
    public decimal ExchangeRateFromUSD { get; set; }
    public bool IsBase { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
