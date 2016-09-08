using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AdventureWorks2008R2.EntityFramework
{
    public class AdventureWorksDbContext : DbContext
    {
        // this constuctor is required by Horton
        public AdventureWorksDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.AddFromAssembly(typeof(AdventureWorksDbContext).Assembly);
        }
    }
}
