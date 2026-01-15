using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Magazine;

public class MagazineIssueDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PublishDate { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}

public class CreateUpdateMagazineIssueDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PublishDate { get; set; }
    public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
    public string PdfFileName { get; set; } = string.Empty;
}

public class BlogPostDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid AuthorId { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
}

public class CreateUpdateBlogPostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsPublished { get; set; }
}

public class PostCommentDto : FullAuditedEntityDto<Guid>
{
    public Guid BlogPostId { get; set; }
    public Guid AlumniId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}

public class CreatePostCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class MagazineListDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class GetMagazinesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
