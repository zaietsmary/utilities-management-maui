using Lab7.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab7.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<AccountService> AccountServices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasKey(a => a.AccountNumber);

            modelBuilder.Entity<Account>()
                .Property(a => a.AccountNumber)
                .ValueGeneratedNever();

            modelBuilder.Entity<AccountService>()
                .HasKey(acs => new { acs.AccountNumber, acs.ServiceId });

            modelBuilder.Entity<AccountService>()
                .HasOne(acs => acs.Account)
                .WithMany(a => a.AccountServices)
                .HasForeignKey(acs => acs.AccountNumber);

            modelBuilder.Entity<AccountService>()
                .HasOne(acs => acs.Service)
                .WithMany(s => s.AccountServices)
                .HasForeignKey(acs => acs.ServiceId);

            modelBuilder.Entity<Service>()
                .Property(s => s.PricePerSqm)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Service>()
                .Property(s => s.PricePerPerson)
                .HasColumnType("decimal(18,2)");
        }
    }
}