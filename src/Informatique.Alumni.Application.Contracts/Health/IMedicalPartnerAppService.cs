using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Health;

public interface IMedicalPartnerAppService : IApplicationService
{
    Task<MedicalPartnerDto> CreateAsync(CreateUpdateMedicalPartnerDto input);
    Task<MedicalPartnerDto> UpdateAsync(Guid id, CreateUpdateMedicalPartnerDto input);
    Task DeleteAsync(Guid id);
    Task<MedicalPartnerDto> GetAsync(Guid id);
    Task<List<MedicalPartnerDto>> GetListAsync(MedicalPartnerType? type);
    
    Task<MedicalPartnerDto> AddOfferAsync(Guid partnerId, CreateUpdateMedicalOfferDto input);
    Task<MedicalPartnerDto> RemoveOfferAsync(Guid partnerId, Guid offerId);
}
