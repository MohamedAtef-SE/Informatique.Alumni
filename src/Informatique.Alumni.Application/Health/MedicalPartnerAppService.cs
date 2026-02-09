using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Health;

[Authorize(AlumniPermissions.Health.ViewOffers)]
public class MedicalPartnerAppService : AlumniAppService, IMedicalPartnerAppService
{
    private readonly IRepository<MedicalPartner, Guid> _partnerRepository;
    private readonly IRepository<AssociationRequest, Guid> _membershipRepository;
    private readonly IDistributedCache<List<MedicalPartnerDto>> _cache;
    private readonly AlumniApplicationMappers _alumniMappers;

    public MedicalPartnerAppService(
        IRepository<MedicalPartner, Guid> partnerRepository,
        IRepository<AssociationRequest, Guid> membershipRepository,
        IDistributedCache<List<MedicalPartnerDto>> cache,
        AlumniApplicationMappers alumniMappers)
    {
        _partnerRepository = partnerRepository;
        _membershipRepository = membershipRepository;
        _cache = cache;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task<MedicalPartnerDto> CreateAsync(CreateUpdateMedicalPartnerDto input)
    {
        var partner = new MedicalPartner(GuidGenerator.Create(), input.Name, input.Type, input.Address, input.ContactNumber)
        {
            Description = input.Description,
            Website = input.Website
        };
        partner.SetPremiumDetails(input.LogoUrl, input.City, input.Region, input.Category, input.Email, input.HotlineNumber);
        
        await _partnerRepository.InsertAsync(partner);
        await InvalidateCacheAsync();
        return _alumniMappers.MapToDto(partner);
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task<MedicalPartnerDto> UpdateAsync(Guid id, CreateUpdateMedicalPartnerDto input)
    {
        var partner = await _partnerRepository.GetAsync(id);
        
        // Map basic fields
        partner.Name = input.Name;
        partner.Description = input.Description;
        partner.Type = input.Type;
        partner.Address = input.Address;
        partner.ContactNumber = input.ContactNumber;
        partner.Website = input.Website;
        
        // Map premium fields
        partner.SetPremiumDetails(input.LogoUrl, input.City, input.Region, input.Category, input.Email, input.HotlineNumber);

        await _partnerRepository.UpdateAsync(partner);
        await InvalidateCacheAsync();
        return _alumniMappers.MapToDto(partner);
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _partnerRepository.DeleteAsync(id);
        await InvalidateCacheAsync();
    }

    public async Task<MedicalPartnerDto> GetAsync(Guid id)
    {
        var partner = await _partnerRepository.WithDetailsAsync(x => x.Offers)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == id));
        
        if (partner == null) return null;

        var dto = _alumniMappers.MapToDto(partner);
        await GateDiscountCodesAsync(new List<MedicalPartnerDto> { dto });
        return dto;
    }

    public async Task<List<MedicalPartnerDto>> GetListAsync(GetMedicalPartnersInput input)
    {
        // For now, disabling cache for complex filtering or we need a smarter cache key
        // Simplest approach: Cache the full list, then filter in memory (if list < 1000 items)
        // Or just query DB directly if we expect many items. 
        // Given typically < 100 partners, Memory Filter on Cached All is fine.
        
        var cacheKey = "MedicalPartners_All";
        
        var allDtos = await _cache.GetOrAddAsync(cacheKey, async () => 
        {
            var query = await _partnerRepository.WithDetailsAsync(x => x.Offers);
            var items = await AsyncExecuter.ToListAsync(query);
            return _alumniMappers.MapToDtos(items);
        });

        // Apply Filters in Memory
        var queryable = allDtos.AsQueryable();

        if (input.Type.HasValue)
        {
            queryable = queryable.Where(x => x.Type == input.Type.Value);
        }

        if (!string.IsNullOrEmpty(input.FilterText))
        {
            queryable = queryable.Where(x => x.Name.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase) || 
                                             (x.Description != null && x.Description.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                             (x.Category != null && x.Category.Contains(input.FilterText, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrEmpty(input.Category))
        {
             queryable = queryable.Where(x => x.Category != null && x.Category.Contains(input.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(input.City))
        {
             queryable = queryable.Where(x => x.City != null && x.City.Equals(input.City, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrEmpty(input.Region))
        {
             queryable = queryable.Where(x => x.Region != null && x.Region.Contains(input.Region, StringComparison.OrdinalIgnoreCase));
        }

        if (input.HasActiveOffers == true)
        {
             queryable = queryable.Where(x => x.Offers.Any(o => o.IsActive));
        }
        
        // Sorting would go here

        var paged = queryable.ToList(); // Add Skip/Take if needed later

        await GateDiscountCodesAsync(paged);
        return paged;
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task<MedicalPartnerDto> AddOfferAsync(Guid partnerId, CreateUpdateMedicalOfferDto input)
    {
        var partner = await _partnerRepository.WithDetailsAsync(x => x.Offers)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == partnerId));
            
        partner.AddOffer(GuidGenerator.Create(), input.Title, input.Description, input.DiscountCode);
        await _partnerRepository.UpdateAsync(partner);
        await InvalidateCacheAsync();
        return _alumniMappers.MapToDto(partner);
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task<MedicalPartnerDto> RemoveOfferAsync(Guid partnerId, Guid offerId)
    {
        var partner = await _partnerRepository.WithDetailsAsync(x => x.Offers)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == partnerId));
            
        var offer = partner.Offers.FirstOrDefault(x => x.Id == offerId);
        if (offer != null)
        {
            partner.Offers.Remove(offer);
            await _partnerRepository.UpdateAsync(partner);
            await InvalidateCacheAsync();
        }
        return _alumniMappers.MapToDto(partner);
    }

    private async Task InvalidateCacheAsync()
    {
        // Simple invalidation strategy
        await _cache.RemoveAsync("MedicalPartners_All");
        foreach (var type in Enum.GetNames(typeof(MedicalPartnerType)))
        {
            await _cache.RemoveAsync($"MedicalPartners_{type}");
        }
    }

    private async Task GateDiscountCodesAsync(List<MedicalPartnerDto> partners)
    {
        // Feature Gating logic: Only active members see codes
        var currentUserId = CurrentUser.Id;
        if (!currentUserId.HasValue) 
        {
             HideCodes(partners);
             return;
        }

        // Check Phase 3 Membership Status
        var hasActiveRecord = await _membershipRepository.AnyAsync(x => 
            x.AlumniId == currentUserId.Value && 
            x.Status == MembershipRequestStatus.Approved);

        if (!hasActiveRecord)
        {
            HideCodes(partners);
        }
    }

    private void HideCodes(List<MedicalPartnerDto> partners)
    {
        foreach (var partner in partners)
        {
            foreach (var offer in partner.Offers)
            {
                offer.DiscountCode = "******** (Active Membership Required)";
            }
        }
    }
    public async Task<HealthStatsDto> GetStatsAsync()
    {
        var partnerCount = await _partnerRepository.CountAsync();

        var query = await _partnerRepository.GetQueryableAsync();
        var offerCount = query.SelectMany(x => x.Offers).Count(x => x.IsActive);

        return new HealthStatsDto
        {
            MedicalPartnersCount = (int)partnerCount,
            ActiveOffersCount = offerCount,
            AverageSavings = "20%", 
            VerifiedQuality = "100%"
        };
    }
}
