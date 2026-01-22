using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Benefits;

public class DiscountOffer : FullAuditedAggregateRoot<Guid>
{
    public Guid DiscountCategoryId { get; private set; }
    public string Title { get; private set; }
    public string FileUrl { get; private set; }
    public DateTime UploadDate { get; private set; }

    private DiscountOffer() { }

    public DiscountOffer(Guid id, Guid discountCategoryId, string title, string fileUrl)
        : base(id)
    {
        DiscountCategoryId = discountCategoryId;
        SetTitle(title);
        SetFileUrl(fileUrl);
        UploadDate = DateTime.Now;
    }

    public void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
    }

    public void SetFileUrl(string fileUrl)
    {
        FileUrl = Check.NotNullOrWhiteSpace(fileUrl, nameof(fileUrl));
        UploadDate = DateTime.Now;
    }
}
