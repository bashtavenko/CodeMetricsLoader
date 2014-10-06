using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace CodeMetricsLoader.Data
{
    public class LoaderContext : DbContext
    {
        public LoaderContext(string databaseName, IDatabaseInitializer<LoaderContext> initializer)
            : base(databaseName)
        {
            Database.SetInitializer(initializer);
        }

        public LoaderContext(string connectionString)
            : base(nameOrConnectionString: connectionString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<LoaderContext>());
        }

        public LoaderContext() : base("CodeMetricsLoaderWarehouse")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<LoaderContext>());                        
        }

        public DbSet<DimRun> Runs { get; set; }        
        public DbSet<DimDate> Dates { get; set; }
        public DbSet<FactMetrics> Metrics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<DimDate>().HasKey(k => k.DateId);

            modelBuilder.Entity<DimRun>().HasKey(k => k.RunId);

            // Can't have index on nvarchar(max) and max index length is 900          
            modelBuilder.Entity<DimRun>().Property(p => p.Tag).HasColumnType("varchar").IsRequired().HasMaxLength(100);
            modelBuilder.Entity<DimRun>().Property(p => p.Module).HasColumnType("varchar").IsRequired().HasMaxLength(100);
            modelBuilder.Entity<DimRun>().Property(p => p.Namespace).HasColumnType("varchar").HasMaxLength(100);
            modelBuilder.Entity<DimRun>().Property(p => p.Type).HasColumnType("varchar").HasMaxLength(150);
            modelBuilder.Entity<DimRun>().Property(p => p.Member).HasColumnType("varchar").HasMaxLength(255);

            modelBuilder.Entity<FactMetrics>().HasKey(k => k.MetricsId);
        }
    }
}
