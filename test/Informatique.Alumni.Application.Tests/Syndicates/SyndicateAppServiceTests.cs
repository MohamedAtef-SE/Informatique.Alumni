using System;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Domain.Repositories;
using Shouldly;
using Informatique.Alumni.Syndicates;

namespace Informatique.Alumni.Syndicates;

public class SyndicateAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    public SyndicateAppServiceTests()
    {
    }

    [Fact]
    public async Task Should_Create_Syndicate()
    {
        var syndicateAppService = GetRequiredService<ISyndicateAppService>();
        var syndicateRepository = GetRequiredService<IRepository<Syndicate, Guid>>();

        // Act
        var syndicateDto = await syndicateAppService.CreateSyndicateAsync(new CreateUpdateSyndicateDto
        {
            Name = "Engineers Syndicate",
            Description = "For Engineers",
            Requirements = "ID,Degree"
        });

        // Assert
        syndicateDto.ShouldNotBeNull();
        syndicateDto.Name.ShouldBe("Engineers Syndicate");

        var syndicate = await syndicateRepository.GetAsync(syndicateDto.Id);
        syndicate.ShouldNotBeNull();
        syndicate.Name.ShouldBe("Engineers Syndicate");
    }

    [Fact]
    public async Task Should_Apply_For_Syndicate()
    {
        var syndicateAppService = GetRequiredService<ISyndicateAppService>();
        
        // Arrange
        var syndicateDto = await syndicateAppService.CreateSyndicateAsync(new CreateUpdateSyndicateDto
        {
            Name = "Medical Syndicate",
            Description = "For Doctors",
            Requirements = "ID,License"
        });

        // Act
        var subscriptionDto = await syndicateAppService.ApplyAsync(new ApplySyndicateDto
        {
            SyndicateId = syndicateDto.Id
        });

        // Assert
        subscriptionDto.ShouldNotBeNull();
        subscriptionDto.SyndicateId.ShouldBe(syndicateDto.Id);
        subscriptionDto.Status.ShouldBe(SyndicateStatus.Pending);
    }

    [Fact]
    public void Should_Resolve_Mappers()
    {
        GetRequiredService<AlumniApplicationMappers>().ShouldNotBeNull();
    }

    [Fact]
    public void Should_Resolve_BlobContainer()
    {
        GetRequiredService<Volo.Abp.BlobStoring.IBlobContainer<SyndicateBlobContainer>>().ShouldNotBeNull();
    }

    [Fact]
    public void Should_Resolve_Manager()
    {
        GetRequiredService<SyndicateManager>().ShouldNotBeNull();
    }
}
