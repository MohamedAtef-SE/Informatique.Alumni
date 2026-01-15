using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
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
    public async Task<CurriculumVitaeDto> UpdateCvAsync(CurriculumVitaeDto input)
    {
        var alumniId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        var cv = await _cvRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId);
        if (cv == null) throw new UserFriendlyException("CV not found.");

        await LoadAllCollectionsAsync(cv);
        
        // Manual mapping or clear and re-add for children
        _alumniMappers.MapToEntity(input, cv);
        
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
