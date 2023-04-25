﻿using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;

        private readonly string _hostName = "localhost";
        private readonly string _userName = "guest";
        private readonly string _password = "guest";
        private readonly string _queuePaymentResult = "orderpaymentresultqueue";

        public RabbitMQPaymentConsumer(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_queuePaymentResult, false, false, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (chanel, ev) =>
            {
                var content = Encoding.UTF8.GetString(ev.Body.ToArray());

                UpdatePaymentResultVO paymentResultVO = JsonSerializer.Deserialize<UpdatePaymentResultVO>(content);

                UpdatePaymentStatus(paymentResultVO).GetAwaiter().GetResult();

                _channel.BasicAck(ev.DeliveryTag, false);
            };

            _channel.BasicConsume(_queuePaymentResult, false, consumer);

            return Task.CompletedTask;
        }

        private async Task UpdatePaymentStatus(UpdatePaymentResultVO paymentResultVO)
        {
            try
            {
                await _orderRepository.UpdateOrderPaymentStatus(paymentResultVO.OrderId, paymentResultVO.Status);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}