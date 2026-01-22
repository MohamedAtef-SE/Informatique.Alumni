using System;
using Volo.Abp.EventBus;

namespace Informatique.Alumni.EmploymentFair;

[EventName("Informatique.Alumni.EmploymentFair.CvReviewCompleted")]
public class CvReviewCompletedEto
{
    public Guid RequestId { get; set; }
    public Guid GraduateId { get; set; }
    public string ReviewedFileUrl { get; set; }
    public string Notes { get; set; }
}
