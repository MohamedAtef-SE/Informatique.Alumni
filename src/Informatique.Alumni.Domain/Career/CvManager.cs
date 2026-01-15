using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Career;

public class CvManager : DomainService
{
    public CvManager() { }

    public void Approve(CurriculumVitae cv)
    {
        cv.Status = CvStatus.Approved;
    }

    public void Reject(CurriculumVitae cv, string reason)
    {
        cv.Status = CvStatus.Rejected;
        // In a real scenario, we might log the reason or send a notification
    }

    public void SubmitForAudit(CurriculumVitae cv)
    {
        cv.Status = CvStatus.PendingAudit;
    }
}
