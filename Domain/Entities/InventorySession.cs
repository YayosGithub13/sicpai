namespace Domain.Entities;

public class InventorySession
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Abierto";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public int CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public ICollection<InventoryCount> Counts { get; set; } = new List<InventoryCount>();
}
