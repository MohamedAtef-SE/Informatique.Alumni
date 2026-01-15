using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Sms;
using Volo.Abp.Guids; // For IGuidGenerator
using Volo.Abp.Uow;
using Volo.Abp.Linq;
using System.Linq;

namespace Informatique.Alumni.Communications;

public class GeneralMessageSenderJob : AsyncBackgroundJob<GeneralMessageSenderJobArgs>, ITransientDependency
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IRepository<CommunicationLog, Guid> _logRepository;
    private readonly IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> _membershipRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public GeneralMessageSenderJob(
        IRepository<AlumniProfile, Guid> profileRepository,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IRepository<CommunicationLog, Guid> logRepository,
        IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> membershipRepository,
        IGuidGenerator guidGenerator,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _profileRepository = profileRepository;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logRepository = logRepository;
        _membershipRepository = membershipRepository;
        _guidGenerator = guidGenerator;
        _asyncExecuter = asyncExecuter;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(GeneralMessageSenderJobArgs args)
    {
        // 1. Re-build Query (Simplistic duplication of logic for now)
        var query = await _profileRepository.GetQueryableAsync();
        
        // Filter Logic (Same as AppService)
        query = query.Where(p => p.BranchId == args.Filter.BranchId);
        
        if (args.Filter.GraduationYear.HasValue)
        {
             query = query.Where(p => p.Educations.OrderByDescending(e => e.GraduationYear).ThenByDescending(e => e.GraduationSemester).FirstOrDefault().GraduationYear == args.Filter.GraduationYear);
        }
        
        if (args.Filter.GraduationSemester.HasValue)
        {
             query = query.Where(p => p.Educations.OrderByDescending(e => e.GraduationYear).ThenByDescending(e => e.GraduationSemester).FirstOrDefault().GraduationSemester == args.Filter.GraduationSemester);
        }
        
        if (args.Filter.CollegeId.HasValue)
        {
             query = query.Where(p => p.Educations.OrderByDescending(e => e.GraduationYear).ThenByDescending(e => e.GraduationSemester).FirstOrDefault().CollegeId == args.Filter.CollegeId);
        }
        
        if (args.Filter.MajorId.HasValue)
        {
             query = query.Where(p => p.Educations.OrderByDescending(e => e.GraduationYear).ThenByDescending(e => e.GraduationSemester).FirstOrDefault().MajorId == args.Filter.MajorId);
        }

        // Membership Status Filter
        if (args.Filter.MembershipStatus.HasValue && args.Filter.MembershipStatus != CommunicationMembershipStatus.All)
        {
             var activeAlumniIds = (await _membershipRepository.GetQueryableAsync())
                .Where(r => r.Status == Informatique.Alumni.Membership.MembershipRequestStatus.Approved)
                .Select(r => r.AlumniId);

             if (args.Filter.MembershipStatus == CommunicationMembershipStatus.Active)
             {
                 query = query.Where(p => activeAlumniIds.Contains(p.Id));
             }
             else // Inactive
             {
                 query = query.Where(p => !activeAlumniIds.Contains(p.Id));
             }
        }

        // Execute Query
        var alumniList = await _asyncExecuter.ToListAsync(query);

        foreach (var alumni in alumniList)
        {
            try
            {
                await ProcessSingleAlumniAsync(alumni, args);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to send message to Alumni {alumni.Id}");
                await LogAsync(args.SenderId, alumni.Id, args.Channel, args.Subject, args.Body, "Failed", ex.Message);
            }
        }
    }

    private async Task ProcessSingleAlumniAsync(AlumniProfile alumni, GeneralMessageSenderJobArgs args)
    {
        string? targetAddress = null;

        // 2. Resolve Primary Contact
        if (args.Channel == CommunicationChannel.Email)
        {
            var primaryEmail = alumni.Emails.FirstOrDefault(e => e.IsPrimary);
            if (primaryEmail == null)
            {
                Logger.LogWarning($"Alumni {alumni.Id} has no primary email. Skipping.");
                await LogAsync(args.SenderId, alumni.Id, args.Channel, args.Subject, args.Body, "Skipped", "No Primary Email");
                return;
            }
            targetAddress = primaryEmail.Email; // Correct Property
            
            // Send Email
            await _emailSender.QueueAsync(targetAddress, args.Subject, args.Body); 
        }
        else // SMS
        {
            var primaryMobile = alumni.Mobiles.FirstOrDefault(m => m.IsPrimary);
            if (primaryMobile == null)
            {
                Logger.LogWarning($"Alumni {alumni.Id} has no primary mobile. Skipping.");
                await LogAsync(args.SenderId, alumni.Id, args.Channel, args.Subject, args.Body, "Skipped", "No Primary Mobile");
                return;
            }
            targetAddress = primaryMobile.MobileNumber; // Correct Property

            // Send SMS
            await _smsSender.SendAsync(new SmsMessage(targetAddress, args.Body));
        }

        // 3. Log Success
        await LogAsync(args.SenderId, alumni.Id, args.Channel, args.Subject, args.Body, "Sent", null);
    }

    private async Task LogAsync(Guid senderId, Guid recipientId, CommunicationChannel channel, string subject, string content, string status, string? error)
    {
        var log = new CommunicationLog(
            _guidGenerator.Create(),
            senderId,
            recipientId,
            channel.ToString(),
            subject,
            content,
            status
        );
        if (error != null) log.ErrorMessage = error;

        await _logRepository.InsertAsync(log);
    }
}
