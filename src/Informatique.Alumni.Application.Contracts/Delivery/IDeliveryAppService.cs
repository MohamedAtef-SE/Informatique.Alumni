using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Delivery;

public interface IDeliveryAppService : IApplicationService
{
    Task<ShipmentRequestDto> RequestShipmentAsync(CreateShipmentDto input);
    Task<ShipmentRequestDto> UpdateStatusAsync(Guid id, ShipmentStatus newStatus);
    Task<string> GetShippingLabelUrlAsync(Guid id); // Mock batch printing
}
