using System.Threading.Tasks;
using Shouldly;
using Informatique.Alumni.Communications;
using Xunit;

namespace Informatique.Alumni.Communication;

public class CommunicationAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ICommunicationsAppService _communicationsAppService;

    public CommunicationAppServiceTests()
    {
        _communicationsAppService = GetRequiredService<ICommunicationsAppService>();
    }

    [Fact]
    public async Task SendMessageAsync_Should_Not_Throw()
    {
        // Act & Assert — Background job manager is mocked, so no SMTP/SMS call occurs.
        await Should.NotThrowAsync(async () =>
        {
            await _communicationsAppService.SendMessageAsync(new SendGeneralMessageInputDto
            {
                Subject = "Test Subject",
                Body = "Test Body",
                Channel = CommunicationChannel.Email,
                Filter = new AlumniCommunicationFilterDto()
            });
        });
    }
}
