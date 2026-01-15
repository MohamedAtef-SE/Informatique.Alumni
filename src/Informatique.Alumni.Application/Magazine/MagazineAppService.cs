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
    private readonly IRepository<MagazineIssue, Guid> _issueRepository;
    private readonly IRepository<Magazine, Guid> _magazineRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IBlobContainer<GalleryBlobContainer> _galleryBlobContainer;
    private readonly IBlobContainer<MagazineBlobContainer> _magazineBlobContainer;
    private readonly MembershipManager _membershipManager;
    private readonly AlumniApplicationMappers _alumniMappers;

    public MagazineAppService(
        IRepository<MagazineIssue, Guid> issueRepository,
        IRepository<Magazine, Guid> magazineRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IBlobContainer<GalleryBlobContainer> galleryBlobContainer,
        IBlobContainer<MagazineBlobContainer> magazineBlobContainer,
        MembershipManager membershipManager,
        AlumniApplicationMappers alumniMappers)
    {
        _issueRepository = issueRepository;
        _magazineRepository = magazineRepository;
        _profileRepository = profileRepository;
        _galleryBlobContainer = galleryBlobContainer;
        _magazineBlobContainer = magazineBlobContainer;
        _membershipManager = membershipManager;
        _alumniMappers = alumniMappers;
    }

    // Existing MagazineIssue Methods (Preserved)
    [Authorize(AlumniPermissions.Magazine.ManageIssues)]
    public async Task<MagazineIssueDto> CreateIssueAsync(CreateUpdateMagazineIssueDto input)
    {
        var pdfBlobName = $"mag_{Guid.NewGuid()}.pdf";
        await _galleryBlobContainer.SaveAsync(pdfBlobName, input.PdfBytes);

        var issue = new MagazineIssue(
            GuidGenerator.Create(),
            input.Title,
            input.PublishDate,
            pdfBlobName,
            "placeholder_thumb.jpg"
        ) { Description = input.Description };

        await _issueRepository.InsertAsync(issue);
        return _alumniMappers.MapToDto(issue);
    }

    public async Task<PagedResultDto<MagazineIssueDto>> GetIssuesAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _issueRepository.GetCountAsync();
        var entities = await _issueRepository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, input.Sorting);
        return new PagedResultDto<MagazineIssueDto>(count, _alumniMappers.MapToDtos(entities));
    }

    [Authorize(AlumniPermissions.Magazine.ManageIssues)]
    public async Task DeleteIssueAsync(Guid id)
    {
        var issue = await _issueRepository.GetAsync(id);
        await _galleryBlobContainer.DeleteAsync(issue.PdfBlobName);
        await _issueRepository.DeleteAsync(issue);
    }

    // =========================================================
    // NEW: Graduate Portal Implementation (Phase 19)
    // =========================================================

    public async Task<PagedResultDto<MagazineListDto>> GetListAsync(GetMagazinesInput input)
    {
        // 1. Gate: Check Membership
        await CheckMembershipActiveAsync();

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
            DownloadUrl = $"/api/app/magazine/{x.Id}/download" // Convention
        }).ToList();

        return new PagedResultDto<MagazineListDto>(totalCount, dtos);
    }

    public async Task<System.IO.Stream> DownloadAsync(Guid id)
    {
        // 1. Gate: Check Membership
        await CheckMembershipActiveAsync();

        var magazine = await _magazineRepository.GetAsync(id);
        
        return await _magazineBlobContainer.GetAsync(magazine.FileBlobName);
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

        // Resolve Profile
        var queryable = await _profileRepository.GetQueryableAsync();
        var profile = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.UserId == currentUserId.Value)
        );

        if (profile == null)
        {
             // If no profile, can't be member
             throw new UserFriendlyException("Active membership required to view the magazine.");
        }

        // Check Membership Status
        var isActive = await _membershipManager.IsActiveAsync(profile.Id);
        if (!isActive)
        {
            throw new UserFriendlyException("Active membership required to view the magazine.");
        }
    }
}
