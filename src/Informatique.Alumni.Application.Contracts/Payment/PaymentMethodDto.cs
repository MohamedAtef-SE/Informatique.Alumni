using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Payment;

public class PaymentMethodDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public bool IsActive { get; set; }
}
