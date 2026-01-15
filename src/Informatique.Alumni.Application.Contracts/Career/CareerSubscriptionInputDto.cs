using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CareerSubscriptionInputDto
{
    public Guid ActivityId { get; set; }
    public Guid TimeslotId { get; set; }
    public CareerPaymentMethod PaymentMethod { get; set; }
}
