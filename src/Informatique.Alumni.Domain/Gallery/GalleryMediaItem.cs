using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Gallery;

public class GalleryMediaItem : CreationAuditedEntity<Guid>
{
    public Guid GalleryAlbumId { get; private set; }
    public string Url { get; private set; } = string.Empty; // BlobName
    public GalleryMediaType MediaType { get; private set; } // Image or Video
    public string FileName { get; private set; } = string.Empty;

    private GalleryMediaItem() { }

    internal GalleryMediaItem(Guid id, Guid albumId, string url, GalleryMediaType mediaType, string fileName)
        : base(id)
    {
        GalleryAlbumId = albumId;
        Url = url;
        MediaType = mediaType;
        FileName = fileName;
    }
}
