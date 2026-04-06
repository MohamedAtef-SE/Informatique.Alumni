using System;
using Informatique.Alumni.Profiles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Majors;

public class MajorAppService : CrudAppService<
    Major,
    MajorDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateMajorDto>,
    IMajorAppService
{
    private readonly AlumniApplicationMappers _alumniMappers;

    public MajorAppService(IRepository<Major, Guid> repository, AlumniApplicationMappers alumniMappers)
        : base(repository)
    {
        _alumniMappers = alumniMappers;
    }

    protected override System.Threading.Tasks.Task<MajorDto> MapToGetOutputDtoAsync(Major entity)
    {
        return System.Threading.Tasks.Task.FromResult(_alumniMappers.MapToDto(entity));
    }

    protected override System.Threading.Tasks.Task<System.Collections.Generic.List<MajorDto>> MapToGetListOutputDtosAsync(System.Collections.Generic.List<Major> entities)
    {
        return System.Threading.Tasks.Task.FromResult(_alumniMappers.MapToDtos(entities));
    }

    protected override System.Threading.Tasks.Task<Major> MapToEntityAsync(CreateUpdateMajorDto createInput)
    {
        var entity = new Major(GuidGenerator.Create(), createInput.CollegeId, createInput.Name);
        return System.Threading.Tasks.Task.FromResult(entity);
    }

    protected override System.Threading.Tasks.Task MapToEntityAsync(CreateUpdateMajorDto updateInput, Major entity)
    {
        entity.Update(updateInput.Name, updateInput.CollegeId);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
