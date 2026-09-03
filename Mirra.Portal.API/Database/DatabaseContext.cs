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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft delete: configurações marcadas como excluídas (e seus agendamentos)
            // ficam invisíveis para todas as consultas da aplicação.
            modelBuilder.Entity<CustomerPlatformConfigurationTableRow>()
                .HasQueryFilter(configuration => !configuration.IsDeleted);

            modelBuilder.Entity<SchedulingTableRow>()
                .HasQueryFilter(scheduling => !scheduling.IsDeleted
                    && !scheduling.CustomerPlatformConfiguration.IsDeleted);
        }

        public DbSet<CustomerTableRow> Customers => Set<CustomerTableRow>();
        public DbSet<PlatformTableRow> Platforms => Set<PlatformTableRow>();
        public DbSet<ParametersTableRow> Parameters => Set<ParametersTableRow>();
        public DbSet<CustomerPlatformConfigurationTableRow> CustomerPlatformsConfiguration => Set<CustomerPlatformConfigurationTableRow>();
        public DbSet<SchedulingTableRow> Schedulings => Set<SchedulingTableRow>();
        public DbSet<ContentTypeTableRow> ContentTypes => Set<ContentTypeTableRow>();
        public DbSet<SubscriptionPlanTableRow> SubscriptionPlans => Set<SubscriptionPlanTableRow>();
        public DbSet<SchedulingStatusTableRow> SchedulingStatus => Set<SchedulingStatusTableRow>();
        public DbSet<SubscriptionStatusTableRow> SubscriptionStatus => Set<SubscriptionStatusTableRow>();
        public DbSet<SubscriptionPaymentLinkTableRow> SubscriptionPaymentLinks => Set<SubscriptionPaymentLinkTableRow>();


    }
}
