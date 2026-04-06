using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;

namespace Informatique.Alumni.Import;

public class SendImportedAlumniWelcomeEmailJob : AsyncBackgroundJob<SendImportedAlumniWelcomeEmailJobArgs>, ITransientDependency
{
    private readonly IEmailSender _emailSender;

    public SendImportedAlumniWelcomeEmailJob(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public override async Task ExecuteAsync(SendImportedAlumniWelcomeEmailJobArgs args)
    {
        var subject = $"Welcome to Informatique Alumni Portal, {args.Name}!";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                <h2 style='color: #0056b3;'>Welcome, {args.Name}!</h2>
                <p>An administrator has created an account for you on the Informatique Alumni Portal.</p>
                <p>Here are your temporary login credentials:</p>
                <div style='background-color: #f4f4f4; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <p style='margin: 5px 0;'><strong>Email:</strong> {args.Email}</p>
                    <p style='margin: 5px 0;'><strong>Password:</strong> {args.Password}</p>
                </div>
                <p>We recommend signing in and updating your password as soon as possible.</p>
                <br/>
                <p>Best Regards,</p>
                <p><strong>Informatique Alumni Team</strong></p>
            </div>
        ";

        await _emailSender.SendAsync(args.Email, subject, body, isBodyHtml: true);
    }
}
