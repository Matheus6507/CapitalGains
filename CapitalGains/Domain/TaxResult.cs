using System.Text.Json.Serialization;

namespace CapitalGains.Domain
{
    public class TaxResult
    {
        [JsonPropertyName("tax")]
        public decimal Tax { get; set; }

        public TaxResult(decimal tax)
        {
            Tax = tax;
        }
    }
}
