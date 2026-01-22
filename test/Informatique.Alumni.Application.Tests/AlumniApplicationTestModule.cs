using Volo.Abp.Modularity;
using Informatique.Alumni.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Informatique.Alumni;

[DependsOn(
    typeof(AlumniApplicationModule),
    typeof(AlumniDomainTestModule),
    typeof(AlumniEntityFrameworkCoreTestModule)
)]
public class AlumniApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Mock Student System Integration
        context.Services.AddSingleton<Informatique.Alumni.Profiles.IStudentSystemIntegrationService>(
            NSubstitute.Substitute.For<Informatique.Alumni.Profiles.IStudentSystemIntegrationService>()
        );

        // Mock Html Sanitizer
        context.Services.AddSingleton<Ganss.Xss.IHtmlSanitizer>(
            NSubstitute.Substitute.For<Ganss.Xss.IHtmlSanitizer>()
        );

        // Mock Payment Gateway
        context.Services.AddSingleton<Informatique.Alumni.Payment.IPaymentGateway>(
            NSubstitute.Substitute.For<Informatique.Alumni.Payment.IPaymentGateway>()
        );

        // Mock Background Job Manager
        context.Services.AddSingleton<Volo.Abp.BackgroundJobs.IBackgroundJobManager>(
            NSubstitute.Substitute.For<Volo.Abp.BackgroundJobs.IBackgroundJobManager>()
        );

        // Mock Blob Container (Generic Factory Mock)
        // Note: For IBlobContainer<T>, we need to ensure the container itself is resolvable or factory is mocked.
        // Simplest is to register open generic or specific.
        // Career uses IBlobContainer<AlumniBlobContainer>
        context.Services.AddSingleton<Volo.Abp.BlobStoring.IBlobContainer<Informatique.Alumni.BlobContainers.AlumniBlobContainer>>(
            NSubstitute.Substitute.For<Volo.Abp.BlobStoring.IBlobContainer<Informatique.Alumni.BlobContainers.AlumniBlobContainer>>()
        );
        // Job uses CvSnapshotBlobContainer
        context.Services.AddSingleton<Volo.Abp.BlobStoring.IBlobContainer<Informatique.Alumni.Career.CvSnapshotBlobContainer>>(
            NSubstitute.Substitute.For<Volo.Abp.BlobStoring.IBlobContainer<Informatique.Alumni.Career.CvSnapshotBlobContainer>>()
        );

        // Register Fee Strategies for Delivery Tests
        context.Services.AddTransient<Informatique.Alumni.Delivery.Strategies.IFeeCalculatorStrategy, Informatique.Alumni.Delivery.Strategies.LocalFeeStrategy>();
        // context.Services.AddTransient<Informatique.Alumni.Delivery.Strategies.IFeeCalculatorStrategy, Informatique.Alumni.Delivery.Strategies.AramexFeeStrategy>(); // If Aramex exists

    }
}
