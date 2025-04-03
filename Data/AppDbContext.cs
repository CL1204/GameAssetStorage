using Microsoft.EntityFrameworkCore;
using GameAssetStorage.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace GameAssetStorage.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Your existing lowercase table mappings
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<User>().Property(u => u.username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(u => u.password).HasColumnName("password");

            // Data protection keys configuration
            modelBuilder.Entity<DataProtectionKey>(b =>
            {
                b.ToTable("data_protection_keys");
                b.Property(k => k.FriendlyName).HasColumnName("friendly_name");
                b.Property(k => k.Xml).HasColumnName("xml");
            });
        }
    }
}