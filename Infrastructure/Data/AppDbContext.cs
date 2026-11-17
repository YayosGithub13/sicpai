using Domain.Entities;
using Infrastructure.Configurations;
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
        public DbSet<Asset> Assets { get; set; } = default!;
        public DbSet<AssetMovement> AssetMovements { get; set; } = default!;
        public DbSet<AssetAttachment> AssetAttachments { get; set; } = default!;
        public DbSet<InstitutionalArea> InstitutionalAreas { get; set; } = default!;
        public DbSet<PhysicalLocation> PhysicalLocations { get; set; } = default!;
        public DbSet<AuditRecord> AuditRecords { get; set; } = default!;
        public DbSet<ActivityLog> ActivityLogs { get; set; } = default!;
        public DbSet<PermissionCatalog> PermissionCatalogs { get; set; } = default!;
        public DbSet<RolePermission> RolePermissions { get; set; } = default!;
        public DbSet<AssetTransfer> AssetTransfers { get; set; } = default!;
        public DbSet<InventorySession> InventorySessions { get; set; } = default!;
        public DbSet<InventoryCount> InventoryCounts { get; set; } = default!;
        public DbSet<AuditAssetResult> AuditAssetResults { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<FinancialSetting> FinancialSettings { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        }
    }
}