using Shouldly;
using System.Threading.Tasks;
using Xunit;
using System;
using Informatique.Alumni.Payment;

namespace Informatique.Alumni.Payment;

public class PaymentAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IPaymentAppService _paymentAppService;

    public PaymentAppServiceTests()
    {
        _paymentAppService = GetRequiredService<IPaymentAppService>();
    }

    [Fact]
    public void Service_Should_Be_Resolvable()
    {
        _paymentAppService.ShouldNotBeNull();
    }
}
