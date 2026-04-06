using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Payments;

/// <summary>
/// A simulated payment transaction processor mapped via Dependency Injection to instantly upgrade to Stripe or Paymob in the future.
/// </summary>
public class MockPaymentGateway : IPaymentGateway, ITransientDependency
{
    public async Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, string referenceId)
    {
        // Simulate real-world network transmission delay to a bank
        await Task.Delay(2000);

        // Always succeed for mock showcase purposes:
        return new PaymentGatewayResult
        {
            Success = true,
            TransactionId = $"txn_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            ErrorMessage = null
        };
    }
}
