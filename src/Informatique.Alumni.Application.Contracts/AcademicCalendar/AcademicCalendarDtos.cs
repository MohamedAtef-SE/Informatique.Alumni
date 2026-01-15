using System;

namespace Informatique.Alumni.AcademicCalendar;

public class AcademicCalendarItemDto
{
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string EventName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public AcademicSemester Semester { get; set; }
}

public enum AcademicSemester
{
    Fall = 1,
    Spring = 2,
    Summer = 3
}
