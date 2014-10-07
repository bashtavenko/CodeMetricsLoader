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

        public DbSet<DimTarget> Targets { get; set; }
        public DbSet<DimModule> Modules { get; set; }
        public DbSet<DimNamespace> Namespaces { get; set; }
        public DbSet<DimType> Types { get; set; }
        public DbSet<DimMember> Members { get; set; }                        
        public DbSet<DimDate> Dates { get; set; }
        public DbSet<FactMetrics> Metrics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<DimTarget>().HasKey(k => k.TargetId);
            modelBuilder.Entity<DimModule>().HasKey(k => k.ModuleId);
            modelBuilder.Entity<DimNamespace>().HasKey(k => k.NamespaceId);
            modelBuilder.Entity<DimType>().HasKey(k => k.TypeId);
            modelBuilder.Entity<DimMember>().HasKey(k => k.MemberId);            

            modelBuilder.Entity<DimDate>().HasKey(k => k.DateId);
            
            modelBuilder.Entity<FactMetrics>().HasKey(k => k.MetricsId);
        }
    }
}
