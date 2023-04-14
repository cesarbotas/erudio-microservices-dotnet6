using GeekShopping.Web.Models.Coupon;

namespace GeekShopping.Web.Services.IServices
{
    public interface ICouponService
    {
        Task<CouponViewModel> GetCoupon(string couponCode, string token);
    }
}