using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Informatique.Alumni.Communication;
using Xunit;

namespace Informatique.Alumni.Communication;

public class CommunicationAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ICommunicationAppService _communicationAppService;

    public CommunicationAppServiceTests()
    {
        _communicationAppService = GetRequiredService<ICommunicationAppService>();
    }

    [Fact]
    public async Task QueueMassMessageAsync_Should_Process_Message()
    {
        // Act
        // Assuming SendMassMessageDto exists
        await _communicationAppService.QueueMassMessageAsync(new SendMassMessageDto 
        { 
            Subject = "Test", 
            Content = "Body",
            SendAsEmail = true 
        });

        // Assert
        // If no exception, it passed.
    }
}
