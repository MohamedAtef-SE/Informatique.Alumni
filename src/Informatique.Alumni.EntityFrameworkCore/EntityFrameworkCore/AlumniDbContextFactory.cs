using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Informatique.Alumni.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AlumniDbContextFactory : IDesignTimeDbContextFactory<AlumniDbContext>
{
    public AlumniDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        AlumniEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<AlumniDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new AlumniDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "../Informatique.Alumni.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
