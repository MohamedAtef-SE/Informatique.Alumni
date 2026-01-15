using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Magazine;

public class PostComment : FullAuditedEntity<Guid>
{
    public Guid BlogPostId { get; private set; }
    public Guid AlumniId { get; private set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }

    private PostComment() { }

    internal PostComment(Guid id, Guid blogPostId, Guid alumniId, string content)
        : base(id)
    {
        BlogPostId = blogPostId;
        AlumniId = alumniId;
        Content = content;
        IsApproved = false; // Requires approval by default
    }
}
