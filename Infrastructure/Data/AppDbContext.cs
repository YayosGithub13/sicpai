using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
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

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("ASSETS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssetCode).IsUnique();
                entity.HasIndex(e => e.Barcode);
                entity.Property(e => e.AcquisitionValue).HasPrecision(18, 2);
                entity.HasMany(e => e.Movements).WithOne(e => e.Asset).HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Attachments).WithOne(e => e.Asset).HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssetMovement>(entity =>
            {
                entity.ToTable("ASSETMOVEMENTS");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.PerformedByUser).WithMany().HasForeignKey(e => e.PerformedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<AssetAttachment>(entity =>
            {
                entity.ToTable("ASSETATTACHMENTS");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<InstitutionalArea>(entity =>
            {
                entity.ToTable("INSTITUTIONALAREAS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasOne(e => e.ParentArea).WithMany(e => e.Children).HasForeignKey(e => e.ParentAreaId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PhysicalLocation>(entity =>
            {
                entity.ToTable("PHYSICALLOCATIONS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<AuditRecord>(entity =>
            {
                entity.ToTable("AUDITRECORDS");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Area).WithMany().HasForeignKey(e => e.AreaId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.ToTable("ACTIVITYLOGS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OccurredAt);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PermissionCatalog>(entity =>
            {
                entity.ToTable("PERMISSIONCATALOGS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("ROLEPERMISSIONS");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.RoleCatalogId, e.PermissionCatalogId }).IsUnique();
                entity.HasOne(e => e.RoleCatalog).WithMany(e => e.RolePermissions).HasForeignKey(e => e.RoleCatalogId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.PermissionCatalog).WithMany(e => e.RolePermissions).HasForeignKey(e => e.PermissionCatalogId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AssetTransfer>(entity =>
            {
                entity.ToTable("ASSETTRANSFERS");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Asset).WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.RequestedByUser).WithMany().HasForeignKey(e => e.RequestedByUserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.ApprovedByUser).WithMany().HasForeignKey(e => e.ApprovedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<InventorySession>(entity =>
            {
                entity.ToTable("INVENTORYSESSIONS"); entity.HasKey(e => e.Id);
                entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasMany(e => e.Counts).WithOne(e => e.InventorySession).HasForeignKey(e => e.InventorySessionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InventoryCount>(entity =>
            {
                entity.ToTable("INVENTORYCOUNTS"); entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.InventorySessionId, e.AssetId }).IsUnique();
                entity.HasOne(e => e.Asset).WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.CountedByUser).WithMany().HasForeignKey(e => e.CountedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<AuditAssetResult>(entity =>
            {
                entity.ToTable("AUDITASSETRESULTS"); entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AuditRecordId, e.AssetId }).IsUnique();
                entity.HasOne(e => e.AuditRecord).WithMany(e => e.AssetResults).HasForeignKey(e => e.AuditRecordId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Asset).WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.VerifiedByUser).WithMany().HasForeignKey(e => e.VerifiedByUserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("NOTIFICATIONS"); entity.HasKey(e => e.Id); entity.HasIndex(e => new { e.UserId, e.Read, e.CreatedAt });
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}