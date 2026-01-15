using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Delivery;

public class ShipmentRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid ProviderId { get; private set; }
    public string TrackingNumber { get; private set; }
    public string RecipientName { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Fee { get; private set; }
    public ShipmentStatus Status { get; private set; }

    private ShipmentRequest() { }

    public ShipmentRequest(
        Guid id, 
        Guid providerId, 
        string recipientName, 
        string address, 
        string city,
        decimal weight,
        decimal fee) 
        : base(id)
    {
        ProviderId = providerId;
        RecipientName = recipientName;
        Address = address;
        City = city;
        Weight = weight;
        Fee = fee;
        Status = ShipmentStatus.Requested;
        TrackingNumber = string.Empty; // Assigned later or generated
    }

    public void SetTrackingNumber(string trackingNumber)
    {
        TrackingNumber = trackingNumber;
    }

    public void MarkAsPickedUp()
    {
        if (Status != ShipmentStatus.Requested)
        {
            throw new BusinessException("Shipment must be in 'Requested' state to be Picked Up.");
        }
        Status = ShipmentStatus.PickedUp;
    }

    public void MarkAsInTransit()
    {
        if (Status != ShipmentStatus.PickedUp)
        {
             throw new BusinessException("Shipment must be 'Picked Up' before moving to In Transit.");
        }
        Status = ShipmentStatus.InTransit;
    }

    public void MarkAsDelivered()
    {
        if (Status != ShipmentStatus.InTransit && Status != ShipmentStatus.PickedUp)
        {
            throw new BusinessException("Shipment must be In Transit or Picked Up to be Delivered.");
        }
        Status = ShipmentStatus.Delivered;
    }
    
    public void MarkAsReturned()
    {
         Status = ShipmentStatus.Returned;
    }
}
