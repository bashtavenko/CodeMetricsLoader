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

        public DbSet<Run> Runs { get; set; }        
        public DbSet<Date> Dates { get; set; }
        public DbSet<Metrics> Metrics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();            
            
            modelBuilder.Entity<Run>().ToTable("DimRun");
            modelBuilder.Entity<Date>().ToTable("DimDate");
            modelBuilder.Entity<Metrics>().ToTable("FactMetrics");

            modelBuilder.Entity<Date>().Property(t => t.DateNoTime).HasColumnName("Date");            
        }
    }
}
