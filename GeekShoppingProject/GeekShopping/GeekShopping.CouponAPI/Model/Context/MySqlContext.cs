using GeekShopping.CouponAPI.Model.Entity;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CouponAPI.Model.Context
{
    public class MySqlContext : DbContext
    {
        public MySqlContext(DbContextOptions<MySqlContext> options) : base (options) { }

        public DbSet<Coupon> Coupons { get; set; }
    }
}