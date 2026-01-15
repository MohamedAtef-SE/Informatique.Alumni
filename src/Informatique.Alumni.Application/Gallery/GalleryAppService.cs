using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Users;

// ... existing usings ...

namespace Informatique.Alumni.Gallery;

[Authorize]
[Authorize]
public class GalleryAppService : AlumniAppService, IGalleryAppService
{
    private readonly IRepository<GalleryAlbum, Guid> _albumRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IBlobContainer<GalleryBlobContainer> _blobContainer;
    private readonly MembershipManager _membershipManager;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly GalleryManager _galleryManager;

    public GalleryAppService(
        IRepository<GalleryAlbum, Guid> albumRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IBlobContainer<GalleryBlobContainer> blobContainer,
        MembershipManager membershipManager,
        AlumniApplicationMappers alumniMappers,
        GalleryManager galleryManager)
    {
        _albumRepository = albumRepository;
        _profileRepository = profileRepository;
        _blobContainer = blobContainer;
        _membershipManager = membershipManager;
        _alumniMappers = alumniMappers;
        _galleryManager = galleryManager;
    }

    public async Task<GalleryAlbumDto> CreateAlbumAsync(CreateGalleryAlbumDto input)
    {
        // Using Manager for validation
        // Note: DTO creates with empty media list for now as per previous logic, 
        // or we could extend DTO. Keeping it simple as per prompt.
        var album = await _galleryManager.CreateAsync(input.Name, null, new List<(string, GalleryMediaType, string)>());
        
        // Description helper
        // Use repo/manager to update description if needed, logic omitted for brevity as per instructions
        
        await _albumRepository.InsertAsync(album);
        return _alumniMappers.MapToDto(album);
    }

    public async Task<GalleryAlbumDto> GetAsync(Guid id)
    {
        var album = await _albumRepository.WithDetailsAsync(x => x.MediaItems)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == id));
        
        if (album == null) return null;

        // Custom Mapping since AutoMapper might need config update for MediaItems
        // Assuming MapToDto handles basic mapping, but we might need manual mapping for MediaItems -> GalleryImageDto for backward compat
        var dto = new GalleryAlbumDto 
        { 
            Id = album.Id, 
            Name = album.Name, 
            Description = album.Description,
            CreationTime = album.CreationTime,
            Images = album.MediaItems.Select(m => new GalleryImageDto 
            {
                OriginalUrl = $"/api/app/gallery/image?blobName={m.Url}",
                ThumbnailUrl = m.MediaType == GalleryMediaType.Photo ? $"/api/app/gallery/image?blobName=thumb_{m.Url}" : "",
                FileName = m.FileName
            }).ToList()
        };
        
        return dto;
    }

    public async Task<PagedResultDto<GalleryAlbumDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _albumRepository.GetCountAsync();
        var entities = await _albumRepository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, input.Sorting);
        
        // Minimal mapping for list
        var dtos = entities.Select(e => new GalleryAlbumDto 
        { 
            Id = e.Id, 
            Name = e.Name, 
            Description = e.Description, 
            CreationTime = e.CreationTime 
        }).ToList();
        
        return new PagedResultDto<GalleryAlbumDto>(count, dtos);
    }

    [Authorize(AlumniPermissions.Gallery.Delete)]
    public async Task DeleteAlbumAsync(Guid id)
    {
        var album = await _albumRepository.WithDetailsAsync(x => x.MediaItems)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == id));

        if (album != null)
        {
            foreach (var item in album.MediaItems)
            {
                await _blobContainer.DeleteAsync(item.Url);
                if (item.MediaType == GalleryMediaType.Photo)
                {
                    await _blobContainer.DeleteAsync($"thumb_{item.Url}");
                }
            }
            
            await _albumRepository.DeleteAsync(album);
        }
    }

    [Authorize(AlumniPermissions.Gallery.Upload)]
    public async Task UploadImagesAsync(UploadGalleryImagesDto input)
    {
        var album = await _albumRepository.GetAsync(input.AlbumId);

        for (int i = 0; i < input.ImageBytes.Count; i++)
        {
            var bytes = input.ImageBytes[i];
            var fileName = input.FileNames[i];
            var extension = Path.GetExtension(fileName);
            
            var originalBlobName = $"{Guid.NewGuid()}{extension}";
            var thumbnailBlobName = $"thumb_{originalBlobName}";

            // 1. Save Original
            await _blobContainer.SaveAsync(originalBlobName, bytes);

            // 2. Generate and Save Thumbnail (Only for images)
            // Assuming input is only photos for now based on DTO name "UploadGalleryImages"
            using var image = Image.Load(bytes);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(GalleryConsts.ThumbnailWidth, 0)
            }));

            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            await _blobContainer.SaveAsync(thumbnailBlobName, ms.ToArray());

            // 3. Update DB
            // Use MediaItem
            album.AddMediaItem(GuidGenerator.Create(), originalBlobName, GalleryMediaType.Photo, fileName);
        }

        await _albumRepository.UpdateAsync(album);
    }

    public async Task<byte[]> GetImageAsync(string blobName)
    {
        return await _blobContainer.GetAllBytesAsync(blobName);
    }

    // =========================================================
    // NEW: Graduate Portal Implementation (Phase 16)
    // =========================================================

    public async Task<PagedResultDto<GalleryAlbumListDto>> GetAlbumsAsync(GalleryFilterDto input)
    {
        // 1. Gate: Check Membership
        await CheckMembershipActiveAsync();

        // 2. Query
        var queryable = await _albumRepository.WithDetailsAsync(x => x.MediaItems);
        
        var query = queryable.AsQueryable();

        // Filter: Title (Partial) / Name
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.Name.Contains(input.Filter));
        }

        // Filter: Creation Date
        if (input.MinDate.HasValue)
        {
            query = query.Where(x => x.CreationTime >= input.MinDate.Value);
        }
        if (input.MaxDate.HasValue)
        {
            query = query.Where(x => x.CreationTime <= input.MaxDate.Value);
        }

        // Logic: Sort by Creation Date Descending
        query = query.OrderByDescending(x => x.CreationTime);

        var totalCount = await AsyncExecuter.CountAsync(query);
        
        var entities = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        // 3. Map
        var dtos = entities.Select(x => new GalleryAlbumListDto
        {
            Id = x.Id,
            Title = x.Name, // Mapping Name to Title
            CreationTime = x.CreationTime,
            // Cover Image logic: First Photo's thumbnail
            CoverImageUrl = x.MediaItems.Any(m => m.MediaType == GalleryMediaType.Photo)
                ? $"/api/app/gallery/image?blobName=thumb_{x.MediaItems.First(m => m.MediaType == GalleryMediaType.Photo).Url}" 
                : string.Empty
        }).ToList();

        return new PagedResultDto<GalleryAlbumListDto>(totalCount, dtos);
    }

    public async Task<GalleryAlbumDetailDto> GetAlbumDetailsAsync(Guid id)
    {
        // 1. Gate: Check Membership
        await CheckMembershipActiveAsync();

        var queryable = await _albumRepository.WithDetailsAsync(x => x.MediaItems);
        var album = await AsyncExecuter.FirstOrDefaultAsync(queryable, x => x.Id == id);

        if (album == null)
        {
            throw new UserFriendlyException("Album not found.");
        }

        return new GalleryAlbumDetailDto
        {
            Id = album.Id,
            Title = album.Name,
            Description = album.Description ?? string.Empty,
            MediaItems = album.MediaItems.Select(item => new MediaItemDto
            {
                Url = $"/api/app/gallery/image?blobName={item.Url}",
                Type = item.MediaType.ToString()
            }).ToList()
        };
    }

    /// <summary>
    /// Helper to enforce Active Membership Rule
    /// </summary>
    private async Task CheckMembershipActiveAsync()
    {
        var currentUserId = CurrentUser.Id;
        if (currentUserId == null)
        {
            throw new UserFriendlyException("User not authenticated.");
        }

        var queryable = await _profileRepository.GetQueryableAsync();
        var profile = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.UserId == currentUserId.Value)
        );

        if (profile == null)
        {
             throw new UserFriendlyException("Access denied. Active membership required.");
        }

        var isActive = await _membershipManager.IsActiveAsync(profile.Id);
        if (!isActive)
        {
            throw new UserFriendlyException("Access denied. Active membership required.");
        }
    }
}
