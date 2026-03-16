using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Currencies;
using Informatique.Alumni.Lookups;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Lookups;

public interface ILookupAppService : IApplicationService
{
    Task<List<CurrencyDto>> GetCurrenciesAsync();
    Task<List<LookupItemDto>> GetTripTypesAsync();
}
