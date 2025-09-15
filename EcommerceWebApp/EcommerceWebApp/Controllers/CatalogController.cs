using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApp.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ProductService _productService;

        public CatalogController(ProductService productService)
        {
            _productService = productService;
        }

        [Route("/Catalog/Category/{category?}")]
        public IActionResult Category(string category)
        {
            var productList = _productService.GetProductsAsync().Result;

            // Filter products by category
            productList = productList.Where(p => p.Categories.Any(c => c.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase))).ToList();

            ViewBag.SelectedCategory = category;

            return View(productList);
        }
    }
}
