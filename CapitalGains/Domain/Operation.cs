using System.Text.Json.Serialization;

namespace CapitalGains.Domain;

public class Operation
{
    [JsonPropertyName("operation")]
    public string OperationType { get; set; } = string.Empty;
    
    [JsonPropertyName("unit-cost")]
    public decimal UnitCost { get; set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    
    public bool IsBuy => OperationType.Equals("buy", StringComparison.OrdinalIgnoreCase);
    public bool IsSell => OperationType.Equals("sell", StringComparison.OrdinalIgnoreCase);
    public decimal TotalValue => UnitCost * Quantity;
}
