using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Delivery.Strategies;

public class LocalFeeStrategy : IFeeCalculatorStrategy, ITransientDependency
{
    public FeeStrategyType StrategyType => FeeStrategyType.Local;

    public Task<decimal> CalculateFeeAsync(string city, decimal weight)
    {
        // Simple logic: Base fee + weight logic
        // Could be a DB lookup in real scenario
        decimal baseFee = 50; // 50 EGP
        if (weight > 1)
        {
            baseFee += (weight - 1) * 10; // +10 per extra kg
        }
        return Task.FromResult(baseFee);
    }
}
