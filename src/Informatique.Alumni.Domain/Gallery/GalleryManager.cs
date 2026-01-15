using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Timing;

namespace Informatique.Alumni.Gallery;

public class GalleryManager : DomainService
{
    private readonly IRepository<GalleryAlbum, Guid> _albumRepository;
    private readonly IClock _clock;

    public GalleryManager(
        IRepository<GalleryAlbum, Guid> albumRepository,
        IClock clock)
    {
        _albumRepository = albumRepository;
        _clock = clock;
    }

    public async Task<GalleryAlbum> CreateAsync(
        string title,
        DateTime? date,
        List<(string Url, GalleryMediaType Type, string FileName)> mediaItems)
    {
        Check.NotNullOrWhiteSpace(title, nameof(title));
        
        // 1. Validation Logic
        var albumDate = date ?? _clock.Now;

        // Future Date Guard
        if (albumDate > _clock.Now)
        {
            throw new UserFriendlyException("Cannot create albums with future dates.");
        }

        // Uniqueness Logic (Title + Date Scope)
        var exists = await _albumRepository.AnyAsync(x => x.Name == title && x.Date.Date == albumDate.Date);
        if (exists)
        {
            throw new UserFriendlyException("An album with this title already exists for the selected date.");
        }

        // 2. Create Entity
        var album = new GalleryAlbum(GuidGenerator.Create(), title, albumDate);

        // 3. Add Media Items
        foreach (var item in mediaItems)
        {
            album.AddMediaItem(GuidGenerator.Create(), item.Url, item.Type, item.FileName);
        }

        return album;
    }
}
