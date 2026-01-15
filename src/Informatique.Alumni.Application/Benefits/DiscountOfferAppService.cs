
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Benefits;

[Authorize]
public class DiscountOfferAppService : AlumniAppService
{
    private readonly IRepository<DiscountOffer, Guid> _offerRepository;
    private readonly IRepository<DiscountCategory, Guid> _categoryRepository;
    private readonly MembershipManager _membershipManager;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;

    public DiscountOfferAppService(
        IRepository<DiscountOffer, Guid> offerRepository,
        IRepository<DiscountCategory, Guid> categoryRepository,
        MembershipManager membershipManager,
        IRepository<AlumniProfile, Guid> profileRepository)
    {
        _offerRepository = offerRepository;
        _categoryRepository = categoryRepository;
        _membershipManager = membershipManager;
        _profileRepository = profileRepository;
    }

    public async Task<List<DiscountOfferListDto>> GetListAsync()
    {
        // 1. Resolve Alumni Profile from Current User
        var alumni = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == CurrentUser.Id);
        if (alumni == null)
        {
             // If no profile, they definitely don't have an active membership.
             throw new UserFriendlyException("To benefit from these offers, membership must be active");
        }

        // 2. Business Rule: Guard - Membership Active Check
        if (!await _membershipManager.IsActiveAsync(alumni.Id))
        {
            throw new UserFriendlyException("To benefit from these offers, membership must be active");
        }

        // 3. Fetch Data
        var offers = await _offerRepository.GetListAsync();
        var categoryIds = offers.Select(x => x.DiscountCategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        var categoryDict = categories.ToDictionary(x => x.Id, x => CurrentCultureName(x));

        // 4. Map to DTO
        return offers.Select(offer => new DiscountOfferListDto
        {
            Id = offer.Id,
            CategoryName = categoryDict.ContainsKey(offer.DiscountCategoryId) ? categoryDict[offer.DiscountCategoryId] : "High",
            Title = offer.Title,
            UploadDate = offer.UploadDate,
            DownloadUrl = offer.FilePath // Assuming FilePath is the URL or relative path handled by frontend
        }).ToList();
    }

    private string CurrentCultureName(DiscountCategory category)
    {
        // Simple helper to pick name based on current culture
        // In a real app we might inject ICurrentCulture or similar, but for now simple check
        var isArabic = System.Globalization.CultureInfo.CurrentCulture.Name.StartsWith("ar");
        return isArabic ? category.NameAr : category.NameEn;
    }
}
