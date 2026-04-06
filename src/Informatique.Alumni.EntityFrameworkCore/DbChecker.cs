using System;
using System.Linq;
using Informatique.Alumni.EntityFrameworkCore;
using Informatique.Alumni.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Informatique.Alumni;

public class DbChecker 
{
    public static void Check(AlumniDbContext db) 
    {
        Console.WriteLine("Total Educations: " + db.Set<Education>().Count());
        Console.WriteLine("With CollegeId: " + db.Set<Education>().Count(e => e.CollegeId != null));
        Console.WriteLine("Total Colleges: " + db.Set<College>().Count());
    }
}
