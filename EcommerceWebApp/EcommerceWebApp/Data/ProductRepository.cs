using EcommerceWebApp.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EcommerceWebApp.Data
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        public async Task AddProducts(dynamic productJsonData)
        {
            var productList = ExtractProduct(productJsonData);

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = GenerateInsertProductsQuery();

                using var cmd = new MySqlCommand(sql, conn, transaction);

                foreach (var product in productList)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@ProductType", product.ProductType);
                    cmd.Parameters.AddWithValue("@ProductStatus", product.ProductStatus);
                    cmd.Parameters.AddWithValue("@CatalogVisibility", product.CatalogVisibility == "1");
                    cmd.Parameters.AddWithValue("@RegularPrice", product.RegularPrice);
                    cmd.Parameters.AddWithValue("@Category", JsonConvert.SerializeObject(product.Categories));
                    cmd.Parameters.AddWithValue("@Sku", product.StockKeepingUnit);
                    cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    cmd.Parameters.AddWithValue("@UnitOfMeasurement", product.UnitOfMeasurement ?? "");
                    cmd.Parameters.AddWithValue("@ImageUrls", JsonConvert.SerializeObject(product.ImageUrl));

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Product>> GetProducts()
        {
            var products = new List<Product>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = GenerateGetProductsQuery();

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new Product
                        {
                            ProductId = reader.GetInt32("product_id"),
                            ProductName = reader.GetString("product_name"),
                            ProductType = reader.GetString("product_type"),
                            ProductStatus = reader.GetString("product_status"),
                            RegularPrice = reader.GetDecimal("regular_price"),
                            CatalogVisibility = reader.GetBoolean("catalog_visibility") == true ? "visible" : string.Empty,
                            StockKeepingUnit = reader.GetString("sku"),
                            StockQuantity = reader.IsDBNull(reader.GetOrdinal("stock_quantity")) ? 0 : reader.GetInt32("stock_quantity"),
                            UnitOfMeasurement = reader.IsDBNull(reader.GetOrdinal("unit_of_measurement")) ? "" : reader.GetString("unit_of_measurement").ToUpper(),
                        };

                        // Handle JSON columns
                        string categoryJson = reader.IsDBNull(reader.GetOrdinal("category")) ? "[]" : reader.GetString("category");
                        product.Categories = JsonConvert.DeserializeObject<List<Category>>(categoryJson);

                        string imageJson = reader.IsDBNull(reader.GetOrdinal("image_urls")) ? "[]" : reader.GetString("image_urls");
                        product.ImageUrl = JsonConvert.DeserializeObject<List<ImageUrl>>(imageJson);

                        products.Add(product);
                    }
                }
            }

            return products;
        }

        private List<Product> ExtractProduct(dynamic productJsonData)
        {
            var productList = new List<Product>();

            foreach (var product in productJsonData.products)
            {
                var p = new Product();

                p.ProductId = product.id;
                p.ProductName = product.name;
                p.ProductType = product.type;
                p.CatalogVisibility = (product.catalog_visibility == "visible") ? "1" : "0";
                p.StockKeepingUnit = product.sku;
                p.ProductType = product.type;
                p.ProductStatus = product.status;
                p.RegularPrice = product.regular_price;
                p.StockQuantity = product.stock_quantity;
                p.Categories = product.categories.ToObject<List<Category>>();
                p.ImageUrl = product.images.ToObject<List<ImageUrl>>();
                var uomAttr = ((JArray)product.attributes).FirstOrDefault(a => (string)a["name"] == "UOM");

                if (uomAttr != null)
                {
                    p.UnitOfMeasurement = string.Join(";", uomAttr["options"].Select(o => (string)o));
                }

                productList.Add(p);
            }

            return productList;
        }

        private string GenerateGetProductsQuery()
        {
            return @"SELECT `product_id`, `product_name`, `product_type`, `product_status`, `regular_price`, `category`,
                    `catalog_visibility`, `sku`, `stock_quantity`, `unit_of_measurement`, `image_urls`
                    FROM `products`;";
        }

        private string GenerateInsertProductsQuery()
        {
            return @"INSERT INTO `products` (`product_id`, `product_name`, `product_type`, `product_status`, 
                        `regular_price`, `category`, `catalog_visibility`, `sku`, 
                        `stock_quantity`, `unit_of_measurement`, `image_urls`) 
                     VALUES (@ProductId, @ProductName, @ProductType, @ProductStatus, 
                            @RegularPrice, @Category, @CatalogVisibility, @Sku, 
                            @StockQuantity, @UnitOfMeasurement, @ImageUrls)";
        }
    }
}
