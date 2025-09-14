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
            var productList = ExtractProducts(productJsonData);
            var variationList = ExtractVariations(productJsonData);

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var transaction = await conn.BeginTransactionAsync();

            try
            {
                foreach (var product in productList)
                {
                    var sql = GenerateInsertProductsQuery();
                    using var cmd = new MySqlCommand(sql, conn, transaction);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@ProductType", product.ProductType);
                    cmd.Parameters.AddWithValue("@ProductStatus", product.ProductStatus);
                    cmd.Parameters.AddWithValue("@CatalogVisibility", product.CatalogVisibility == "1");
                    cmd.Parameters.AddWithValue("@Category", JsonConvert.SerializeObject(product.Categories));
                    cmd.Parameters.AddWithValue("@Sku", product.StockKeepingUnit);
                    cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    cmd.Parameters.AddWithValue("@ImageUrls", JsonConvert.SerializeObject(product.ImageUrl));
                    cmd.Parameters.AddWithValue("@VariationIds", product.VariationIds);

                    await cmd.ExecuteNonQueryAsync();
                }

                foreach (var variation in variationList)
                {
                    var sql = GenerateInsertVariationsQuery();
                    using var cmd = new MySqlCommand(sql, conn, transaction);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@VariationId", variation.VariationId);
                    cmd.Parameters.AddWithValue("@RegularPrice", variation.RegularPrice);
                    cmd.Parameters.AddWithValue("@Sku", variation.StockKeepingUnit);
                    cmd.Parameters.AddWithValue("@UnitOfMeasurement", variation.UnitOfMeasurement ?? "");

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
                var getProductSql = GenerateGetProductsQuery();

                // Get all products
                using (var cmd = new MySqlCommand(getProductSql, conn))
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
                            CatalogVisibility = reader.GetBoolean("catalog_visibility") ? "visible" : string.Empty,
                            StockKeepingUnit = reader.GetString("sku"),
                            StockQuantity = reader.GetInt32("stock_quantity"),

                            // Store raw variation string (as in your model)
                            VariationIds = reader.IsDBNull(reader.GetOrdinal("variation_ids"))
                                ? string.Empty
                                : reader.GetString("variation_ids")
                        };

                        var categoryJson = reader.IsDBNull(reader.GetOrdinal("category")) ? "[]" : reader.GetString("category");
                        product.Categories = JsonConvert.DeserializeObject<List<Category>>(categoryJson);

                        var imageJson = reader.IsDBNull(reader.GetOrdinal("image_urls")) ? "[]" : reader.GetString("image_urls");
                        product.ImageUrl = JsonConvert.DeserializeObject<List<ImageUrl>>(imageJson);

                        products.Add(product);
                    }
                }

                // Get all variations
                var getVariationsSql = GenerateGetVariationsQuery();

                var variationsDict = new Dictionary<int, Variation>();

                using (var cmd2 = new MySqlCommand(getVariationsSql, conn))
                using (var reader2 = cmd2.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var variation = new Variation
                        {
                            VariationId = reader2.GetInt32("variation_id"),
                            RegularPrice = reader2.GetDecimal("regular_price"),
                            StockKeepingUnit = reader2.GetString("sku"),
                            UnitOfMeasurement = reader2.IsDBNull(reader2.GetOrdinal("unit_of_measurement")) ? null : reader2.GetString("unit_of_measurement"),
                        };
                        variationsDict[variation.VariationId] = variation;
                    }
                }

                // Map variations back to products
                foreach (var product in products)
                {
                    var ids = string.IsNullOrWhiteSpace(product.VariationIds)
                        ? new List<int>()
                        : product.VariationIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                    product.Variations = ids
                        .Where(id => variationsDict.ContainsKey(id))
                        .Select(id => variationsDict[id])
                        .ToList();
                }
            }

            return products;
        }

        public async Task<List<Variation>> GetVariations()
        {
            var variations = new List<Variation>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var getVariationsSql = GenerateGetVariationsQuery();
                using (var cmd = new MySqlCommand(getVariationsSql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var variation = new Variation
                        {
                            VariationId = reader.GetInt32("variation_id"),
                            RegularPrice = reader.GetDecimal("regular_price"),
                            StockKeepingUnit = reader.GetString("sku"),
                            UnitOfMeasurement = reader.IsDBNull(reader.GetOrdinal("unit_of_measurement")) ? null : reader.GetString("unit_of_measurement"),
                        };
                        variations.Add(variation);
                    }
                }
            }
            return variations;
        }

        private List<Product> ExtractProducts(dynamic productJsonData)
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
                p.StockQuantity = product.stock_quantity;
                p.Categories = product.categories.ToObject<List<Category>>();
                p.ImageUrl = product.images.ToObject<List<ImageUrl>>();
                var variationIds = string.Empty;
                if (product.variations != null && product.variations.HasValues)
                {
                    variationIds = string.Join(",", ((JArray)product.variations).Select(v => (int)v["id"]));
                }

                p.VariationIds = variationIds;

                productList.Add(p);
            }

            return productList;
        }

        private List<Variation> ExtractVariations(dynamic productJsonData)
        {
            var variationList = new List<Variation>();

            foreach (var product in productJsonData.products)
            {
                foreach (var variation in product.variations)
                {
                    var v = new Variation();

                    v.VariationId = variation.id;
                    v.StockKeepingUnit = variation.sku;
                    v.RegularPrice = variation.regular_price;
                    v.UnitOfMeasurement = variation.uom;

                    variationList.Add(v);
                }
            }

            return variationList;
        }

        private string GenerateGetProductsQuery()
        {
            return @"SELECT `product_id`, `product_name`, `product_type`, `product_status`, `category`,
                    `catalog_visibility`, `sku`, `image_urls`, `variation_ids`, `stock_quantity`
                    FROM `products`;";
        }

        private string GenerateGetVariationsQuery()
        {
            return $@"SELECT `variation_id`, `regular_price`, `sku`, `unit_of_measurement`
              FROM variations";
        }

        private string GenerateInsertProductsQuery()
        {
            return @"INSERT INTO `products` (`product_id`, `product_name`, `product_type`, `product_status`, 
                        `category`, `catalog_visibility`, `sku`, 
                        `stock_quantity`, `image_urls`, `variation_ids`) 
                     VALUES (@ProductId, @ProductName, @ProductType, @ProductStatus, @Category,
                            @CatalogVisibility, @Sku, @StockQuantity, @ImageUrls, @VariationIds)";
        }

        private string GenerateInsertVariationsQuery()
        {
            return @"INSERT INTO `variations` (`variation_id`, `regular_price`, `sku`, `unit_of_measurement`) 
                     VALUES (@VariationId, @RegularPrice, @Sku, @UnitOfMeasurement)";
        }
    }
}
