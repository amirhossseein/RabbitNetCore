using Microsoft.AspNetCore.Mvc;

namespace RabbitNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRabbitMqMessage _rabbitMqMessage;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IRabbitMqMessage rabbitMqMessage)
        {
            _logger = logger;
            _rabbitMqMessage = rabbitMqMessage;
        }

        [HttpPost(Name = "SendMessage")]
        public async Task<bool> SendMessage([FromBody] inputClass inputClass)
        {
            while (inputClass.Count != 0)
            {
                _logger.LogInformation("message sending ......");

                _rabbitMqMessage.DirectSendAsync($"Direct-message-{inputClass.Count} send.");
                _rabbitMqMessage.TopicSendAsync($"Topic-message-{inputClass.Count} send.");

                inputClass.Count--;
            }

            return true;
        }
    }
}