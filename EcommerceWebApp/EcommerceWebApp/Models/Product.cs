using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EcommerceWebApp.Models
{
    public class Product
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string ProductName { get; set; }

        [JsonProperty("type")]
        public string ProductType { get; set; }

        [JsonProperty("status")]
        public string ProductStatus { get; set; }

        [JsonProperty("catalog_visibility")]
        public string CatalogVisibility { get; set; }

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }

        [JsonProperty("sku")]
        public string StockKeepingUnit { get; set; }

        [JsonProperty("regular_price")]
        public decimal RegularPrice { get; set; }

        [JsonProperty("stock_quantity")]
        public int StockQuantity { get; set; }

        public string UnitOfMeasurement { get; set; }

        [JsonProperty("images")]
        public List<ImageUrl> ImageUrl { get; set; }
    }

    public class ProductResponse
    {
        [JsonProperty("products")]
        public List<Product> Products { get; set; }
    }

    public class ImageUrl
    {
        [JsonProperty("src")]
        public string OriginalSource { get; set; }

        [JsonProperty("src_small")]
        public string SourceSmall { get; set; }

        [JsonProperty("src_medium")]
        public string SourceMedium { get; set; }

        [JsonProperty("src_large")]
        public string SourceLarge { get; set; }
    }
}
