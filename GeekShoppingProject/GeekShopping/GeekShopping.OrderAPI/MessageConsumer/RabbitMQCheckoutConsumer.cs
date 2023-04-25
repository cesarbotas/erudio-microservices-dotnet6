using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model.Entity;
using GeekShopping.OrderAPI.RabbitMQSender;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        private readonly string _hostName = "localhost";
        private readonly string _userName = "guest";
        private readonly string _password = "guest";
        private readonly string _queueName = "checkoutqueue";
        private readonly string _queuePaymentProcess = "orderpaymentprocessqueue";

        public RabbitMQCheckoutConsumer(OrderRepository orderRepository,
            IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _orderRepository = orderRepository;
            _rabbitMQMessageSender = rabbitMQMessageSender;

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_queueName, false, false, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (chanel, ev) =>
            {
                var content = Encoding.UTF8.GetString(ev.Body.ToArray());

                CheckoutHeaderVO headerVO = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);

                ProcessOrder(headerVO).GetAwaiter().GetResult();

                _channel.BasicAck(ev.DeliveryTag, false);
            };

            _channel.BasicConsume(_queueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessOrder(CheckoutHeaderVO headerVO)
        {
            OrderHeader orderHeader = new()
            {
                UserId = headerVO.UserId,
                FirstName = headerVO.FirstName,
                LastName = headerVO.LastName,
                OrderDetails = new List<OrderDetail>(),
                CardNumber = headerVO.CardNumber,
                CouponCode = headerVO.CouponCode,
                CVV = headerVO.CVV,
                DiscountAmount = headerVO.DiscountAmount,
                Phone= headerVO.Phone,
                Email = headerVO.Email,
                ExpiryMonthYear = headerVO.ExpiryMonthYear,
                OrderTime = DateTime.Now,
                PaymentStatus = false,
                OperationDate = headerVO.OperationDate
            };

            foreach (var detail in headerVO.CartDetails)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.Product.Name,
                    Price = detail.Product.Price,
                    Count = detail.Count
                };

                orderHeader.CartTotalItens += detail.Count;
                orderHeader.OrderDetails.Add(orderDetail);
            }

            await _orderRepository.AddOrder(orderHeader);

            PaymentVO payment = new PaymentVO()
            {
                Name = $"{orderHeader.FirstName} {orderHeader.LastName}",
                CardNumber = orderHeader.CardNumber,
                CVV = orderHeader.CVV,
                ExpireMonthYear = orderHeader.ExpiryMonthYear,
                OrderId = orderHeader.Id,
                PurchaseAmout = orderHeader.PurchaseAmount,
                Email = orderHeader.Email
            };

            try
            {
                _rabbitMQMessageSender.SendMessage(payment, _queuePaymentProcess);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}