using Newtonsoft.Json;

namespace EcommerceWebApp.Models
{
    public class Category
    {
        [JsonProperty("id")]
        public int CategoryId { get; set; }
        public int ParentId { get; set; }

        [JsonProperty("name")]
        public string CategoryName { get; set; }
        public bool Indirect { get; set; }
    }
}
