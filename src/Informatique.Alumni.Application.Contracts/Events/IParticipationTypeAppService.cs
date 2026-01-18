using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Events;

public interface IParticipationTypeAppService : IApplicationService
{
    Task<ParticipationTypeDto> CreateAsync(CreateParticipationTypeDto input);
    Task<ParticipationTypeDto> UpdateAsync(Guid id, UpdateParticipationTypeDto input);
    Task DeleteAsync(Guid id);
    Task<ParticipationTypeDto> GetAsync(Guid id);
    Task<PagedResultDto<ParticipationTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input);
}
