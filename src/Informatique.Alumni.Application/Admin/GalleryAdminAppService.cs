using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Gallery;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.GalleryManage)]
public class GalleryAdminAppService : AlumniAppService, IGalleryAdminAppService
{
    private readonly IRepository<GalleryAlbum, Guid> _albumRepository;

    public GalleryAdminAppService(IRepository<GalleryAlbum, Guid> albumRepository)
    {
        _albumRepository = albumRepository;
    }

    public async Task<PagedResultDto<GalleryAlbumAdminDto>> GetAlbumsAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _albumRepository.WithDetailsAsync();

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.Date)
            .PageBy(input);

        var items = queryable.ToList().Select(a => new GalleryAlbumAdminDto
        {
            Id = a.Id,
            Name = a.Name,
            Date = a.Date,
            Description = a.Description,
            MediaItemCount = a.MediaItems.Count,
            CreationTime = a.CreationTime
        }).ToList();

        return new PagedResultDto<GalleryAlbumAdminDto>(totalCount, items);
    }

    public async Task DeleteAlbumAsync(Guid id)
    {
        await _albumRepository.DeleteAsync(id);
    }

    public async Task DeleteMediaItemAsync(Guid albumId, Guid mediaItemId)
    {
        var queryable = await _albumRepository.WithDetailsAsync(a => a.MediaItems);
        var album = await AsyncExecuter.FirstOrDefaultAsync(queryable, a => a.Id == albumId);
        if (album == null) return;

        var item = album.MediaItems.FirstOrDefault(m => m.Id == mediaItemId);
        if (item != null)
        {
            album.MediaItems.Remove(item);
            await _albumRepository.UpdateAsync(album);
        }
    }
}
