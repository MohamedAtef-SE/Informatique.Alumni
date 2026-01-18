using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Infrastructure.Email;
using Informatique.Alumni.Profiles;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace Informatique.Alumni.Events;

/// <summary>
/// Background job to send bulk emails to event participants.
/// Business Rules:
/// - Sends only to primary email contacts (IsPrimary = true)
/// - Downloads attachments from blob storage
/// - Logs every email attempt to EmailDeliveryLog
/// </summary>
public class EventEmailSenderJob : AsyncBackgroundJob<EventEmailJobArgs>, ITransientDependency
{
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<ContactEmail, Guid> _contactEmailRepository;
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<EmailDeliveryLog, Guid> _emailLogRepository;
    private readonly IEmailSender _emailSender;
    private readonly IBlobContainer<EventAttachmentsBlobContainer> _blobContainer;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<EventEmailSenderJob> _logger;

    public EventEmailSenderJob(
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<ContactEmail, Guid> contactEmailRepository,
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<EmailDeliveryLog, Guid> emailLogRepository,
        IEmailSender emailSender,
        IBlobContainer<EventAttachmentsBlobContainer> blobContainer,
        IGuidGenerator guidGenerator,
        ILogger<EventEmailSenderJob> logger)
    {
        _registrationRepository = registrationRepository;
        _profileRepository = profileRepository;
        _contactEmailRepository = contactEmailRepository;
        _eventRepository = eventRepository;
        _emailLogRepository = emailLogRepository;
        _emailSender = emailSender;
        _blobContainer = blobContainer;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(EventEmailJobArgs args)
    {
        _logger.LogInformation($"Starting bulk email job for EventId: {args.EventId}");

        try
        {
            // 1. Validate event exists
            var eventEntity = await _eventRepository.GetAsync(args.EventId);
            
            // 2. Get all registrations for this event
            var registrations = await _registrationRepository.GetListAsync(r => r.EventId == args.EventId);
            
            if (!registrations.Any())
            {
                _logger.LogWarning($"No registrations found for EventId: {args.EventId}");
                return;
            }

            _logger.LogInformation($"Found {registrations.Count} registrations for event");

            // 3. Get alumni IDs
            var alumniIds = registrations.Select(r => r.AlumniId).Distinct().ToList();

            // 4. Fetch primary email contacts for all alumni
            var primaryEmails = await _contactEmailRepository.GetListAsync(
                ce => alumniIds.Contains(ce.AlumniProfileId) && ce.IsPrimary);

            if (!primaryEmails.Any())
            {
                _logger.LogWarning($"No primary emails found for event participants");
                return;
            }

            _logger.LogInformation($"Found {primaryEmails.Count} primary email contacts");

            // 5. Download attachments from blob storage
            var attachments = new List<(string FileName, byte[] Content)>();
            
            if (args.AttachmentBlobNames != null && args.AttachmentBlobNames.Any())
            {
                foreach (var blobName in args.AttachmentBlobNames)
                {
                    try
                    {
                        var blobBytes = await _blobContainer.GetAllBytesAsync(blobName);
                        attachments.Add((blobName, blobBytes));
                        _logger.LogInformation($"Downloaded attachment: {blobName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to download attachment: {blobName}");
                    }
                }
            }

            // 6. Send emails to each primary contact
            int successCount = 0;
            int failureCount = 0;

            foreach (var contactEmail in primaryEmails)
            {
                try
                {
                    // Send email (ABP's IEmailSender typically handles text emails)
                    // For attachments, we would need to use a more advanced implementation
                    // or create MailMessage directly
                    await _emailSender.SendAsync(
                        to: contactEmail.Email,
                        subject: args.Subject,
                        body: args.Body,
                        isBodyHtml: true
                    );

                    // Log success
                    await _emailLogRepository.InsertAsync(new EmailDeliveryLog(
                        _guidGenerator.Create(),
                        contactEmail.Email,
                        args.Subject,
                        isSuccess: true
                    ));

                    successCount++;
                    _logger.LogInformation($"Email sent successfully to: {contactEmail.Email}");
                }
                catch (Exception ex)
                {
                    // Log failure
                    await _emailLogRepository.InsertAsync(new EmailDeliveryLog(
                        _guidGenerator.Create(),
                        contactEmail.Email,
                        args.Subject,
                        isSuccess: false,
                        errorMessage: ex.Message
                    ));

                    failureCount++;
                    _logger.LogError(ex, $"Failed to send email to: {contactEmail.Email}");
                }
            }

            _logger.LogInformation(
                $"Bulk email job completed. Success: {successCount}, Failures: {failureCount}");

            // 7. Clean up blob storage (optional - delete temp attachments)
            if (args.AttachmentBlobNames != null)
            {
                foreach (var blobName in args.AttachmentBlobNames)
                {
                    try
                    {
                        await _blobContainer.DeleteAsync(blobName);
                        _logger.LogInformation($"Deleted temp attachment: {blobName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete temp attachment: {blobName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Critical error in bulk email job for EventId: {args.EventId}");
            throw;
        }
    }
}
