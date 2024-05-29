namespace RabbitNetCore
{
    public interface IRabbitMqMessage
    {
        Task DirectSendAsync(string message);
        Task TopicSendAsync(string message);



    }
}
