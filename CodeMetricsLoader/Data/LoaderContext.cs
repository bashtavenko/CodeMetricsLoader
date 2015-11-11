using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CodeMetricsLoader.Data.Maps;

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
        
        public DbSet<DimModule> Modules { get; set; }
        public DbSet<DimNamespace> Namespaces { get; set; }
        public DbSet<DimType> Types { get; set; }
        public DbSet<DimMember> Members { get; set; }                        
        public DbSet<DimDate> Dates { get; set; }
        public DbSet<FactMetrics> Metrics { get; set; }
        public DbSet<DimBranch> Branches { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            
            modelBuilder.Configurations.Add(new DimModuleConfiguration());
            modelBuilder.Configurations.Add(new DimNamespaceConfiguration());
            modelBuilder.Configurations.Add(new DimTypeConfiguration());
            modelBuilder.Configurations.Add(new DimMemberConfiguration());
            modelBuilder.Configurations.Add(new DimDateConfiguration());
            modelBuilder.Configurations.Add(new FactMetricsConfiguration());
            modelBuilder.Configurations.Add(new DimBranchConfiguration());
        }
    }
}
