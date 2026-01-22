using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Organization;

public interface IAcademicStructureAppService : IApplicationService
{
    Task<List<BranchDto>> GetHierarchyAsync();
}
