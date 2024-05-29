using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace RabbitNetCore
{
    public  class RabbitBase
    {
        private static readonly object Lock = new object();
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitBase(IOptions<RabbitMqSettings> options)
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
        public Task<IModel> GetChannel()
        {
            if (_channel == null || _channel.IsClosed)
            {
                lock (Lock)
                {
                    _channel?.Dispose();
                    _connection = _connectionFactory.CreateConnection();
                    _channel = _connection.CreateModel();
                    ConfigExchange();
                }
            }

            return Task.FromResult(_channel);

        }

        public void test()
        {
            Console.WriteLine("");
        }
        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
