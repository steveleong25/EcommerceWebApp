using EcommerceWebApp.Data;
using EcommerceWebApp.Helpers;
using EcommerceWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApp.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";
        private readonly ProductRepository _productRepository;

        public CartController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public JsonResult Add([FromBody] CartItemRequest request)
        {
            // Get variation details
            var variationList = _productRepository.GetVariations().Result;
            var variation = variationList.FirstOrDefault(v => v.VariationId == request.VariationId);

            // Get cart from session or create a new one
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(c => c.ProductId == request.VariationId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = request.VariationId,
                    Quantity = request.Quantity,
                    UnitPrice = variation != null ? variation.RegularPrice : 0,
                    UnitOfMeasurement = variation != null ? variation.UnitOfMeasurement : string.Empty
                });
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);

            return Json(new { success = true });
        }
    }
}
