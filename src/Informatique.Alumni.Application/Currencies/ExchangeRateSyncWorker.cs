using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Informatique.Alumni.Currencies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Informatique.Alumni.Currencies;

/// <summary>
/// Daily background worker that fetches exchange rates from the Frankfurter API
/// (European Central Bank data) and persists them to the Currency table.
/// Runs once on startup and then every 24 hours.
/// </summary>
public class ExchangeRateSyncWorker : AsyncPeriodicBackgroundWorkerBase
{
    private const string FrankfurterApiUrl = "https://api.frankfurter.app/latest?base=USD";

    public ExchangeRateSyncWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Run every 24 hours
        Timer.Period = 24 * 60 * 60 * 1000;
    }

    [UnitOfWork]
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("[ExchangeRateSyncWorker] Starting daily exchange rate sync from Frankfurter API...");

        try
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(30);

            var response = await http.GetFromJsonAsync<FrankfurterResponse>(FrankfurterApiUrl);
            if (response?.Rates == null || response.Rates.Count == 0)
            {
                Logger.LogWarning("[ExchangeRateSyncWorker] Received empty rates from Frankfurter API.");
                return;
            }

            var currencyRepo = workerContext.ServiceProvider.GetRequiredService<IRepository<Currency, Guid>>();
            var currencies = await currencyRepo.GetListAsync();

            int updatedCount = 0;
            foreach (var currency in currencies)
            {
                if (currency.IsBase) continue; // USD — always 1.0

                if (response.Rates.TryGetValue(currency.Code, out var rate))
                {
                    currency.UpdateRate((decimal)rate);
                    await currencyRepo.UpdateAsync(currency);
                    updatedCount++;
                }
                else
                {
                    Logger.LogWarning("[ExchangeRateSyncWorker] No rate found for currency: {Code}", currency.Code);
                }
            }

            Logger.LogInformation("[ExchangeRateSyncWorker] Sync complete. Updated {Count} currencies.", updatedCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ExchangeRateSyncWorker] Failed to sync exchange rates.");
        }
    }

    // Frankfurter API response shape
    private class FrankfurterResponse
    {
        [JsonPropertyName("base")]
        public string Base { get; set; } = default!;

        [JsonPropertyName("date")]
        public string Date { get; set; } = default!;

        [JsonPropertyName("rates")]
        public Dictionary<string, double> Rates { get; set; } = new();
    }
}
