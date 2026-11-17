using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("USERS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.UserName).IsUnique();
        entity.HasMany(e => e.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId);
    }
}

public sealed class RoleCatalogConfiguration : IEntityTypeConfiguration<RoleCatalog>
{
    public void Configure(EntityTypeBuilder<RoleCatalog> entity)
    {
        entity.ToTable("USERCATALOGS");
        entity.HasKey(e => e.Id);
        entity.HasMany(rc => rc.UserRoles).WithOne(ur => ur.RoleCatalog).HasForeignKey(ur => ur.RoleCatalogId);
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.ToTable("USERROLES");
        entity.HasKey(e => e.Id);
    }
}

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> entity)
    {
        entity.ToTable("ASSETS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.AssetCode).IsUnique();
        entity.HasIndex(e => e.Barcode);
        entity.Property(e => e.AcquisitionValue).HasPrecision(18, 2);
        entity.HasMany(e => e.Movements).WithOne(e => e.Asset).HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(e => e.Attachments).WithOne(e => e.Asset).HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AssetMovementConfiguration : IEntityTypeConfiguration<AssetMovement>
{
    public void Configure(EntityTypeBuilder<AssetMovement> entity)
    {
        entity.ToTable("ASSETMOVEMENTS");
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.PerformedByUser).WithMany().HasForeignKey(e => e.PerformedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class AssetAttachmentConfiguration : IEntityTypeConfiguration<AssetAttachment>
{
    public void Configure(EntityTypeBuilder<AssetAttachment> entity)
    {
        entity.ToTable("ASSETATTACHMENTS");
        entity.HasKey(e => e.Id);
    }
}

public sealed class InstitutionalAreaConfiguration : IEntityTypeConfiguration<InstitutionalArea>
{
    public void Configure(EntityTypeBuilder<InstitutionalArea> entity)
    {
        entity.ToTable("INSTITUTIONALAREAS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Code).IsUnique();
        entity.HasOne(e => e.ParentArea).WithMany(e => e.Children).HasForeignKey(e => e.ParentAreaId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class PhysicalLocationConfiguration : IEntityTypeConfiguration<PhysicalLocation>
{
    public void Configure(EntityTypeBuilder<PhysicalLocation> entity)
    {
        entity.ToTable("PHYSICALLOCATIONS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Code).IsUnique();
    }
}

public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> entity)
    {
        entity.ToTable("AUDITRECORDS");
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Area).WithMany().HasForeignKey(e => e.AreaId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> entity)
    {
        entity.ToTable("ACTIVITYLOGS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.OccurredAt);
        entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class PermissionCatalogConfiguration : IEntityTypeConfiguration<PermissionCatalog>
{
    public void Configure(EntityTypeBuilder<PermissionCatalog> entity)
    {
        entity.ToTable("PERMISSIONCATALOGS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Code).IsUnique();
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("ROLEPERMISSIONS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.RoleCatalogId, e.PermissionCatalogId }).IsUnique();
        entity.HasOne(e => e.RoleCatalog).WithMany(e => e.RolePermissions).HasForeignKey(e => e.RoleCatalogId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.PermissionCatalog).WithMany(e => e.RolePermissions).HasForeignKey(e => e.PermissionCatalogId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AssetTransferConfiguration : IEntityTypeConfiguration<AssetTransfer>
{
    public void Configure(EntityTypeBuilder<AssetTransfer> entity)
    {
        entity.ToTable("ASSETTRANSFERS");
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Asset).WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.RequestedByUser).WithMany().HasForeignKey(e => e.RequestedByUserId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.ApprovedByUser).WithMany().HasForeignKey(e => e.ApprovedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class InventorySessionConfiguration : IEntityTypeConfiguration<InventorySession>
{
    public void Configure(EntityTypeBuilder<InventorySession> entity)
    {
        entity.ToTable("INVENTORYSESSIONS");
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        entity.HasMany(e => e.Counts).WithOne(e => e.InventorySession).HasForeignKey(e => e.InventorySessionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class InventoryCountConfiguration : IEntityTypeConfiguration<InventoryCount>
{
    public void Configure(EntityTypeBuilder<InventoryCount> entity)
    {
        entity.ToTable("INVENTORYCOUNTS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.InventorySessionId, e.AssetId }).IsUnique();
        entity.HasOne(e => e.Asset).WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.CountedByUser).WithMany().HasForeignKey(e => e.CountedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class AuditAssetResultConfiguration : IEntityTypeConfiguration<AuditAssetResult>
{
    public void Configure(EntityTypeBuilder<AuditAssetResult> entity)
    {
        entity.ToTable("AUDITASSETRESULTS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.AuditRecordId, e.AssetId }).IsUnique();
        entity.HasOne(e => e.AuditRecord).WithMany(e => e.AssetResults).HasForeignKey(e => e.AuditRecordId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Asset).WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(e => e.VerifiedByUser).WithMany().HasForeignKey(e => e.VerifiedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.ToTable("NOTIFICATIONS");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.UserId, e.Read, e.CreatedAt });
        entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}