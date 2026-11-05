namespace Domain.Entities;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleCatalogId { get; set; }
    public int PermissionCatalogId { get; set; }
    public bool Active { get; set; } = true;

    public RoleCatalog RoleCatalog { get; set; } = null!;
    public PermissionCatalog PermissionCatalog { get; set; } = null!;
}
