using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Events;

public class AlumniEventRegistration : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public Guid EventId { get; private set; }
    public string TicketCode { get; private set; } = string.Empty;
    public RegistrationStatus Status { get; private set; }

    public Guid? TimeslotId { get; private set; }
    public string? PaymentMethod { get; private set; }
    public decimal? PaidAmount { get; private set; }
    public bool IsRefunded { get; private set; }

    private AlumniEventRegistration() { }

    public AlumniEventRegistration(
        Guid id, 
        Guid alumniId, 
        Guid eventId, 
        string ticketCode, 
        Guid? timeslotId = null,
        string? paymentMethod = null,
        decimal? paidAmount = null)
        : base(id)
    {
        AlumniId = Check.NotDefaultOrNull<Guid>(alumniId, nameof(alumniId));
        EventId = Check.NotDefaultOrNull<Guid>(eventId, nameof(eventId));
        TicketCode = Check.NotNullOrWhiteSpace(ticketCode, nameof(ticketCode));
        Status = RegistrationStatus.Registered;
        TimeslotId = timeslotId;
        PaymentMethod = paymentMethod;
        PaidAmount = paidAmount;
    }

    public void MarkAsAttended()
    {
        if (Status == RegistrationStatus.Cancelled)
        {
            throw new UserFriendlyException("Cannot mark a cancelled registration as attended.");
        }
        
        if (Status == RegistrationStatus.Attended)
        {
            throw new UserFriendlyException("This ticket has already been used.");
        }

        Status = RegistrationStatus.Attended;
    }

    public void Cancel()
    {
        Status = RegistrationStatus.Cancelled;
    }

    public void MarkAsRefunded()
    {
        if (Status != RegistrationStatus.Cancelled)
        {
            throw new BusinessException("Alumni:EventRegistration:CannotRefundActive")
                .WithData("Reason", "Cannot mark as refunded unless registration is cancelled");
        }
        IsRefunded = true;
    }
}
