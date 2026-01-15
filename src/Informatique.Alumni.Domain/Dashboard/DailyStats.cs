using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Dashboard;

public class DailyStats : Entity<Guid>
{
    public DateTime Date { get; set; }
    public int TotalAlumniCount { get; set; }
    public double EmploymentRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveJobsCount { get; set; }
    public DateTime LastUpdated { get; set; }

    private DailyStats() { }

    public DailyStats(Guid id, DateTime date) : base(id)
    {
        Date = date.Date;
        LastUpdated = DateTime.Now;
    }
}
