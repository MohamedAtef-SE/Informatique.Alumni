using System;
using System.Threading.Tasks;
using Informatique.Alumni.Payment;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Infrastructure.Payment;

public class MockPaymentGateway : IPaymentGateway, ITransientDependency
{
    public Task<string> InitiatePaymentAsync(decimal amount, string currency, string description)
    {
        // Simulate a transaction ID
        return Task.FromResult($"MOCK_TRX_{Guid.NewGuid()}");
    }

    public Task<bool> VerifyPaymentAsync(string transactionId)
    {
        // Always return true for mock
        return Task.FromResult(true);
    }

    public Task<string?> RefundAsync(string transactionId, decimal amount)
    {
        // Simulate refund ID
        return Task.FromResult($"MOCK_REF_{Guid.NewGuid()}");
    }
}
