using System.Threading.Tasks;

namespace Informatique.Alumni.Payment;

public interface IPaymentGateway
{
    Task<string> InitiatePaymentAsync(decimal amount, string currency, string description);
    Task<bool> VerifyPaymentAsync(string transactionId);
    Task<string?> RefundAsync(string transactionId, decimal amount);
}
