
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Syndicates;

public interface ISyndicateRequirementAppService : IApplicationService
{
    Task<PagedResultDto<SyndicateRequirementDto>> GetListAsync(SyndicateRequirementFilterDto input);
}
