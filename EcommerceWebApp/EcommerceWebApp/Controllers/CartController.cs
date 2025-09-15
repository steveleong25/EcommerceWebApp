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

        public IActionResult Cart()
        {
            var cart = GetCart();
            var variationIds = cart.Items.Select(i => i.ProductId).ToList();

            var productList = _productRepository.GetProducts().Result;
            foreach (var item in cart.Items)
            {
                var product = productList.FirstOrDefault(p => !string.IsNullOrEmpty(p.VariationIds) &&
                                                            p.VariationIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                .Select(id => int.Parse(id.Trim()))
                                                                .Contains(item.ProductId));
                if (product != null)
                {
                    item.ProductImageUrl = product.ImageUrl.Any() ? product.ImageUrl[0].SourceSmall : string.Empty;
                    item.ProductStockQuantity = product.StockQuantity;
                }
            }

            return View(cart);
        }

        [HttpPost]
        public JsonResult Add([FromBody] CartItemRequest request)
        {
            // Get product details
            var productList = _productRepository.GetProducts().Result;
            var product = productList.FirstOrDefault(p => !string.IsNullOrEmpty(p.VariationIds) &&
                                                        p.VariationIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                            .Select(id => int.Parse(id.Trim()))
                                                            .Contains(request.VariationId));

            // Get variation details
            var variationList = _productRepository.GetVariations().Result;
            var variation = variationList.FirstOrDefault(v => v.VariationId == request.VariationId);

            // Get cart
            var cart = GetCart();

            var existingItem = cart.Items.FirstOrDefault(c => c.ProductId == request.VariationId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = request.VariationId,
                    Quantity = request.Quantity,
                    UnitPrice = variation != null ? variation.RegularPrice : 0,
                    UnitOfMeasurement = variation != null ? variation.UnitOfMeasurement : string.Empty,
                    ProductName = product != null ? product.ProductName : string.Empty,
                    ProductSku = product != null ? product.StockKeepingUnit : string.Empty,
                });
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int variationId)
        {
            var cart = GetCart();
            if (cart.Items.Any(i => i.ProductId == variationId))
            {
                cart.Items.First(i => i.ProductId == variationId).Quantity++;
                SaveCart(cart);
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int variationId)
        {
            var cart = GetCart();
            if (cart.Items.Any(i => i.ProductId == variationId))
            {
                var cartItem = cart.Items.First(i => i.ProductId == variationId);
                cartItem.Quantity--;

                if (cartItem.Quantity <= 0)
                    cart.Items.Remove(cartItem);

                SaveCart(cart);
            }

            return RedirectToAction("Cart");
        }


        [HttpPost]
        public IActionResult RemoveItem(int variationId)
        {
            var cart = GetCart();
            if (cart.Items.Any(i => i.ProductId == variationId))
            {
                cart.Items.RemoveAll(i => i.ProductId == variationId);
                SaveCart(cart);
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult ClearCart(string returnUrl)
        {
            var cart = GetCart();
            cart.Items.Clear();
            SaveCart(cart);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private Cart GetCart()
        {
            return HttpContext.Session.GetObject<Cart>(CartSessionKey) ?? new Cart();
        }

        private void SaveCart(Cart cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

    }
}
