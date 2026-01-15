using System;

namespace Informatique.Alumni.AcademicCalendar;

/// <summary>
/// Domain model for Academic Calendar Item.
/// Clean Architecture: Domain models live in Domain layer, not DTOs.
/// </summary>
public class AcademicCalendarItem
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
