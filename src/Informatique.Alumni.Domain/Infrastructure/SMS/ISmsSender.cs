using System.Threading.Tasks;

namespace Informatique.Alumni.Infrastructure.SMS;

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message);
}
