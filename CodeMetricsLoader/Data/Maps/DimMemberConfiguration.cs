using System.Data.Entity.ModelConfiguration;

namespace CodeMetricsLoader.Data.Maps
{
    public class DimMemberConfiguration : EntityTypeConfiguration<DimMember>
    {
        public DimMemberConfiguration()
        {
           HasKey(k => k.MemberId);
           Property(k => k.Name).HasColumnType("varchar").HasMaxLength(800);
           Property(k => k.File).HasColumnType("varchar").HasMaxLength(90);
        }
    }
}