using Microsoft.Extensions.Options;
using System.Text;
using System.Threading.Channels;

namespace RabbitNetCore
{
    public class RabbitMqMessage : IRabbitMqMessage
    {
        private readonly RabbitBase _rabbitBase;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly ILogger<RabbitMqMessage> _logger;
        public RabbitMqMessage(IOptions<RabbitMqSettings> options, RabbitBase rabbitBase, ILogger<RabbitMqMessage> logger)
        {
            _rabbitMqSettings = options.Value;
            _rabbitBase = rabbitBase;
            _logger = logger;
        }
        public  Task DirectSendAsync(string message)
        {
            var channel = _rabbitBase.GetChannel().Result;

            var properties = channel.CreateBasicProperties();
            var exchange = _rabbitMqSettings.Exchanges.Where(x => x.Type == "direct")?.FirstOrDefault();
            var body = Encoding.UTF8.GetBytes(message);
            try
            {
                channel.BasicPublish(exchange?.Name, exchange.Queues.FirstOrDefault()?.Key, true, properties, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            
            return Task.CompletedTask;
        }

        public Task TopicSendAsync(string message)
        {
            var channel = _rabbitBase.GetChannel().Result;

            var properties = channel.CreateBasicProperties();
            var exchange = _rabbitMqSettings.Exchanges.Where(x => x.Type == "topic")?.FirstOrDefault();
            var body = Encoding.UTF8.GetBytes(message);
            try
            {
                channel.BasicPublish(exchange?.Name, exchange.Queues.FirstOrDefault()?.Key, true, properties, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            return Task.CompletedTask;
        }
    }
}
