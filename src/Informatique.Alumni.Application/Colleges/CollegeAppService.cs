using System;
using Informatique.Alumni.Profiles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Colleges;

public class CollegeAppService : CrudAppService<
    College,
    CollegeDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateCollegeDto>,
    ICollegeAppService
{
    private readonly AlumniApplicationMappers _alumniMappers;

    public CollegeAppService(IRepository<College, Guid> repository, AlumniApplicationMappers alumniMappers)
        : base(repository)
    {
        _alumniMappers = alumniMappers;
    }

    protected override System.Threading.Tasks.Task<CollegeDto> MapToGetOutputDtoAsync(College entity)
    {
        return System.Threading.Tasks.Task.FromResult(_alumniMappers.MapToDto(entity));
    }

    protected override System.Threading.Tasks.Task<System.Collections.Generic.List<CollegeDto>> MapToGetListOutputDtosAsync(System.Collections.Generic.List<College> entities)
    {
        return System.Threading.Tasks.Task.FromResult(_alumniMappers.MapToDtos(entities));
    }

    protected override System.Threading.Tasks.Task<College> MapToEntityAsync(CreateUpdateCollegeDto createInput)
    {
        var entity = new College(GuidGenerator.Create(), createInput.Name, createInput.BranchId, createInput.ExternalId);
        return System.Threading.Tasks.Task.FromResult(entity);
    }

    protected override System.Threading.Tasks.Task MapToEntityAsync(CreateUpdateCollegeDto updateInput, College entity)
    {
        entity.Update(updateInput.Name, updateInput.BranchId, updateInput.ExternalId);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
