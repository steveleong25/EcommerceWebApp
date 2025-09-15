/* MySQL version: 8.4 */

/* Create products table */
CREATE TABLE `products` (
  `product_id` int NOT NULL,
  `product_name` varchar(255) NOT NULL,
  `product_type` varchar(60) NOT NULL,
  `product_status` varchar(60) NOT NULL,
  `catalog_visibility` tinyint(1) NOT NULL,
  `stock_quantity` int NOT NULL,
  `sku` varchar(255) NOT NULL,
  `image_urls` json DEFAULT NULL,
  `category` json DEFAULT NULL,
  `variation_ids` varchar(255) NOT NULL,
  PRIMARY KEY (`product_id`)
)

/* Create variations table */
CREATE TABLE `variations` (
  `variation_id` int NOT NULL,
  `sku` varchar(255) NOT NULL,
  `regular_price` decimal(10,2) NOT NULL,
  `unit_of_measurement` varchar(255) NOT NULL,
  PRIMARY KEY (`variation_id`)
) 
