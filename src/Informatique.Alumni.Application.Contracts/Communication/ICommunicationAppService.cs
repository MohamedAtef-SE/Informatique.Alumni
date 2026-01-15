using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Communication;

public interface ICommunicationAppService : IApplicationService
{
    Task QueueMassMessageAsync(SendMassMessageDto input);
}
