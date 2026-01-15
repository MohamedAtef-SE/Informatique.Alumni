namespace Informatique.Alumni.Delivery;

public enum ShipmentStatus
{
    Requested = 0,
    PickedUp = 1,
    InTransit = 2,
    Delivered = 3,
    Returned = 4,
    Cancelled = 5
}

public enum FeeStrategyType
{
    Local = 0,
    Aramex = 1,
    DHL = 2
}
