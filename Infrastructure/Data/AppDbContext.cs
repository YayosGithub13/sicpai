using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;
        public DbSet<RoleCatalog> RoleCatalogs { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId);
            });

            modelBuilder.Entity<RoleCatalog>(entity =>
            {
                entity.ToTable("USERCATALOGS");
                entity.HasKey(e => e.Id);
                entity.HasMany(rc => rc.UserRoles).WithOne(ur => ur.RoleCatalog).HasForeignKey(ur => ur.RoleCatalogId);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("USERROLES");
                entity.HasKey(e => e.Id);
            });
        }
    }
}