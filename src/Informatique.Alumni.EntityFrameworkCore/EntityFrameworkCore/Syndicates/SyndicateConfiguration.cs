using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Syndicates;

namespace Informatique.Alumni.EntityFrameworkCore.Syndicates;

public class SyndicateConfiguration : IEntityTypeConfiguration<Syndicate>
{
    public void Configure(EntityTypeBuilder<Syndicate> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "Syndicates", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(SyndicateConsts.MaxNameLength);
            
        builder.Property(x => x.Description)
            .HasMaxLength(SyndicateConsts.MaxDescriptionLength);
            
        // Requirements is a simple string, no max length constraint in original code but good to have one? 
        // Original code didn't have one on Requirements line 423 of AlumniDbContext.
        // Wait, original code was: 
        // b.Property(x => x.Name).IsRequired().HasMaxLength(SyndicateConsts.MaxNameLength);
        // b.Property(x => x.Description).HasMaxLength(SyndicateConsts.MaxDescriptionLength);
        // Requirements was default.
    }
}
