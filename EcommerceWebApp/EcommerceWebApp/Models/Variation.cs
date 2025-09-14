using Newtonsoft.Json;

namespace EcommerceWebApp.Models
{
    public class Variation
    {
        [JsonProperty("id")]
        public int VariationId { get; set; }

        [JsonProperty("sku")]
        public string StockKeepingUnit { get; set; }

        [JsonProperty("regular_price")]
        public decimal RegularPrice { get; set; }

        [JsonProperty("uom")]
        public string UnitOfMeasurement { get; set; }

    }
}
