using Microsoft.EntityFrameworkCore;
using GameAssetStorage.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace GameAssetStorage.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Avoid creating the Users table
            modelBuilder.Entity<User>().ToTable("users"); // Explicitly define the table name
            modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<User>().Property(u => u.username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(u => u.password).HasColumnName("password");
            //modelBuilder.Entity<User>().Property(u => u.created_at).HasColumnName("created_at");
            modelBuilder.Entity<User>().Property(u => u.is_admin).HasColumnName("is_admin");
            modelBuilder.Entity<User>().Property(u => u.is_banned).HasColumnName("is_banned");

            // Assets table creation
            modelBuilder.Entity<Asset>().ToTable("assets");
            modelBuilder.Entity<Asset>().Property(a => a.Id).HasColumnName("id");
            modelBuilder.Entity<Asset>().Property(a => a.Title).HasColumnName("title");
            modelBuilder.Entity<Asset>().Property(a => a.Description).HasColumnName("description");
            modelBuilder.Entity<Asset>().Property(a => a.ImageUrl).HasColumnName("image_url");
            modelBuilder.Entity<Asset>().Property(a => a.IsApproved).HasColumnName("is_approved");
            modelBuilder.Entity<Asset>().Property(a => a.Likes).HasColumnName("likes");
            modelBuilder.Entity<Asset>().Property(a => a.UserId).HasColumnName("user_id");
            modelBuilder.Entity<Asset>().Property(a => a.CreatedAt).HasColumnName("created_at");

            // Data protection keys table
            modelBuilder.Entity<DataProtectionKey>(b =>
            {
                b.ToTable("data_protection_keys");
                b.Property(k => k.FriendlyName).HasColumnName("friendly_name");
                b.Property(k => k.Xml).HasColumnName("xml");
            });
        }
    }
}
