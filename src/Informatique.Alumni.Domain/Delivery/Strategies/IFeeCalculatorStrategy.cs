using System.Threading.Tasks;

namespace Informatique.Alumni.Delivery.Strategies;

public interface IFeeCalculatorStrategy
{
    FeeStrategyType StrategyType { get; }
    Task<decimal> CalculateFeeAsync(string city, decimal weight);
}
