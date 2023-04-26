using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender;
using GeekShopping.PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQMessageSender _rabbitMQMessageSender;
        private readonly IProcessPayment _processPayment;

        private readonly string _hostName = "localhost";
        private readonly string _userName = "guest";
        private readonly string _password = "guest";
        private readonly string _queuePaymentProcess = "orderpaymentprocessqueue";
        private readonly string _queuePaymentResult = "orderpaymentresultqueue";

        public RabbitMQPaymentConsumer(IRabbitMQMessageSender rabbitMQMessageSender,
            IProcessPayment processPayment)
        {
            _rabbitMQMessageSender = rabbitMQMessageSender;
            _processPayment = processPayment;

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_queuePaymentProcess, false, false, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (chanel, ev) =>
            {
                var content = Encoding.UTF8.GetString(ev.Body.ToArray());

                PaymentMessage paymentMessage = JsonSerializer.Deserialize<PaymentMessage>(content);

                ProcessPayment(paymentMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ev.DeliveryTag, false);
            };

            _channel.BasicConsume(_queuePaymentProcess, false, consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessPayment(PaymentMessage paymentMessage)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage paymentResultMessage = new UpdatePaymentResultMessage
            {
                Status = result,
                OrderId = paymentMessage.OrderId,
                Email = paymentMessage.Email
            };

            try
            {
                _rabbitMQMessageSender.SendMessage(paymentResultMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}