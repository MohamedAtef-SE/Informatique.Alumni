using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;

namespace Informatique.Alumni.Payment;

[Authorize]
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IRepository<Informatique.Alumni.Payment.PaymentTransaction, Guid> _transactionRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly AlumniApplicationMappers _alumniMappers;

    public PaymentAppService(
        IRepository<Informatique.Alumni.Payment.PaymentTransaction, Guid> transactionRepository,
        IPaymentGateway paymentGateway,
        AlumniApplicationMappers alumniMappers)
    {
        _transactionRepository = transactionRepository;
        _paymentGateway = paymentGateway;
        _alumniMappers = alumniMappers;
    }

    public async Task<PaymentTransactionDto> CheckoutAsync(CheckoutDto input)
    {
        // 1. Create Pending Transaction
        var transaction = new Informatique.Alumni.Payment.PaymentTransaction(
            GuidGenerator.Create(),
            input.OrderId,
            input.Amount,
            input.Currency
        );

        await _transactionRepository.InsertAsync(transaction);

        try
        {
            // 2. Call Gateway
            var gatewayTxId = await _paymentGateway.InitiatePaymentAsync(input.Amount, input.Currency, input.Description);

            // 3. Mark as Completed (Simulated instant success for Mock)
            // In real scenario, this might happen via Webhook
            transaction.MarkAsCompleted(gatewayTxId);
            
            await _transactionRepository.UpdateAsync(transaction);
        }
        catch (Exception ex)
        {
            transaction.MarkAsFailed(ex.Message);
            await _transactionRepository.UpdateAsync(transaction);
            throw new UserFriendlyException("Payment initiation failed.", innerException: ex);
        }

        return _alumniMappers.MapToDto(transaction);
    }

    public async Task<PaymentTransactionDto> ProcessRefundAsync(Guid transactionId, decimal amount)
    {
        var originalTx = await _transactionRepository.GetAsync(transactionId);
        
        if (originalTx.Status != PaymentStatus.Completed)
        {
            throw new UserFriendlyException("Can only refund completed transactions.");
        }

        // 1. Call Gateway Refund
        var refundTxId = await _paymentGateway.RefundAsync(originalTx.GatewayTransactionId, amount);

        if (refundTxId == null)
        {
             throw new UserFriendlyException("Refund failed verify gateway.");
        }

        // 2. Create NEW Transaction for Refund (Immutable Ledger)
        var refundTransaction = new Informatique.Alumni.Payment.PaymentTransaction(
            GuidGenerator.Create(),
            originalTx.OrderId,
            -amount, // Negative amount
            originalTx.Currency,
            refundTxId
        );
        
        // Mark strictly as completed since it's a refund record
        refundTransaction.MarkAsCompleted(refundTxId);

        await _transactionRepository.InsertAsync(refundTransaction);
        
        return _alumniMappers.MapToDto(refundTransaction);
    }
}
