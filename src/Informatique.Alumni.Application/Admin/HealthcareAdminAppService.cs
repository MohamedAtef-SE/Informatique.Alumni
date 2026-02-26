using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Health;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.HealthcareManage)]
public class HealthcareAdminAppService : AlumniAppService, IHealthcareAdminAppService
{
    private readonly IRepository<MedicalPartner, Guid> _partnerRepository;

    public HealthcareAdminAppService(IRepository<MedicalPartner, Guid> partnerRepository)
    {
        _partnerRepository = partnerRepository;
    }

    public async Task<PagedResultDto<MedicalPartnerAdminDto>> GetPartnersAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _partnerRepository.WithDetailsAsync();

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var items = queryable.ToList().Select(p => new MedicalPartnerAdminDto
        {
            Id = p.Id,
            Name = p.Name,
            Type = p.Type,
            Address = p.Address,
            ContactNumber = p.ContactNumber,
            IsActive = p.IsActive,
            OfferCount = p.Offers.Count,
            CreationTime = p.CreationTime
        }).ToList();

        return new PagedResultDto<MedicalPartnerAdminDto>(totalCount, items);
    }

    public async Task TogglePartnerActiveAsync(Guid id)
    {
        var partner = await _partnerRepository.GetAsync(id);
        partner.IsActive = !partner.IsActive;
        await _partnerRepository.UpdateAsync(partner);
    }
}
