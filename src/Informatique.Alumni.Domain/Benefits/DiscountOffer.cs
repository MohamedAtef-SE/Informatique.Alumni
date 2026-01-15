
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Benefits;

public class DiscountOffer : FullAuditedAggregateRoot<Guid>
{
    public Guid DiscountCategoryId { get; private set; }
    public string Title { get; private set; }
    public string FilePath { get; private set; }
    public DateTime UploadDate { get; private set; }

    private DiscountOffer()
    {
        Title = string.Empty;
        FilePath = string.Empty;
    }

    public DiscountOffer(Guid id, Guid discountCategoryId, string title, string filePath)
        : base(id)
    {
        DiscountCategoryId = discountCategoryId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        FilePath = Check.NotNullOrWhiteSpace(filePath, nameof(filePath));
        UploadDate = DateTime.Now;
    }

    public void UpdateInfo(string title, string filePath)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            FilePath = filePath;
            UploadDate = DateTime.Now; // Update date on file change? Usually yes.
            // Requirement says "UploadDate: Auto-generated (System Clock)".
            // Ill update it if file changes.
        }
    }
}
