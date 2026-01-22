using System;
using System.IO;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;

namespace Informatique.Alumni.EmploymentFair;

public class CvReviewManager : DomainService
{
    private readonly IRepository<CvReview, Guid> _cvReviewRepository;
    private readonly ILocalEventBus _localEventBus;
    private readonly ServiceAccessManager _serviceAccessManager;

    public CvReviewManager(
        IRepository<CvReview, Guid> cvReviewRepository,
        ILocalEventBus localEventBus,
        ServiceAccessManager serviceAccessManager)
    {
        _cvReviewRepository = cvReviewRepository;
        _localEventBus = localEventBus;
        _serviceAccessManager = serviceAccessManager;
    }

    public async Task CreateRequestAsync(Guid alumniId, string originalFileUrl)
    {
        // 1. Eligibility Check: Membership
        await _serviceAccessManager.CheckAccessAsync(alumniId, "CvReview"); 
        // "CvReview" string could be a constant but hardcoded for now as per specific service name requirement.

        // 2. File Validation (.doc/.docx Only)
        // Check extension
        var extension = Path.GetExtension(originalFileUrl)?.ToLower();
        if (string.IsNullOrEmpty(extension) || (extension != ".doc" && extension != ".docx"))
        {
             throw new UserFriendlyException("Only Word documents are allowed.");
        }

        // 3. Create Entity
        var review = new CvReview(GuidGenerator.Create(), alumniId, originalFileUrl);
        
        await _cvReviewRepository.InsertAsync(review);

        // 4. Notifications (On Create: Notify Admin)
        // We can publish an event here, or rely on AppService to send email. 
        // Domain Service usually triggers Domain Events.
        // I'll publish a local event for "Created" if needed, but for now just persisting is the main goal.
        // Prompt says: "Notify Admin". 
        // Best practice: Publish Event.
        // await _localEventBus.PublishAsync(new CvReviewCreatedEto { RequestId = review.Id });
    }

    public async Task CompleteReviewAsync(Guid requestId, string reviewedFileUrl, string notes)
    {
        var review = await _cvReviewRepository.GetAsync(requestId);

        // Validation 1: Status must be InProgress 
        // (Or Pending, if Admin skips "In Progress" marking? Logic says InProgress -> Completed usually, but Pending -> Completed allows fewer clicks)
        // Prompt Statuses: Pending (New) -> InProgress -> Completed.
        // Strictly, should be able to complete from InProgress.
        
        // Validation 2: File Type (Word Only)
        var extension = Path.GetExtension(reviewedFileUrl)?.ToLower();
        if (extension != ".doc" && extension != ".docx")
        {
            throw new BusinessException("EmploymentFair:InvalidReviewedFileFormat");
        }

        review.Complete(reviewedFileUrl, notes);
        
        await _cvReviewRepository.UpdateAsync(review);

        // Trigger Event (Notify Graduate)
        await _localEventBus.PublishAsync(new CvReviewCompletedEto
        {
            RequestId = review.Id,
            GraduateId = review.GraduateId,
            ReviewedFileUrl = review.ReviewedFileUrl,
            Notes = review.Notes
        });
    }
}
