using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Magazine;

public class BlogPost : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public Guid AuthorId { get; set; }
    public string? CoverImageBlobName { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }

    public ICollection<PostComment> Comments { get; private set; }

    private BlogPost() 
    {
        Comments = new List<PostComment>();
    }

    public BlogPost(Guid id, string title, string summary, string content, Guid authorId, string? category = null, string? tags = null)
        : base(id)
    {
        Title = title;
        Summary = summary;
        Content = content;
        AuthorId = authorId;
        Category = category;
        Tags = tags;
        Comments = new List<PostComment>();
    }

    public void AddComment(Guid id, Guid alumniId, string content)
    {
        Comments.Add(new PostComment(id, Id, alumniId, content));
    }
}
