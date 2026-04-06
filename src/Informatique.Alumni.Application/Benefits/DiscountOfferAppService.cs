using System;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Benefits;

[Authorize]
public class DiscountOfferAppService : ApplicationService
{
    private readonly IRepository<DiscountOffer, Guid> _offerRepository;
    private readonly MembershipManager _membershipManager;
    private readonly DiscountManager _discountManager;
    private readonly IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> _profileRepository;

    public DiscountOfferAppService(
        IRepository<DiscountOffer, Guid> offerRepository,
        MembershipManager membershipManager,
        DiscountManager discountManager,
        IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> profileRepository)
    {
        _offerRepository = offerRepository;
        _membershipManager = membershipManager;
        _discountManager = discountManager;
        _profileRepository = profileRepository;
    }

    public async Task<string> GetDownloadUrlAsync(Guid offerId)
    {
        // 1. Get Current Alumni ID
        if (CurrentUser.Id == null)
        {
            throw new UnauthorizedAccessException();
        }

        // Dynamically resolve Profile.Id from UserId to satisfy Domain relational rules
        var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == CurrentUser.Id.Value);
        if (profile == null)
        {
            throw new UserFriendlyException("Alumni profile not found for the current user.");
        }
        var alumniId = profile.Id;

        // 2. Membership Gate
        var isActive = await _membershipManager.IsActiveAsync(alumniId);
        if (!isActive)
        {
            throw new UserFriendlyException("You must have an Active Membership to download discount offers.");
        }

        // 3. Retrieve Offer
        var offer = await _offerRepository.GetAsync(offerId);

        return offer.FileUrl;
    }

    // Admin Methods (Create) would use DiscountManager
    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task CreateAsync(Guid categoryId, string title, string fileUrl)
    {
        var offer = await _discountManager.CreateOfferAsync(categoryId, title, fileUrl);
        await _offerRepository.InsertAsync(offer);
    }
}
