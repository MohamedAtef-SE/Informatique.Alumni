using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Payments;

public interface IMembershipPaymentAppService : IApplicationService
{
    Task<MembershipCheckoutDto> ProcessDigitalCheckoutAsync();
}
