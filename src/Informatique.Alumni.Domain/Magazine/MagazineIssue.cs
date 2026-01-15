using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Magazine;

public class MagazineIssue : FullAuditedEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PublishDate { get; set; }
    public string PdfBlobName { get; set; } = string.Empty;
    public string ThumbnailBlobName { get; set; } = string.Empty;

    private MagazineIssue() { }

    public MagazineIssue(Guid id, string title, DateTime publishDate, string pdfBlobName, string thumbnailBlobName)
        : base(id)
    {
        Title = title;
        PublishDate = publishDate;
        PdfBlobName = pdfBlobName;
        ThumbnailBlobName = thumbnailBlobName;
    }
}
