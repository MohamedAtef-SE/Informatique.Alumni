using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Gallery;

public class GalleryCleanupJob : AsyncBackgroundJob<GalleryCleanupArgs>, ITransientDependency
{
    private readonly IBlobContainer<GalleryBlobContainer> _blobContainer;

    public GalleryCleanupJob(IBlobContainer<GalleryBlobContainer> blobContainer)
    {
        _blobContainer = blobContainer;
    }

    public override async Task ExecuteAsync(GalleryCleanupArgs args)
    {
        foreach (var blobName in args.BlobNames)
        {
            await _blobContainer.DeleteAsync(blobName);
        }
    }
}
