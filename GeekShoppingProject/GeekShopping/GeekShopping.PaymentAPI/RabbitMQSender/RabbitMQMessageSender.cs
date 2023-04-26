using GeekShopping.MessageBus;
using GeekShopping.PaymentAPI.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.RabbitMQSender
{
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private string _queueNamePaymentEmailUpdate = "PaymentEmailUpdateQueue";
        private string _queueNamePaymentOrderUpdate = "PaymentOrderUpdateQueue";
        private string _exchangeName = "DirectPaymentUpdateExchange";
        private string _routingKeyPaymentEmailUpdate = "PaymentEmail";
        private string _routingKeyPaymentOrderUpdate = "PaymentOrder";
        private IConnection _connection;

        public RabbitMQMessageSender()
        {
            _hostName = "localhost";
            _userName = "guest";
            _password = "guest";
        }

        public void SendMessage(BaseMessage message)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: false);

                channel.QueueDeclare(_queueNamePaymentEmailUpdate, false, false, false, null);
                channel.QueueDeclare(_queueNamePaymentOrderUpdate, false, false, false, null);

                channel.QueueBind(_queueNamePaymentEmailUpdate, _exchangeName, _routingKeyPaymentEmailUpdate);
                channel.QueueBind(_queueNamePaymentOrderUpdate, _exchangeName, _routingKeyPaymentOrderUpdate);

                byte[] body = GetMessageAsByteArray(message);

                channel.BasicPublish(exchange: _exchangeName, _routingKeyPaymentEmailUpdate, basicProperties: null, body: body);
                channel.BasicPublish(exchange: _exchangeName, _routingKeyPaymentOrderUpdate, basicProperties: null, body: body);
            }
        }

        private byte[] GetMessageAsByteArray(BaseMessage message)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var json = JsonSerializer.Serialize<UpdatePaymentResultMessage>((UpdatePaymentResultMessage)message, options);

            var body = Encoding.UTF8.GetBytes(json);

            return body;
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _userName,
                    Password = _password
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool ConnectionExists()
        {
            if ( _connection != null ) 
                return true;

            CreateConnection();

            return _connection != null;
        }        
    }
}