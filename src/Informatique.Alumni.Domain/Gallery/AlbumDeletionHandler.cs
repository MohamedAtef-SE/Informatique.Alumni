using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace Informatique.Alumni.Gallery;

public class AlbumDeletionHandler : 
    ILocalEventHandler<EntityDeletedEventData<GalleryAlbum>>,
    ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public AlbumDeletionHandler(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task HandleEventAsync(EntityDeletedEventData<GalleryAlbum> eventData)
    {
        var album = eventData.Entity;
        if (album.MediaItems.Any())
        {
            var blobNames = new List<string>();
            foreach (var item in album.MediaItems)
            {
                blobNames.Add(item.Url);
                // Thumbnail logic (Convention: thumb_{Url}) - Assuming same convention as before
                blobNames.Add($"thumb_{item.Url}");
            }

            await _backgroundJobManager.EnqueueAsync(new GalleryCleanupArgs { BlobNames = blobNames });
        }
    }
}
