using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Branches;

public interface IBranchAppService : ICrudAppService<BranchDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateBranchDto>
{
    Task<List<BranchDto>> GetAcademicStructureAsync();
}
