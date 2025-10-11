using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.DBEntities;
using System.Diagnostics;

namespace Mirra_Portal_API.Database
{
    public class DatabaseContext : DbContext
    {

        private readonly string connectionString;

        public DatabaseContext(IConfiguration configuration, DbContextOptions<DatabaseContext> options) : base(options)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);

#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging()
                   .LogTo(Console.WriteLine, LogLevel.Information)
                   .LogTo(message => Debug.WriteLine(message), LogLevel.Information);
#endif
        }

        public DbSet<CustomerTableRow> Customers => Set<CustomerTableRow>();
        public DbSet<PlatformTableRow> Platforms => Set<PlatformTableRow>();
        public DbSet<ParametersTableRow> Parameters => Set<ParametersTableRow>();
        public DbSet<CustomerPlatformConfigurationTableRow> CustomerPlatformsConfiguration => Set<CustomerPlatformConfigurationTableRow>();
        public DbSet<SchedulingTableRow> Schedulings => Set<SchedulingTableRow>();

    }
}
