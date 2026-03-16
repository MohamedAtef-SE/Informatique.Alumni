using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus;
using Informatique.Alumni.Profiles;
using Volo.Abp.Identity;
using Microsoft.Extensions.Logging;

namespace Informatique.Alumni.Guidance.EventHandlers;

public class AdvisingRequestStatusChangedEventHandler : ILocalEventHandler<AdvisingRequestStatusChangedEto>, ITransientDependency
{
    private readonly IEmailSender _emailSender;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<AdvisingRequest, Guid> _requestRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<AdvisingRequestStatusChangedEventHandler> _logger;

    public AdvisingRequestStatusChangedEventHandler(
        IEmailSender emailSender,
        IdentityUserManager userManager,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<AdvisingRequest, Guid> requestRepository,
        Microsoft.Extensions.Logging.ILogger<AdvisingRequestStatusChangedEventHandler> logger)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _profileRepository = profileRepository;
        _requestRepository = requestRepository;
        _logger = logger;
    }

    public async Task HandleEventAsync(AdvisingRequestStatusChangedEto eventData)
    {
        _logger.LogInformation($"[EMAIL HANDLER TRACE] HandleEventAsync invoked for Request ID: {eventData.RequestId}, Status: {eventData.NewStatus}");
        
        try
        {
            if (eventData.NewStatus != AdvisingRequestStatus.Approved && eventData.NewStatus != AdvisingRequestStatus.Rejected)
            {
                return; // We only send emails on direct approval/rejection.
            }

            // 1. Fetch Request to double check details, especially AdvisorId
            var request = await _requestRepository.FirstOrDefaultAsync(r => r.Id == eventData.RequestId);
            if (request == null) 
            {
                _logger.LogError($"[EMAIL HANDLER ERROR] Could not find AdvisingRequest with ID {eventData.RequestId}");
                return;
            }

            // 2. Fetch Alumni User and Profile Emails
            var profileQuery = await _profileRepository.WithDetailsAsync(x => x.Emails);
            var fullAlumniProfile = profileQuery.FirstOrDefault(p => p.Id == request.AlumniId);
            
            if (fullAlumniProfile == null)
            {
                _logger.LogError($"[EMAIL HANDLER ERROR] Could not find AlumniProfile with ID {request.AlumniId}");
                return;
            }
            
            var alumniUser = await _userManager.FindByIdAsync(fullAlumniProfile.UserId.ToString());
            if (alumniUser == null)
            {
                _logger.LogError($"[EMAIL HANDLER ERROR] Could not find IdentityUser for AlumniProfile UserId {fullAlumniProfile.UserId}");
                return;
            }

            string targetEmail = alumniUser.Email;
            var primaryEmailObj = fullAlumniProfile.Emails?.FirstOrDefault(e => e.IsPrimary);
            if (primaryEmailObj != null && !string.IsNullOrWhiteSpace(primaryEmailObj.Email))
            {
                targetEmail = primaryEmailObj.Email;
            }

            // 3. Fetch Advisor User
            var advisorProfile = await _profileRepository.FirstOrDefaultAsync(p => p.Id == request.AdvisorId);
            var advisorUserId = advisorProfile?.UserId ?? request.AdvisorId; 
            var advisorUser = await _userManager.FindByIdAsync(advisorUserId.ToString());
            
            var advisorName = "an advisor";
            if (advisorUser != null)
            {
                if (advisorUser.ExtraProperties.TryGetValue("Name", out var nObj) && nObj != null)
                    advisorName = nObj.ToString();
                else if (!string.IsNullOrWhiteSpace(advisorUser.Name))
                    advisorName = $"{advisorUser.Name} {advisorUser.Surname}".Trim();
                else
                    advisorName = advisorUser.UserName;
            }

            string subject = $"Guidance Session Update: {eventData.NewStatus}";
            
            string alumniName = "Alumni";
            if (alumniUser.ExtraProperties.TryGetValue("Name", out var anObj) && anObj != null)
                alumniName = anObj.ToString();
            else if (!string.IsNullOrWhiteSpace(alumniUser.Name))
                alumniName = $"{alumniUser.Name} {alumniUser.Surname}".Trim();
            else if (!string.IsNullOrWhiteSpace(alumniUser.UserName))
                alumniName = alumniUser.UserName;

            string body = $"Dear {alumniName},<br/><br/>";
            body += $"Your guidance session request regarding <b>'{request.Subject}'</b> has been updated.<br/><br/>";
            body += $"<b>New Status:</b> {eventData.NewStatus}<br/><br/>";

            if (eventData.NewStatus == AdvisingRequestStatus.Approved)
            {
                body += $"Your session with <b>{advisorName}</b> is confirmed for <b>{request.StartTime.ToString("f")}</b>.<br/>";
                
                if (!string.IsNullOrWhiteSpace(request.MeetingLink))
                {
                    body += $"- <b>Meeting Link:</b> <a href=\"{request.MeetingLink}\">{request.MeetingLink}</a><br/>";
                }
                else
                {
                    body += $"- <b>Location:</b> Online<br/>";
                }
                
                if (!string.IsNullOrWhiteSpace(request.AdminNotes))
                {
                    body += $"- <b>Notes:</b> {request.AdminNotes}<br/>";
                }
            }
            else if (eventData.NewStatus == AdvisingRequestStatus.Rejected)
            {
                body += $"Unfortunately, this session could not be scheduled at this time.<br/>";
                if (!string.IsNullOrWhiteSpace(request.AdminNotes))
                {
                    body += $"<b>Reason:</b> {request.AdminNotes}<br/>";
                }
            }

            body += "<br/>Best regards,<br/>Informatique Alumni Association";

            _logger.LogInformation($"[EMAIL HANDLER SUCCESS] Attempting to send '{subject}' to {targetEmail}...");
            await _emailSender.SendAsync(targetEmail, subject, body);
            _logger.LogInformation($"[EMAIL HANDLER SUCCESS] Email payload handed off to IEmailSender successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[EMAIL HANDLER FATAL] Exception caught: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
