using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Delivery.Strategies;

public class AramexFeeStrategy : IFeeCalculatorStrategy, ITransientDependency
{
    public FeeStrategyType StrategyType => FeeStrategyType.Aramex;

    public Task<decimal> CalculateFeeAsync(string city, decimal weight)
    {
        // Mock external API call
        // Typically Aramex charges more
        decimal rate = 150; 
        return Task.FromResult(rate + (weight * 20));
    }
}
