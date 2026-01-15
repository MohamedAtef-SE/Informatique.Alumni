using System;
using System.Threading.Tasks;
using Informatique.Alumni.Delivery.Strategies;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Delivery;

[Authorize]
public class DeliveryAppService : ApplicationService, IDeliveryAppService
{
    private readonly IRepository<ShipmentRequest, Guid> _shipmentRepository;
    private readonly IRepository<DeliveryProvider, Guid> _providerRepository;
    private readonly FeeCalculatorResolver _feeResolver;
    private readonly AlumniApplicationMappers _alumniMappers;

    public DeliveryAppService(
        IRepository<ShipmentRequest, Guid> shipmentRepository,
        IRepository<DeliveryProvider, Guid> providerRepository,
        FeeCalculatorResolver feeResolver,
        AlumniApplicationMappers alumniMappers)
    {
        _shipmentRepository = shipmentRepository;
        _providerRepository = providerRepository;
        _feeResolver = feeResolver;
        _alumniMappers = alumniMappers;
    }

    public async Task<ShipmentRequestDto> RequestShipmentAsync(CreateShipmentDto input)
    {
        // 1. Get Provider
        var provider = await _providerRepository.GetAsync(input.ProviderId);
        if (!provider.IsActive)
        {
            throw new UserFriendlyException("Selected provider is not active.");
        }

        // 2. Calculate Fee using Strategy Pattern
        var strategy = _feeResolver.Resolve(provider.FeeStrategyType);
        var fee = await strategy.CalculateFeeAsync(input.City, input.Weight);

        // 3. Create Shipment
        var shipment = new ShipmentRequest(
            GuidGenerator.Create(),
            input.ProviderId,
            input.RecipientName,
            input.Address,
            input.City,
            input.Weight,
            fee
        );
        
        // Mock Tracking Number generation
        shipment.SetTrackingNumber($"TRK-{DateTime.Now.Ticks}-{input.City.Substring(0, 3).ToUpper()}");

        await _shipmentRepository.InsertAsync(shipment);

        return _alumniMappers.MapToDto(shipment);
    }

    public async Task<ShipmentRequestDto> UpdateStatusAsync(Guid id, ShipmentStatus newStatus)
    {
        var shipment = await _shipmentRepository.GetAsync(id);

        switch (newStatus)
        {
            case ShipmentStatus.PickedUp:
                shipment.MarkAsPickedUp();
                break;
            case ShipmentStatus.InTransit:
                shipment.MarkAsInTransit();
                break;
            case ShipmentStatus.Delivered:
                shipment.MarkAsDelivered();
                break;
             case ShipmentStatus.Returned:
                shipment.MarkAsReturned();
                break;
            default:
                // No specific logic for others yet
                break;
        }

        await _shipmentRepository.UpdateAsync(shipment);
        return _alumniMappers.MapToDto(shipment);
    }

    public Task<string> GetShippingLabelUrlAsync(Guid id)
    {
        // Mock URL for PDF
        return Task.FromResult($"https://api.alumni.com/shipping/labels/{id}.pdf");
    }
}
