using System;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Emailing;

namespace Informatique.Alumni.Career;

public class CareerSubscriptionCancelledEventHandler : 
    ILocalEventHandler<CareerSubscriptionCancelledEto>,
    ITransientDependency
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<ContactEmail, Guid> _emailRepository;
    private readonly IEmailSender _emailSender;
    private readonly Volo.Abp.Identity.IIdentityUserRepository _userRepository;

    public CareerSubscriptionCancelledEventHandler(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<ContactEmail, Guid> emailRepository,
        IEmailSender emailSender,
        Volo.Abp.Identity.IIdentityUserRepository userRepository)
    {
        _profileRepository = profileRepository;
        _emailRepository = emailRepository;
        _emailSender = emailSender;
        _userRepository = userRepository;
    }

    public async Task HandleEventAsync(CareerSubscriptionCancelledEto eventData)
    {
        // 1. Fetch Alumni Profile
        var profile = await _profileRepository.GetAsync(eventData.AlumniId);

        // 2. Fetch Primary Email
        var primaryEmail = await _emailRepository.FirstOrDefaultAsync(
            e => e.AlumniProfileId == profile.Id && e.IsPrimary
        );

        if (primaryEmail == null)
        {
            // No email to send to, log and return
            return;
        }

        // 3. Fetch User Details
        var user = await _userRepository.GetAsync(profile.UserId);

        // 4. Build Email Content
        var subject = "Career Service Subscription Cancelled";
        var body = BuildEmailBody(
            user.Name ?? "Alumni",
            eventData.ServiceName,
            eventData.CancellationReason,
            eventData.WasRefunded,
            eventData.RefundAmount
        );

        // 5. Send Email
        try
        {
            await _emailSender.SendAsync(
                primaryEmail.Email,
                subject,
                body,
                isBodyHtml: true
            );
        }
        catch (Exception)
        {
            // Log error but don't throw - notification failure shouldn't break cancellation
            // In production, use ILogger to log the exception
        }

        // TODO: Add SMS notification logic if required
    }

    private string BuildEmailBody(
        string alumniName,
        string serviceName,
        string cancellationReason,
        bool wasRefunded,
        decimal refundAmount)
    {
        var refundMessage = wasRefunded && refundAmount > 0
            ? $"<p><strong>Refund:</strong> An amount of {refundAmount:C} has been automatically refunded to your wallet.</p>"
            : refundAmount > 0
                ? $"<p><strong>Refund:</strong> A refund of {refundAmount:C} will be processed manually. Please contact the administration for details.</p>"
                : "";

        return $@"
            <html>
            <body>
                <h2>Career Service Subscription Cancelled</h2>
                <p>Dear {alumniName},</p>
                <p>Your subscription to the career service <strong>{serviceName}</strong> has been cancelled by the administration.</p>
                <p><strong>Reason:</strong> {cancellationReason}</p>
                {refundMessage}
                <p>If you have any questions, please contact the Alumni Affairs office.</p>
                <br/>
                <p>Best regards,</p>
                <p>Alumni Affairs Team</p>
            </body>
            </html>
        ";
    }
}
