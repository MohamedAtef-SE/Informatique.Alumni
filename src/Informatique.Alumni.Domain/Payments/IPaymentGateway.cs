using System.Threading.Tasks;

namespace Informatique.Alumni.Payments;

/// <summary>
/// Domain-Driven Design strategy interface abstracting away third-party payment integrations.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, string referenceId);
}
