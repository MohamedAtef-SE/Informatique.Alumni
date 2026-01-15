using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Delivery.Strategies;

public class FeeCalculatorResolver : ITransientDependency
{
    private readonly IEnumerable<IFeeCalculatorStrategy> _strategies;

    public FeeCalculatorResolver(IEnumerable<IFeeCalculatorStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IFeeCalculatorStrategy Resolve(FeeStrategyType strategyType)
    {
        var strategy = _strategies.FirstOrDefault(s => s.StrategyType == strategyType);
        if (strategy == null)
        {
             // Fallback or throw
             throw new ArgumentException($"No strategy found for type {strategyType}");
        }
        return strategy;
    }
}
