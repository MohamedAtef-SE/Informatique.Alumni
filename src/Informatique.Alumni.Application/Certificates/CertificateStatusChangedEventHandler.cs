using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus.Distributed;

namespace Informatique.Alumni.Certificates;

public class CertificateStatusChangedEventHandler : IDistributedEventHandler<EntityUpdatedEto<CertificateRequest>>, ITransientDependency
{
    private readonly IEmailSender _emailSender;
    private readonly Volo.Abp.Identity.IdentityUserManager _userManager;
    private readonly IRepository<CertificateRequest, Guid> _repository; // Inject repo to get fresh data if needed

    public CertificateStatusChangedEventHandler(
        IEmailSender emailSender,
        Volo.Abp.Identity.IdentityUserManager userManager,
        IRepository<CertificateRequest, Guid> repository)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _repository = repository;
    }

    public async Task HandleEventAsync(EntityUpdatedEto<CertificateRequest> eventData)
    {
        var oldStatus = eventData.Entity.Status; // This might not be available in standard ETO if it's not tracking old values? 
        // Actually EntityUpdatedEto usually contains the NEW state. 
        // To compare, we might need to fetch the old state or use a custom event.
        // But for now, let's just notify based on the NEW status.
        
        var request = eventData.Entity;
        var user = await _userManager.GetByIdAsync(request.AlumniId);
        
        if (user == null) return;

        string subject = $"Certificate Request Update - {request.Status}";
        string body = $"Dear {user.Name},<br/><br/>Your certificate request status has been updated to: <b>{request.Status}</b>.<br/><br/>";

        if (request.Status == CertificateRequestStatus.ReadyForPickup)
        {
            body += "Your certificate is ready for pickup at your selected branch.<br/>";
        }
        else if (request.Status == CertificateRequestStatus.OutForDelivery)
        {
            body += "Your certificate is out for delivery.<br/>";
        }
        else if (request.Status == CertificateRequestStatus.Rejected)
        {
            body += $"Reason: {request.AdminNotes}<br/>";
        }

        body += "<br/>Best regards,<br/>Alumni Association";

        await _emailSender.SendAsync(user.Email, subject, body);
    }
}
