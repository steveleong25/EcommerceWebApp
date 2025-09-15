using EcommerceWebApp.Helpers;
using EcommerceWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApp.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartApiController : ControllerBase
    {
        [HttpGet("count")]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObject<Cart>("Cart");
            int count = cart?.Items.Count ?? 0;
            return Ok(new { count });
        }
    }
}
