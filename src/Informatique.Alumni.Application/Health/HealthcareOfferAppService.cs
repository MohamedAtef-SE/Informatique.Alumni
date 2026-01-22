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

[Authorize]
public class HealthcareOfferAppService : ApplicationService, IHealthcareOfferAppService
{
    private readonly IRepository<HealthcareOffer, Guid> _offerRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IBlobContainer<HealthcareBlobContainer> _blobContainer;
    private readonly MembershipManager _membershipManager;
    private readonly MembershipGuard _membershipGuard;

    public HealthcareOfferAppService(
        IRepository<HealthcareOffer, Guid> offerRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IBlobContainer<HealthcareBlobContainer> blobContainer,
        MembershipManager membershipManager,
        MembershipGuard membershipGuard)
    {
        _offerRepository = offerRepository;
        _profileRepository = profileRepository;
        _blobContainer = blobContainer;
        _membershipManager = membershipManager;
        _membershipGuard = membershipGuard;
    }

    public async Task<PagedResultDto<HealthcareOfferListDto>> GetListAsync(GetHealthcareOffersInput input)
    {
        // GUARD CLAUSE: Business Rule - Active membership required
        await _membershipGuard.CheckAsync();

        var queryable = await _offerRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(o => o.Title.Contains(input.Filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var offers = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(o => o.UploadDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = offers.Select(offer => new HealthcareOfferListDto
        {
            Id = offer.Id,
            Title = offer.Title,
            DownloadUrl = GenerateDownloadUrl(offer.Id),
            UploadDate = offer.UploadDate
        }).ToList();

        return new PagedResultDto<HealthcareOfferListDto>(totalCount, dtos);
    }

    public async Task<Stream> DownloadAsync(Guid id)
    {
        // GUARD CLAUSE: Business Rule - Active membership required
        await _membershipGuard.CheckAsync();

        var offer = await _offerRepository.GetAsync(id);

        return await _blobContainer.GetAsync(offer.FileBlobName);
    }

    private string GenerateDownloadUrl(Guid offerId)
    {
        return $"/api/app/healthcare-offer/download/{offerId}";
    }
}
