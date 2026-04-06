using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace Informatique.Alumni.Career;

[Authorize]
public class CvAppService : AlumniAppService, ICVAppService
{
    private readonly IRepository<CurriculumVitae, Guid> _cvRepository;
    private readonly CvManager _cvManager;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly ICVPdfGenerator _pdfGenerator;

    public CvAppService(
        IRepository<CurriculumVitae, Guid> cvRepository,
        CvManager cvManager,
        AlumniApplicationMappers alumniMappers,
        ICVPdfGenerator pdfGenerator)
    {
        _cvRepository = cvRepository;
        _cvManager = cvManager;
        _alumniMappers = alumniMappers;
        _pdfGenerator = pdfGenerator;
    }

    [Authorize(AlumniPermissions.Careers.CvManage)]
    public async Task<CurriculumVitaeDto> GetMyCvAsync()
    {
        var alumniId = await GetCurrentAlumniProfileIdAsync();
        var cv = await _cvRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId);
        
        if (cv == null)
        {
            cv = new CurriculumVitae(GuidGenerator.Create(), alumniId);
            await _cvRepository.InsertAsync(cv);
        }
        else
        {
            await LoadAllCollectionsAsync(cv);
        }

        return _alumniMappers.MapToDto(cv);
    }

    [Authorize(AlumniPermissions.Careers.CvManage)]
    public async Task<CurriculumVitaeDto> UpdateMyCvAsync(CurriculumVitaeDto input)
    {
        var alumniId = await GetCurrentAlumniProfileIdAsync();
        var cv = await _cvRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId);
        if (cv == null) throw new UserFriendlyException("CV not found.");

        await LoadAllCollectionsAsync(cv);
        
        // --- 1. Basic Info ---
        cv.Summary = input.Summary;
        cv.IsLookingForJob = input.IsLookingForJob;
        
        // --- 2. Collection Synchronization (Production-Ready Smart Sync) ---
        SyncCollection(cv.Experiences, input.Experiences, (entity, dto) => {
            entity.Company = dto.Company;
            entity.Position = dto.Position;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Description = dto.Description;
        }, cv.Id);

        SyncCollection(cv.Educations, input.Educations, (entity, dto) => {
            entity.Institution = dto.Institution;
            entity.Degree = dto.Degree;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
        }, cv.Id);

        SyncCollection(cv.Skills, input.Skills, (entity, dto) => {
            entity.Name = dto.Name;
            entity.ProficiencyLevel = dto.ProficiencyLevel;
        }, cv.Id);

        SyncCollection(cv.Languages, input.Languages, (entity, dto) => {
            entity.Name = dto.Name;
            entity.FluencyLevel = dto.FluencyLevel;
        }, cv.Id);

        SyncCollection(cv.Certifications, input.Certifications, (entity, dto) => {
            entity.Name = dto.Name;
            entity.Issuer = dto.Issuer;
            entity.Date = dto.Date;
        }, cv.Id);

        SyncCollection(cv.Projects, input.Projects, (entity, dto) => {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Link = dto.Link;
        }, cv.Id);

        SyncCollection(cv.SocialLinks, input.SocialLinks, (entity, dto) => {
            entity.Platform = dto.Platform;
            entity.Url = dto.Url;
        }, cv.Id);

        // --- Handle other collections as needed later (Audit-readiness) ---

        await _cvRepository.UpdateAsync(cv);
        return _alumniMappers.MapToDto(cv);
    }

    /// <summary>
    /// Production-Ready Smart Sync: Identifies changes, additions, and deletions within a collection.
    /// Preserves existing IDs to maintain database integrity and audit logs.
    /// </summary>
    private void SyncCollection<TEntity, TDto>(
        ICollection<TEntity> entityCollection,
        IEnumerable<TDto> dtoCollection,
        Action<TEntity, TDto> mapAction,
        Guid parentId) 
        where TEntity : class, IEntity<Guid>, new()
        where TDto : class, IEntityDto<Guid>
    {
        if (dtoCollection == null) return;

        var dtos = dtoCollection.ToList();
        var entities = entityCollection.ToList();

        // 1. Remove Orphans (Items present in DB but not in the input)
        foreach (var entity in entities)
        {
            if (dtos.All(d => d.Id != entity.Id))
            {
                entityCollection.Remove(entity);
            }
        }

        // 2. Update Existing & Add New
        foreach (var dto in dtos)
        {
            var existingEntity = entities.FirstOrDefault(e => e.Id != Guid.Empty && e.Id == dto.Id);

            if (existingEntity != null)
            {
                // Update
                mapAction(existingEntity, dto);
            }
            else
            {
                // Add New
                var newEntity = new TEntity();
                EntityHelper.TrySetId(newEntity, () => GuidGenerator.Create());
                
                // Set the backlink using reflection or a dynamic check if needed, 
                // but since all these entities have 'CurriculumVitaeId' property:
                var prop = typeof(TEntity).GetProperty("CurriculumVitaeId");
                prop?.SetValue(newEntity, parentId);

                mapAction(newEntity, dto);
                entityCollection.Add(newEntity);
            }
        }
    }

    public async Task<byte[]> DownloadCvPdfAsync(Guid cvId)
    {
        var cv = await _cvRepository.GetAsync(cvId);
        await LoadAllCollectionsAsync(cv);
        
        // Privacy check
        if (!cv.IsLookingForJob && cv.AlumniId != await GetCurrentAlumniProfileIdAsync())
        {
             throw new UnauthorizedAccessException("This CV is private.");
        }

        var dto = _alumniMappers.MapToDto(cv);
        return await _pdfGenerator.GeneratePdfAsync(dto);
    }

    [Authorize(AlumniPermissions.Careers.CvManage)]
    public async Task<bool> HasCvAsync()
    {
        var alumniId = await GetCurrentAlumniProfileIdAsync();
        return await _cvRepository.AnyAsync(x => x.AlumniId == alumniId);
    }

    private async Task LoadAllCollectionsAsync(CurriculumVitae cv)
    {
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Educations);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Experiences);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Skills);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Languages);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Certifications);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Projects);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Awards);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.VolunteerWorks);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.References);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Publications);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Interests);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.SocialLinks);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.Courses);
        await _cvRepository.EnsureCollectionLoadedAsync(cv, x => x.PracticalTrainings);
    }
}
