using Microsoft.EntityFrameworkCore;
using GameAssetStorage.Models;

namespace GameAssetStorage.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<User>().Property(u => u.username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(u => u.password).HasColumnName("password");

            modelBuilder.Entity<Asset>().ToTable("assets");
            modelBuilder.Entity<Asset>().Property(a => a.Id).HasColumnName("id");
            modelBuilder.Entity<Asset>().Property(a => a.title).HasColumnName("title");
            modelBuilder.Entity<Asset>().Property(a => a.description).HasColumnName("description");
            modelBuilder.Entity<Asset>().Property(a => a.imageUrl).HasColumnName("image_url");
            modelBuilder.Entity<Asset>().Property(a => a.isApproved).HasColumnName("is_approved");
            modelBuilder.Entity<Asset>().Property(a => a.likes).HasColumnName("likes");
            modelBuilder.Entity<Asset>().Property(a => a.userId).HasColumnName("user_id");
            modelBuilder.Entity<Asset>().Property(a => a.createdAt).HasColumnName("created_at");
        }
    }
}
