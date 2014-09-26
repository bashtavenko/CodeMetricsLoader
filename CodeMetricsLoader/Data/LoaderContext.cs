using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeMetricsLoader.Data
{
    public class LoaderContext : DbContext
    {
        public DbSet<Target> Targets { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Namespace> Namespaces { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Date> Dates { get; set; }
        public DbSet<Metrics> Metrics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Date>().ToTable("DimDate");
            modelBuilder.Entity<Target>().ToTable("DimTarget");
            modelBuilder.Entity<Module>().ToTable("DimModule");
            modelBuilder.Entity<Namespace>().ToTable("DimNamespace");
            modelBuilder.Entity<Type>().ToTable("DimType");
            modelBuilder.Entity<Member>().ToTable("DimMember");
            modelBuilder.Entity<Metrics>().ToTable("FactMetrics");
        }
    }
}
