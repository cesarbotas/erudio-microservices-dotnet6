using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private IRabbitMQMessageSender _rabbitMQMessageSender;
        private readonly ICouponRepository _couponRepository;

        public CartController(ICartRepository cartRepository,
            IRabbitMQMessageSender rabbitMQMessageSender,
            ICouponRepository couponRepository)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _rabbitMQMessageSender = rabbitMQMessageSender ?? throw new ArgumentNullException(nameof(IRabbitMQMessageSender));
            _couponRepository = couponRepository;
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartVO>> FindById(string id)
        {
            var cart = await _cartRepository.FindCartByUserId(id);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart(CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null) return NotFound();

            return Ok(cart);
        }

        [HttpPut("update-cart")]
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

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO cartVO)
        {
            var status = await _cartRepository.ApplyCoupon(cartVO.CartHeader.UserId, cartVO.CartHeader.CouponCode);

            if (!status) return NotFound();

            return Ok(status);
        }

        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
        {
            var status = await _cartRepository.RemoveCoupon(userId);

            if (!status) return NotFound();

            return Ok(status);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO checkoutHeaderVO)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (checkoutHeaderVO?.UserId == null) return BadRequest();

            var cart = await _cartRepository.FindCartByUserId(checkoutHeaderVO.UserId);

            if (cart == null) return NotFound();

            if (!string.IsNullOrEmpty(checkoutHeaderVO.CouponCode))
            {
                CouponVO coupon = await _couponRepository.GetCoupon(checkoutHeaderVO.CouponCode, token);

                if (checkoutHeaderVO.DiscountAmount != coupon.DiscountAmount)
                {
                    return StatusCode(412);
                }
            }

            checkoutHeaderVO.CartDetails = cart.CartDetails;
            checkoutHeaderVO.OperationDate = DateTime.Now;

            _rabbitMQMessageSender.SendMessage(checkoutHeaderVO, "checkoutqueue");

            return Ok(checkoutHeaderVO);
        }
    }
}