using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Syndicates;

namespace Informatique.Alumni.EntityFrameworkCore.Syndicates;

public class SyndicateSubscriptionConfiguration : IEntityTypeConfiguration<SyndicateSubscription>
{
    public void Configure(EntityTypeBuilder<SyndicateSubscription> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "SyndicateSubscriptions", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        // Navigation collection
        builder.HasMany(p => p.Documents)
            .WithOne()
            .HasForeignKey(x => x.SyndicateSubscriptionId)
            .IsRequired();
            
        // Indexes
        builder.HasIndex(x => new { x.AlumniId, x.SyndicateId });
    }
}
