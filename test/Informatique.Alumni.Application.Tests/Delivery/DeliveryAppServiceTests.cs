using System;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Delivery;

namespace Informatique.Alumni.Delivery;

public class DeliveryAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IDeliveryAppService _deliveryAppService;
    private readonly Volo.Abp.Domain.Repositories.IRepository<DeliveryProvider, Guid> _providerRepository;
    private readonly Volo.Abp.Domain.Repositories.IRepository<ShipmentRequest, Guid> _shipmentRepository;

    public DeliveryAppServiceTests()
    {
        _deliveryAppService = GetRequiredService<IDeliveryAppService>();
        _providerRepository = GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<DeliveryProvider, Guid>>();
        _shipmentRepository = GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<ShipmentRequest, Guid>>();
    }

    [Fact]
    public async Task RequestShipmentAsync_Should_Create_Request()
    {
        // Arrange
        // Use Local (0) as it is registered in TestModule
        var provider = new DeliveryProvider(Guid.NewGuid(), "FastShip", FeeStrategyType.Local)
        {
            IsActive = true
        };
        await _providerRepository.InsertAsync(provider);

        var input = new CreateShipmentDto
        {
            ProviderId = provider.Id,
            RecipientName = "Alice Smith",
            Address = "123 Main St",
            City = "Cairo",
            Weight = 2.5m 
        };

        // Act
        var result = await _deliveryAppService.RequestShipmentAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TrackingNumber.ShouldStartWith("TRK-");
        result.RecipientName.ShouldBe(input.RecipientName);
        
        var dbShipment = await _shipmentRepository.GetAsync(result.Id);
        dbShipment.ShouldNotBeNull();
    }

    [Fact]
    public async Task RequestShipmentAsync_Should_Throw_If_Provider_Inactive()
    {
        // Arrange
        var provider = new DeliveryProvider(Guid.NewGuid(), "SlowShip", FeeStrategyType.Local) 
        {
            IsActive = false
        };
        await _providerRepository.InsertAsync(provider);

        var input = new CreateShipmentDto
        {
            ProviderId = provider.Id,
            RecipientName = "Bob Jones",
            City = "Alexandria",
            Weight = 1.0m
        };

        // Act & Assert
        await Assert.ThrowsAsync<Volo.Abp.UserFriendlyException>(async () =>
        {
            await _deliveryAppService.RequestShipmentAsync(input);
        });
    }
}
