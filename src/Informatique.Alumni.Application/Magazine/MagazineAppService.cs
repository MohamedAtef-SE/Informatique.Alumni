using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Gallery;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.BlobContainers;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Magazine;

[Authorize]
public class MagazineAppService : AlumniAppService, IMagazineAppService
{
    private readonly IRepository<Magazine, Guid> _magazineRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IBlobContainer<MagazineBlobContainer> _magazineBlobContainer;
    private readonly MagazineManager _magazineManager;
    private readonly MembershipManager _membershipManager;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly MembershipGuard _membershipGuard;

    public MagazineAppService(
        IRepository<Magazine, Guid> magazineRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IBlobContainer<MagazineBlobContainer> magazineBlobContainer,
        MagazineManager magazineManager,
        MembershipManager membershipManager,
        AlumniApplicationMappers alumniMappers,
        MembershipGuard membershipGuard)
    {
        _magazineRepository = magazineRepository;
        _profileRepository = profileRepository;
        _magazineBlobContainer = magazineBlobContainer;
        _magazineManager = magazineManager;
        _membershipManager = membershipManager;
        _alumniMappers = alumniMappers;
        _membershipGuard = membershipGuard;
    }

    [Authorize(AlumniPermissions.Magazine.ManageIssues)]
    public async Task<MagazineIssueDto> CreateIssueAsync(CreateUpdateMagazineIssueDto input)
    {
        using var stream = new System.IO.MemoryStream(input.PdfBytes);
        var magazine = await _magazineManager.CreateAsync(
            input.Title,
            input.PublishDate,
            stream,
            input.PdfFileName // Assuming input has a filename, otherwise "file.pdf"
        );
        
        await _magazineRepository.InsertAsync(magazine);

        // Map manually or use object mapper if possible, but fields differ slightly
        return new MagazineIssueDto
        {
            Id = magazine.Id,
            Title = magazine.Title,
            PublishDate = magazine.IssueDate,
            PdfUrl = $"/api/app/magazine/{magazine.Id}/download",
            CreationTime = magazine.CreationTime,
            Description = null, // Not supported in Magazine entity
            ThumbnailUrl = "" // Not supported in Magazine entity
        };
    }

    public async Task<PagedResultDto<MagazineIssueDto>> GetIssuesAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _magazineRepository.GetQueryableAsync();
        
        var count = await AsyncExecuter.CountAsync(queryable);
        
        var entities = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.IssueDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = entities.Select(x => new MagazineIssueDto
        {
            Id = x.Id,
            Title = x.Title,
            PublishDate = x.IssueDate,
            PdfUrl = $"/api/app/magazine/{x.Id}/download",
            CreationTime = x.CreationTime,
            Description = null,
            ThumbnailUrl = ""
        }).ToList();

        return new PagedResultDto<MagazineIssueDto>(count, dtos);
    }

    [Authorize(AlumniPermissions.Magazine.ManageIssues)]
    public async Task DeleteIssueAsync(Guid id)
    {
        var magazine = await _magazineRepository.GetAsync(id);
        if (!string.IsNullOrEmpty(magazine.FileBlobName))
        {
            await _magazineBlobContainer.DeleteAsync(magazine.FileBlobName);
        }
        await _magazineRepository.DeleteAsync(magazine);
    }

    public async Task<PagedResultDto<MagazineListDto>> GetListAsync(GetMagazinesInput input)
    {
        // 1. Gate: Check Membership
        // Removed to allow viewing list
        // await _membershipGuard.CheckAsync();

        // 2. Query
        var queryable = await _magazineRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Title.Contains(input.Filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var entities = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.IssueDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // 3. Map
        var dtos = entities.Select(x => new MagazineListDto
        {
            Id = x.Id,
            Title = x.Title,
            IssueDate = x.IssueDate,
            CreationTime = x.CreationTime,
            DownloadUrl = $"/api/app/magazine/{x.Id}/download"
        }).ToList();

        return new PagedResultDto<MagazineListDto>(totalCount, dtos);
    }

    public async Task<Volo.Abp.Content.IRemoteStreamContent> DownloadAsync(Guid id)
    {
        // 1. Gate: Check Membership
        await _membershipGuard.CheckAsync();

        var magazine = await _magazineRepository.GetAsync(id);
        var stream = await _magazineBlobContainer.GetAsync(magazine.FileBlobName);
        
        return new Volo.Abp.Content.RemoteStreamContent(stream, magazine.FileBlobName, "application/pdf");
    }
}
