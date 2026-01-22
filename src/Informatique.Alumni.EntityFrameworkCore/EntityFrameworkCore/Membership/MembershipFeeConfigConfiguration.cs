using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Informatique.Alumni.Membership;

namespace Informatique.Alumni.EntityFrameworkCore.Membership;

public class MembershipFeeConfigConfiguration : IEntityTypeConfiguration<MembershipFeeConfig>
{
    public void Configure(EntityTypeBuilder<MembershipFeeConfig> builder)
    {
        builder.ToTable(AlumniConsts.DbTablePrefix + "MembershipFeeConfigs", AlumniConsts.DbSchema);
        builder.ConfigureByConvention();
        
        builder.Property(x => x.OneYearFee).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TwoYearsFee).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MoreThanTwoYearsFee).HasColumnType("decimal(18,2)");
    }
}
