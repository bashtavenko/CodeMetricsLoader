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

        public DbSet<Target> Targets { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Namespace> Namespaces { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Date> Dates { get; set; }
        public DbSet<Metrics> Metrics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Date>().ToTable("DimDate");
            modelBuilder.Entity<Target>().ToTable("DimTarget");
            modelBuilder.Entity<Module>().ToTable("DimModule");
            modelBuilder.Entity<Namespace>().ToTable("DimNamespace");
            modelBuilder.Entity<Type>().ToTable("DimType");
            modelBuilder.Entity<Member>().ToTable("DimMember");
            modelBuilder.Entity<Metrics>().ToTable("FactMetrics");

            modelBuilder.Entity<Date>().Property(t => t.DateNoTime).HasColumnName("Date");            
        }
    }
}
