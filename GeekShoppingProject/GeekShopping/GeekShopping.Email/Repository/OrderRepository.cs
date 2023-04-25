using GeekShopping.Email.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<MySqlContext> _mySqlContext;

        public OrderRepository(DbContextOptions<MySqlContext> mySqlContext)
        {
            _mySqlContext = mySqlContext;
        }

        public async Task UpdateOrderPaymentStatus(long orderHeaderId, bool status)
        {
            //await using var _dbContext = new MySqlContext(_mySqlContext);

            //var header = await _dbContext.Headers.FirstOrDefaultAsync(o => o.Id == orderHeaderId);

            //if (header != null)
            //{
            //    header.PaymentStatus = status;

            //    await _dbContext.SaveChangesAsync();
            //}
        }
    }
}