using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Magazine;

public class Magazine : AuditedAggregateRoot<Guid>
{
    public string Title { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public string FileBlobName { get; private set; } = string.Empty;
    public MagazineFileType FileType { get; private set; } // Enum: Pdf, Image

    private Magazine() { }

    internal Magazine(Guid id, string title, DateTime issueDate, string fileBlobName, MagazineFileType fileType)
        : base(id)
    {
        SetTitle(title);
        SetFile(fileBlobName, fileType);
        IssueDate = issueDate;
    }

    public void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), MagazineConsts.MaxTitleLength);
    }

    public void SetFile(string fileBlobName, MagazineFileType fileType)
    {
        FileBlobName = Check.NotNullOrWhiteSpace(fileBlobName, nameof(fileBlobName));
        FileType = fileType;
    }

    public void SetIssueDate(DateTime issueDate)
    {
        IssueDate = issueDate;
    }
}
