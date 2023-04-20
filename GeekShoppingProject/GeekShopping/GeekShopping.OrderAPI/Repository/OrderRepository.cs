using GeekShopping.OrderAPI.Model.Context;
using GeekShopping.OrderAPI.Model.Entity;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<MySqlContext> _mySqlContext;

        public OrderRepository(DbContextOptions<MySqlContext> mySqlContext)
        {
            _mySqlContext = mySqlContext;
        }

        public async Task<bool> AddOrder(OrderHeader header)
        {
            if (header == null) return false;

            await using var _dbContext = new MySqlContext(_mySqlContext);

            _dbContext.Headers.Add(header);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task UpdateOrderPaymentStatus(long orderHeaderId, bool status)
        {
            await using var _dbContext = new MySqlContext(_mySqlContext);

            var header = await _dbContext.Headers.FirstOrDefaultAsync(o => o.Id == orderHeaderId);

            if (header != null)
            {
                header.PaymentStatus = status;

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}