using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Gallery;

public class GalleryAlbumDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<GalleryImageDto> Images { get; set; } = new();
}

public class CreateGalleryAlbumDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class GalleryImageDto : CreationAuditedEntityDto<Guid>
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class UploadGalleryImagesDto
{
    public Guid AlbumId { get; set; }
    public List<byte[]> ImageBytes { get; set; } = new();
    public List<string> FileNames { get; set; } = new();
}

// Graduate Portal DTOs
public class GalleryAlbumListDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
}

public class GalleryAlbumDetailDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MediaItemDto> MediaItems { get; set; } = new();
}

public class MediaItemDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "Photo"; // Default to Photo
}

public class GalleryFilterDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; } // Album Title (Partial)
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
