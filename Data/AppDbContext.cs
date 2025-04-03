using Microsoft.EntityFrameworkCore;
using GameAssetStorage.Models;

namespace GameAssetStorage.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users"); // Explicit lowercase
            modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<User>().Property(u => u.username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(u => u.password).HasColumnName("password");
        }
    }
}