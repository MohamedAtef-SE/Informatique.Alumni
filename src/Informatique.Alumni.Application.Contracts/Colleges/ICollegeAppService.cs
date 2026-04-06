using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Colleges;

public interface ICollegeAppService : ICrudAppService<
    CollegeDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateCollegeDto>
{
}
