using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;

namespace Informatique.Alumni.Career;

public class WorkshopEmailJobArgs
{
    public Guid ActivityId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class WorkshopEmailJob : AsyncBackgroundJob<WorkshopEmailJobArgs>
{
    public override Task ExecuteAsync(WorkshopEmailJobArgs args)
    {
        // Mock email sending to all attendees
        // In a real scenario, we would resolve the repository, get emails, and send via IEmailSender
        Console.WriteLine($"[BULK EMAIL] Sent to attendees of Activity {args.ActivityId}. Subject: {args.Subject}");
        return Task.CompletedTask;
    }
}
