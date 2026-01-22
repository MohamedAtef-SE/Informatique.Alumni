using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Membership;

namespace Informatique.Alumni.EntityFrameworkCore.Membership;

public class SubscriptionFeeConfiguration : IEntityTypeConfiguration<SubscriptionFee>
{
    public void Configure(EntityTypeBuilder<SubscriptionFee> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "SubscriptionFees", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(MembershipConsts.MaxNameLength);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}
