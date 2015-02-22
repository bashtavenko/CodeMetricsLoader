using System.Data.Entity.ModelConfiguration;

namespace CodeMetricsLoader.Data.Maps
{
    public class FactMetricsConfiguration : EntityTypeConfiguration<FactMetrics>
    {
        public FactMetricsConfiguration()
        {
            HasKey(k => k.MetricsId);
        }
    }
}