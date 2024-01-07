using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqConsumer;
using System.Text;

namespace RabbitMqConsumer
{
    public class RabbitMqReceiverService : BackgroundService
    {
        private static readonly object Lock = new object();
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly ILogger<RabbitMqReceiverService> _logger;

        public RabbitMqReceiverService(IOptions<RabbitMqSettings> options, ILogger<RabbitMqReceiverService> logger)
        {
            _rabbitMqSettings = options.Value;
            _connectionFactory = new ConnectionFactory()
            {
                HostName = _rabbitMqSettings.Host,
                Port = _rabbitMqSettings.Port,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password
            };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            ConfigExchange();
            _logger = logger;
        }

        private void ConfigExchange()
        {
            if (_rabbitMqSettings.Exchanges.Any())
            {
                foreach (var exchange in _rabbitMqSettings.Exchanges)
                {
                    _channel.ExchangeDeclare(exchange.Name, exchange.Type, true, false, null);
                    if (exchange.Queues.Any())
                    {
                        foreach (var queue in exchange.Queues)
                        {
                            _channel.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete, null);
                            _channel.QueueBind(queue.Name, exchange.Name, queue.Key);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("RabbitMq Exchegs Not Defined.");
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received: {message}");

                _channel.BasicNack(e.DeliveryTag, false,true);
            };

            var Topicconsumer = new EventingBasicConsumer(_channel);
            Topicconsumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received-------Topic: {message}");

                _channel.BasicNack(e.DeliveryTag, false,false);
            };

            _channel.BasicConsume("secondeQ", false, Topicconsumer);
            _channel.BasicConsume("firstQ", false, consumer);

            while (!stoppingToken.IsCancellationRequested) 
            {
                await Task.Delay(1000,stoppingToken);
            }

            _channel.Close();
            _connection.Close();
        }

    }
}
