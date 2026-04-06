using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Majors;

public interface IMajorAppService : ICrudAppService<
    MajorDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateMajorDto>
{
}
