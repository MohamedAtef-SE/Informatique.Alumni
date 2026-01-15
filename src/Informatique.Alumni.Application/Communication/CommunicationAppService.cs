using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Directory;
using Informatique.Alumni.Permissions;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Communication;

[Authorize(AlumniPermissions.Communication.SendMassMessage)]
public class CommunicationAppService : AlumniAppService, ICommunicationAppService
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IRepository<AlumniDirectoryCache, Guid> _cacheRepository;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public CommunicationAppService(
        IBackgroundJobManager backgroundJobManager,
        IRepository<AlumniDirectoryCache, Guid> cacheRepository,
        IHtmlSanitizer htmlSanitizer)
    {
        _backgroundJobManager = backgroundJobManager;
        _cacheRepository = cacheRepository;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task QueueMassMessageAsync(SendMassMessageDto input)
    {
        // Security: Sanitize HTML content to prevent XSS
        var sanitizedContent = _htmlSanitizer.Sanitize(input.Content);

        var query = await _cacheRepository.GetQueryableAsync();
        
        if (input.TargetUserIds != null && input.TargetUserIds.Any())
        {
            query = query.Where(x => input.TargetUserIds.Contains(x.UserId));
        }

        query = query
            .WhereIf(!input.FilterMajor.IsNullOrWhiteSpace(), x => x.Major == input.FilterMajor)
            .WhereIf(input.FilterYear.HasValue, x => x.GraduationYear == input.FilterYear);

        var users = await AsyncExecuter.ToListAsync(query.Select(x => new { x.UserId, x.Email }));

        foreach (var user in users)
        {
            if (input.SendAsEmail)
            {
                // In a real system, you would have a specific JobArgs class
                // We'll queue a dummy job to demonstrate the queuing logic
                await _backgroundJobManager.EnqueueAsync(new MassEmailJobArgs
                {
                    UserId = user.UserId,
                    To = user.Email,
                    Subject = input.Subject,
                    Body = sanitizedContent // Use sanitized content
                });
            }
            
            // Similar logic for SMS...
        }
    }
}

public class MassEmailJobArgs
{
    public Guid UserId { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
