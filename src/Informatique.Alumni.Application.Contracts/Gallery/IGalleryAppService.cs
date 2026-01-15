using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Gallery;

public interface IGalleryAppService : IApplicationService
{
    Task<GalleryAlbumDto> CreateAlbumAsync(CreateGalleryAlbumDto input);
    Task<GalleryAlbumDto> GetAsync(Guid id);
    Task<PagedResultDto<GalleryAlbumDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task DeleteAlbumAsync(Guid id);
    
    Task UploadImagesAsync(UploadGalleryImagesDto input);
    Task<byte[]> GetImageAsync(string blobName);

    // Graduate Portal Methods
    Task<PagedResultDto<GalleryAlbumListDto>> GetAlbumsAsync(GalleryFilterDto input);
    Task<GalleryAlbumDetailDto> GetAlbumDetailsAsync(Guid id);
}
