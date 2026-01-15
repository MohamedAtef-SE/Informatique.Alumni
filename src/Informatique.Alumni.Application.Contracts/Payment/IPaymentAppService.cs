using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Payment;

public interface IPaymentAppService : IApplicationService
{
    Task<PaymentTransactionDto> CheckoutAsync(CheckoutDto input);
    Task<PaymentTransactionDto> ProcessRefundAsync(Guid transactionId, decimal amount);
}
