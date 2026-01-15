using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Delivery;

public class CreateShipmentDto
{
    public Guid ProviderId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}

public class ShipmentRequestDto : FullAuditedEntityDto<Guid>
{
    public Guid ProviderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Fee { get; set; }
    public ShipmentStatus Status { get; set; }
}
