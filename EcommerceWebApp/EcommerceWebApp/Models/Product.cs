namespace EcommerceWebApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string CatalogVisibility { get; set; }
        public string StockKeepingUnit { get; set; }
        public string UnitOfMeasurement { get; set; }
        public decimal RegularPrice { get; set; }

        public string ImageUrl { get; set; }
    }
}
