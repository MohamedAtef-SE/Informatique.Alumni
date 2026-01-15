using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Career;

public class AlumniCareerSubscription : Entity<Guid>
{
    public Guid CareerServiceId { get; set; }
    public Guid AlumniId { get; set; }

    public Guid TimeslotId { get; set; }
    public CareerPaymentMethod PaymentMethod { get; set; }
    public CareerPaymentStatus PaymentStatus { get; set; }
    public decimal AmountPaid { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime RegistrationDate { get; set; }

    private AlumniCareerSubscription() { }

    public AlumniCareerSubscription(Guid id, Guid careerServiceId, Guid alumniId)
        : base(id)
    {
        CareerServiceId = careerServiceId;
        AlumniId = alumniId;

        RegistrationDate = DateTime.Now;
    }
    
    public void SetFinancialDetails(Guid timeslotId, CareerPaymentMethod method, decimal amount)
    {
        TimeslotId = timeslotId;
        PaymentMethod = method;
        AmountPaid = amount;
        PaymentStatus = CareerPaymentStatus.Pending; // Default
    }

    public void MarkAsPaid()
    {
        PaymentStatus = CareerPaymentStatus.Paid;
    }

    public void MarkAsCancelled(bool isRefunded)
    {
        PaymentStatus = CareerPaymentStatus.Cancelled;
        IsRefunded = isRefunded;
    }
}


