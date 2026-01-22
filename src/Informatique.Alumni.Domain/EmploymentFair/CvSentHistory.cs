using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.EmploymentFair;

public class CvSentHistory : AuditedEntity<Guid>
{
    public Guid AlumniId { get; private set; }
    public string CompanyEmail { get; private set; }
    public string Subject { get; private set; }
    public DateTime SentDate { get; private set; }

    protected CvSentHistory() { }

    public CvSentHistory(Guid id, Guid alumniId, string companyEmail, string subject, DateTime sentDate)
        : base(id)
    {
        AlumniId = alumniId;
        CompanyEmail = companyEmail;
        Subject = subject;
        SentDate = sentDate;
    }
}
