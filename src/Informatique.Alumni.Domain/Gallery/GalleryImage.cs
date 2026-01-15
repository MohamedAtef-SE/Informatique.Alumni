using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Gallery;

public class GalleryImage : CreationAuditedEntity<Guid>
{
    public Guid GalleryAlbumId { get; private set; }
    public string OriginalBlobName { get; private set; } = string.Empty;
    public string ThumbnailBlobName { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;

    private GalleryImage() { }

    internal GalleryImage(Guid id, Guid albumId, string originalBlobName, string thumbnailBlobName, string fileName)
        : base(id)
    {
        GalleryAlbumId = albumId;
        OriginalBlobName = originalBlobName;
        ThumbnailBlobName = thumbnailBlobName;
        FileName = fileName;
    }
}
