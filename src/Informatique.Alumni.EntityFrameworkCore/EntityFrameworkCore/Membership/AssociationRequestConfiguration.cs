using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Membership;

namespace Informatique.Alumni.EntityFrameworkCore.Membership;

public class AssociationRequestConfiguration : IEntityTypeConfiguration<AssociationRequest>
{
    public void Configure(EntityTypeBuilder<AssociationRequest> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "AssociationRequests", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        builder.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(128);
        
        builder.HasIndex(x => x.AlumniId);
        builder.HasIndex(x => x.SubscriptionFeeId);
        builder.HasIndex(x => x.Status);
        
        // History relation
        builder.HasMany(x => x.StatusHistory)
               .WithOne()
               .HasForeignKey(x => x.AssociationRequestId)
               .IsRequired();
    }
}
