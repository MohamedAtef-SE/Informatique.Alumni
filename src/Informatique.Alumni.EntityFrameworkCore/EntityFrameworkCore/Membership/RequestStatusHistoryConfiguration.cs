using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Membership;

namespace Informatique.Alumni.EntityFrameworkCore.Membership;

public class RequestStatusHistoryConfiguration : IEntityTypeConfiguration<RequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<RequestStatusHistory> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "RequestStatusHistory", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        builder.Property(x => x.Note).HasMaxLength(500); // Standard length for notes
        builder.HasIndex(x => x.AssociationRequestId);
    }
}
