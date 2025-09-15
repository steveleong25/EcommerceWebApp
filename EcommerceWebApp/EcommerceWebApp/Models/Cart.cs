namespace EcommerceWebApp.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.Quantity * i.UnitPrice);
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string UnitOfMeasurement { get; set; }
        public decimal UnitPrice { get; set; }

        // DTO properties for display purposes
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public string ProductImageUrl { get; set; }
        public int ProductStockQuantity { get; set; }
    }

    public class CartItemRequest
    {
        public int VariationId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
    }
}
