namespace Domain.Entities;

public class InventoryCount
{
    public int Id { get; set; }
    public int InventorySessionId { get; set; }
    public int AssetId { get; set; }
    public string Result { get; set; } = "Pendiente";
    public string? Notes { get; set; }
    public DateTime? CountedAt { get; set; }
    public int? CountedByUserId { get; set; }

    public InventorySession InventorySession { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
    public User? CountedByUser { get; set; }
}
