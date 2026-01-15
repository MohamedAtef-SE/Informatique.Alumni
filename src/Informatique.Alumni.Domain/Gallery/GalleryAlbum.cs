using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Gallery;

public class GalleryAlbum : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public DateTime Date { get; private set; } // Required by new rules
    public string? Description { get; private set; }
    
    public ICollection<GalleryMediaItem> MediaItems { get; private set; }

    private GalleryAlbum() 
    {
        MediaItems = new List<GalleryMediaItem>();
    }

    internal GalleryAlbum(Guid id, string name, DateTime date, string? description = null)
        : base(id)
    {
        SetName(name);
        Date = date;
        Description = description;
        MediaItems = new List<GalleryMediaItem>();
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
    }

    public void SetDate(DateTime date)
    {
        Date = date;
    }

    public void AddMediaItem(Guid id, string url, GalleryMediaType mediaType, string fileName)
    {
        MediaItems.Add(new GalleryMediaItem(id, Id, url, mediaType, fileName));
    }
}
