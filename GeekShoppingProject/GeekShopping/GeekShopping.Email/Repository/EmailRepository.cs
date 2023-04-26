using GeekShopping.Email.Messages;
using GeekShopping.Email.Model.Context;
using GeekShopping.Email.Model.Entity;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<MySqlContext> _mySqlContext;

        public EmailRepository(DbContextOptions<MySqlContext> mySqlContext)
        {
            _mySqlContext = mySqlContext;
        }

        public async Task LogEmail(UpdatePaymentResultMessage message)
        {
            EmailLog email = new EmailLog
            {
                Email = message.Email,
                SentDate = DateTime.Now,
                Log = $"Order - {message.OrderId} has been created successully!"
            };

            await using var _dbContext = new MySqlContext(_mySqlContext);

            _dbContext.EmailLogs.Add(email);

            await _dbContext.SaveChangesAsync();
        }
    }
}