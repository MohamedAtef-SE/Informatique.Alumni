using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Informatique.Alumni.BlobContainers;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Health;

/// <summary>
/// Application service for healthcare offers (graduate portal).
/// Business Rule: STRICT membership gating - only active members can access.
/// Clean Code: Guard clause pattern, small focused methods.
/// </summary>
[Authorize]
public class HealthcareOfferAppService : ApplicationService, IHealthcareOfferAppService
{
    private readonly IRepository<HealthcareOffer, Guid> _offerRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IBlobContainer<HealthcareBlobContainer> _blobContainer;
    private readonly MembershipManager _membershipManager;

    public HealthcareOfferAppService(
        IRepository<HealthcareOffer, Guid> offerRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IBlobContainer<HealthcareBlobContainer> blobContainer,
        MembershipManager membershipManager)
    {
        _offerRepository = offerRepository;
        _profileRepository = profileRepository;
        _blobContainer = blobContainer;
        _membershipManager = membershipManager;
    }

    /// <summary>
    /// Gets the list of healthcare offers for graduates with pagination.
    /// Business Rule: GUARD CLAUSE - Check membership BEFORE fetching any data.
    /// Clean Code: Fast-fail pattern, no nested conditionals.
    /// </summary>
    public async Task<PagedResultDto<HealthcareOfferListDto>> GetListAsync(GetHealthcareOffersInput input)
    {
        // GUARD CLAUSE: Business Rule - Active membership required
        await CheckMembershipActiveAsync();

        // Fetch offers with filtering (only executed if membership is active)
        var queryable = await _offerRepository.GetQueryableAsync();

        // Apply filter
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(o => o.Title.Contains(input.Filter));
        }

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting and paging
        var offers = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(o => o.UploadDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // Map to DTOs with download URLs
        var dtos = offers.Select(offer => new HealthcareOfferListDto
        {
            Id = offer.Id,
            Title = offer.Title,
            DownloadUrl = GenerateDownloadUrl(offer.Id),
            UploadDate = offer.UploadDate
        }).ToList();

        return new PagedResultDto<HealthcareOfferListDto>(totalCount, dtos);
    }

    /// <summary>
    /// Downloads a healthcare offer file.
    /// Business Rule: GUARD CLAUSE - Active membership required.
    /// </summary>
    public async Task<Stream> DownloadAsync(Guid id)
    {
        // GUARD CLAUSE: Business Rule - Active membership required
        await CheckMembershipActiveAsync();

        // Get offer
        var offer = await _offerRepository.GetAsync(id);

        // Return file stream from blob storage
        return await _blobContainer.GetAsync(offer.FileBlobName);
    }

    // ============ GUARD CLAUSE METHOD ============

    /// <summary>
    /// Checks if the current user has an active membership.
    /// Business Rule: Access to healthcare offers is RESTRICTED to active members only.
    /// Error Message: Exact text specified in requirements.
    /// Clean Code: Single responsibility - membership validation only.
    /// </summary>
    private async Task CheckMembershipActiveAsync()
    {
        var currentUserId = CurrentUser.Id;

        if (currentUserId == null)
        {
             throw new UserFriendlyException("User not authenticated");
        }

        // Query alumni profile with membership status
        var queryable = await _profileRepository.GetQueryableAsync();
        var profile = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(p => p.UserId == currentUserId.Value)
        );

        if (profile == null)
        {
            throw new UserFriendlyException("Alumni profile not found");
        }

        // Business Rule: Check membership status via MembershipManager
        var isActive = await _membershipManager.IsActiveAsync(profile.Id);

        if (!isActive)
        {
            throw new UserFriendlyException(
                "To benefit from these offers, membership must be active"
            );
        }
    }

    /// <summary>
    /// Generates download URL for a healthcare offer.
    /// Returns API endpoint URL for file download.
    /// Clean Code: Centralized URL generation.
    /// </summary>
    private string GenerateDownloadUrl(Guid offerId)
    {
        // Return API endpoint URL for download
        // Frontend will call this endpoint to download the file
        return $"/api/app/healthcare-offer/download/{offerId}";
    }
}
