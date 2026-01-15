using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Magazine;

public class BlogPost : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid AuthorId { get; set; }
    public string? CoverImageBlobName { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<PostComment> Comments { get; private set; }

    private BlogPost() 
    {
        Comments = new List<PostComment>();
    }

    public BlogPost(Guid id, string title, string content, Guid authorId, string? category = null)
        : base(id)
    {
        Title = title;
        Content = content;
        AuthorId = authorId;
        Category = category;
        Comments = new List<PostComment>();
    }

    public void AddComment(Guid id, Guid alumniId, string content)
    {
        Comments.Add(new PostComment(id, Id, alumniId, content));
    }
}
