using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Syndicates;

namespace Informatique.Alumni.EntityFrameworkCore.Syndicates;

public class SyndicateDocumentConfiguration : IEntityTypeConfiguration<SyndicateDocument>
{
    public void Configure(EntityTypeBuilder<SyndicateDocument> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "SyndicateDocuments", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        builder.Property(x => x.RequirementName)
            .IsRequired()
            .HasMaxLength(128);
            
        builder.Property(x => x.FileBlobName)
            .IsRequired()
            .HasMaxLength(256);

        // Index for ForeignKey is automatically created by EF Core, but we can be explicit if we want.
        builder.HasIndex(x => x.SyndicateSubscriptionId);
    }
}
