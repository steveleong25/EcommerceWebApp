using System.Diagnostics;
using System.Text.Json.Nodes;
using EcommerceWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;

        public HomeController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var productList = await _productService.GetProductsAsync();
            HashSet<string> productCategories = new HashSet<string>();

            foreach (var product in productList)
            {
                foreach (var category in product.Categories)
                {
                    productCategories.Add(category.CategoryName);
                }
            }

            ViewBag.ProductCategories = productCategories;

            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
