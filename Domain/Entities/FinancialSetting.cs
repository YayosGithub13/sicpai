namespace Domain.Entities;

public class FinancialSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public decimal ValueDecimal { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
