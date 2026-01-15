using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

public class Experience : Entity<Guid>
{
    public Guid AlumniProfileId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string JobTitle { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Description { get; private set; }

    private Experience() { }

    public Experience(Guid id, Guid profileId, string company, string title, DateTime start)
        : base(id)
    {
        AlumniProfileId = profileId;
        CompanyName = Check.NotNullOrWhiteSpace(company, nameof(company), ProfileConsts.MaxPlaceLength);
        JobTitle = Check.NotNullOrWhiteSpace(title, nameof(title), ProfileConsts.MaxJobTitleLength);
        StartDate = start;
    }

    public void Update(string company, string title, DateTime start, DateTime? endDate = null, string? description = null)
    {
        CompanyName = Check.NotNullOrWhiteSpace(company, nameof(company), ProfileConsts.MaxPlaceLength);
        JobTitle = Check.NotNullOrWhiteSpace(title, nameof(title), ProfileConsts.MaxJobTitleLength);
        StartDate = start;
        EndDate = endDate;
        
        if (description != null)
        {
            Description = Check.Length(description, nameof(description), ProfileConsts.MaxBioLength);
        }
        else
        {
            Description = null;
        }
    }
}
