using System;
using System.Threading.Tasks;
using Informatique.Alumni.Infrastructure.Email;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.TextTemplating;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Infrastructure.Email;

public class EmailSendingJob : AsyncBackgroundJob<EmailJobArgs>, ITransientDependency
{
    private readonly IEmailSender _emailSender;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IRepository<EmailDeliveryLog, Guid> _logRepository;
    private readonly IGuidGenerator _guidGenerator;

    public EmailSendingJob(
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer,
        IRepository<EmailDeliveryLog, Guid> logRepository,
        IGuidGenerator guidGenerator)
    {
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logRepository = logRepository;
        _guidGenerator = guidGenerator;
    }

    public override async Task ExecuteAsync(EmailJobArgs args)
    {
        bool isSuccess = false;
        string? errorMessage = null;
        string body = args.Body ?? string.Empty;

        try
        {
            if (!string.IsNullOrEmpty(args.TemplateName))
            {
                // Assuming simple model passing or dictionary. 
                // For complex objects, JSON serialization handling might be needed in a real scenario.
                body = await _templateRenderer.RenderAsync(args.TemplateName, args.TemplateModel);
            }

            await _emailSender.SendAsync(args.Recipient, args.Subject, body);
            isSuccess = true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            // In a real scenario, might want to re-throw to let background job retry 
            // OR swallow and log failure if retries are not desired for this type.
            // For now, logging failure.
        }
        finally
        {
            await _logRepository.InsertAsync(new EmailDeliveryLog(
                _guidGenerator.Create(),
                args.Recipient,
                args.Subject,
                isSuccess,
                errorMessage
            ));
        }
    }
}
