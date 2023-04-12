using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(_cartRepository));
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartVO>> FindById(string userId)
        {
            var cart = await _cartRepository.FindCartByUserId(userId);

            if (cart == null) return NotFound();

            return Ok(cart);
        }        
        
        [HttpPost("add-cart/{id}")]
        public async Task<ActionResult<CartVO>> AddCart(CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [HttpPut("update-cart/{id}")]
        public async Task<ActionResult<CartVO>> UpdateCart(CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<CartVO>> RemoveCart(int id)
        {
            var status = await _cartRepository.RemoveFromCart(id);

            if (!status) return BadRequest();

            return Ok(status);
        }
    }
}