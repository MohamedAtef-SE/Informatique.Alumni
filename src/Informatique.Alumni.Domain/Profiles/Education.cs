using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

public class Education : Entity<Guid>
{
    public Guid AlumniProfileId { get; private set; }
    public string InstitutionName { get; private set; } = string.Empty;
    public string Degree { get; private set; } = string.Empty;
    public int GraduationYear { get; private set; }
    
    // Extended properties for Search Filtering
    public int GraduationSemester { get; private set; } // 1=Fall, 2=Spring, 3=Summer
    public Guid? CollegeId { get; private set; }
    public Guid? MajorId { get; private set; }
    public Guid? MinorId { get; private set; }

    private Education() { }

    public Education(Guid id, Guid profileId, string institution, string degree, int year)
        : base(id)
    {
        AlumniProfileId = profileId;
        InstitutionName = Check.NotNullOrWhiteSpace(institution, nameof(institution), ProfileConsts.MaxPlaceLength);
        Degree = Check.NotNullOrWhiteSpace(degree, nameof(degree), 128);
        GraduationYear = year;
    }

    public void Update(string institution, string degree, int year)
    {
        InstitutionName = Check.NotNullOrWhiteSpace(institution, nameof(institution), ProfileConsts.MaxPlaceLength);
        Degree = Check.NotNullOrWhiteSpace(degree, nameof(degree), 128);
        GraduationYear = year;
    }
    
    public void SetAcademicDetails(int semester, Guid? collegeId, Guid? majorId, Guid? minorId)
    {
        GraduationSemester = semester;
        CollegeId = collegeId;
        MajorId = majorId;
        MinorId = minorId;
    }
}
