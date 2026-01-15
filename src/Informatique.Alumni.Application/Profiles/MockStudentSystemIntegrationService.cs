using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Mock implementation of Student System Integration Service.
/// Returns dummy data with multi-qualification support for testing.
/// </summary>
public class MockStudentSystemIntegrationService : IStudentSystemIntegrationService
{
    public async Task<List<SisQualification>> GetStudentTranscriptAsync(string studentId)
    {
        // Simulate network delay
        await Task.Delay(100);

        if (studentId == "99999") return new List<SisQualification>(); // Not found case

        // Return Mock Data (BSc and MSc)
        return new List<SisQualification>
        {
            // Qualification 1: Bachelor
            new SisQualification
            {
                QualificationId = "Q1",
                DegreeName = "Bachelor of Computer Science",
                Major = "Software Engineering",
                College = "Faculty of Computers and AI",
                GraduationYear = 2020,
                CumulativeGPA = 3.5m,
                Semesters = new List<SisSemester>
                {
                    new SisSemester 
                    { 
                        SemesterName = "Spring 2020", Year = 2020, SemesterNumber = 2,
                        Courses = new List<SisCourse>
                        {
                            new SisCourse { CourseName = "Graduation Project", Grade = "A" }
                        }
                    }
                }
            },
            // Qualification 2: Master
            new SisQualification
            {
                QualificationId = "Q2",
                DegreeName = "Master of Science",
                Major = "AI",
                College = "Faculty of Computers and AI",
                GraduationYear = 2023,
                CumulativeGPA = 3.8m,
                Semesters = new List<SisSemester>
                {
                    new SisSemester 
                    { 
                        SemesterName = "Spring 2023", Year = 2023, SemesterNumber = 2,
                       Courses = new List<SisCourse> { new SisCourse { CourseName = "Advanced AI", Grade = "A+" } }
                    }
                }
            }
        };
    }

    public Task<bool> StudentExistsAsync(string studentId)
    {
        return Task.FromResult(studentId != "99999");
    }

    public async Task<SisPagedResult<SisExpectedGraduate>> GetExpectedGraduatesAsync(SisExpectedGraduateFilter input)
    {
        await Task.Delay(100);
        
        // Mock Data
        var items = new List<SisExpectedGraduate>
        {
            new SisExpectedGraduate
            {
                StudentId = "20240001",
                NameEn = "Ahmed Ali",
                NameAr = "أحمد علي",
                BranchName = "Main Branch",
                CollegeName = "Engineering",
                MajorName = "Computer Science",
                GPA = 3.8m,
                CreditHoursPassed = 120,
                Email = "ahmed.ali@student.edu",
                Mobile = "01012345678",
                NationalId = "29901010000000",
                BirthDate = new DateTime(1999, 1, 1)
            },
            new SisExpectedGraduate
            {
                StudentId = "20240002",
                NameEn = "Sara Ahmed",
                NameAr = "سارة أحمد",
                BranchName = "Main Branch",
                CollegeName = "Engineering",
                MajorName = "Architecture",
                GPA = 3.9m,
                CreditHoursPassed = 130,
                Email = "sara.ahmed@student.edu",
                Mobile = "01112345678",
                NationalId = "29902020000000",
                BirthDate = new DateTime(1999, 2, 2)
            }
        };

        // Simple filtering for Mock
        if (input.StudentId != null) 
            items = items.Where(x => x.StudentId.Contains(input.StudentId)).ToList();
            
        return new SisPagedResult<SisExpectedGraduate>(items.Count, items);
    }

    /// <summary>
    /// Mock implementation of GetAcademicCalendarAsync.
    /// Returns sample academic calendar data for testing.
    /// Clean Code: Realistic mock data for development/testing.
    /// </summary>
    public Task<List<Informatique.Alumni.AcademicCalendar.AcademicCalendarItem>> GetAcademicCalendarAsync()
    {
        var items = new List<Informatique.Alumni.AcademicCalendar.AcademicCalendarItem>
        {
            new() 
            {
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 9, 15),
                EventName = "Fall Registration",
                Description = "Registration period for Fall 2024 semester",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Fall
            },
            new() 
            {
                StartDate = new DateTime(2024, 9, 16),
                EventName = "Fall Classes Start",
                Description = "First day of classes for Fall 2024",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Fall
            },
            new() 
            {
                StartDate = new DateTime(2024, 12, 15),
                EndDate = new DateTime(2024, 12, 29),
                EventName = "Final Exams - Fall",
                Description = "Final examination period for Fall 2024 semester",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Fall
            },
            new() 
            {
                StartDate = new DateTime(2025, 2, 1),
                EndDate = new DateTime(2025, 2, 15),
                EventName = "Spring Registration",
                Description = "Registration period for Spring 2025 semester",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Spring
            },
            new() 
            {
                StartDate = new DateTime(2025, 2, 16),
                EventName = "Spring Classes Start",
                Description = "First day of classes for Spring 2025",
                Semester = Informatique.Alumni.AcademicCalendar.AcademicSemester.Spring
            }
        };

        return Task.FromResult(items);
    }
}
