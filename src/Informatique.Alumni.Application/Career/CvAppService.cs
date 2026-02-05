using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
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
        var alumniId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
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
        var alumniId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        var cv = await _cvRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId);
        if (cv == null) throw new UserFriendlyException("CV not found.");

        await LoadAllCollectionsAsync(cv);
        
        // Manual mapping to avoid Mapperly recursion/instantiation issues causing 500 errors
        cv.Summary = input.Summary;
        cv.IsLookingForJob = input.IsLookingForJob;
        
        // Handle Experiences (Full Replacement Strategy)
        cv.Experiences.Clear();
        if (input.Experiences != null)
        {
            foreach (var expDto in input.Experiences)
            {
               var experience = new CvExperience
               {
                   Company = expDto.Company,
                   Position = expDto.Position,
                   StartDate = expDto.StartDate,
                   EndDate = expDto.EndDate,
                   Description = expDto.Description,
                   CurriculumVitaeId = cv.Id
               };
               EntityHelper.TrySetId(experience, () => GuidGenerator.Create());
               cv.Experiences.Add(experience);
            }
        }

        // Handle Educations (Full Replacement Strategy)
        cv.Educations.Clear();
        if (input.Educations != null)
        {
            foreach (var eduDto in input.Educations)
            {
               var education = new CvEducation
               {
                   Institution = eduDto.Institution,
                   Degree = eduDto.Degree,
                   StartDate = eduDto.StartDate,
                   EndDate = eduDto.EndDate,
                   CurriculumVitaeId = cv.Id
               };
               EntityHelper.TrySetId(education, () => GuidGenerator.Create());
               cv.Educations.Add(education);
            }
        }

        // _alumniMappers.MapToEntity(input, cv); // DISABLED causing 500 error
        
        await _cvRepository.UpdateAsync(cv);
        return _alumniMappers.MapToDto(cv);
    }

    public async Task<byte[]> DownloadCvPdfAsync(Guid cvId)
    {
        var cv = await _cvRepository.GetAsync(cvId);
        await LoadAllCollectionsAsync(cv);
        
        // Privacy check
        if (!cv.IsLookingForJob && cv.AlumniId != CurrentUser.Id)
        {
             throw new UnauthorizedAccessException("This CV is private.");
        }

        var dto = _alumniMappers.MapToDto(cv);
        return await _pdfGenerator.GeneratePdfAsync(dto);
    }

    [Authorize(AlumniPermissions.Careers.CvManage)]
    public async Task<bool> HasCvAsync()
    {
        var alumniId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
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
