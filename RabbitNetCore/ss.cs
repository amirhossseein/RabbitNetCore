using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQReceiver
{
    private readonly string queueName = "my_queue"; // Match the queue name with your sender
    private readonly IConnection connection;
    private  IModel channel;

   
    public RabbitMQReceiver()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost", // RabbitMQ server hostname
            Port = 5672,            // RabbitMQ server port
            UserName = "guest",     // RabbitMQ username
            Password = "guest",     // RabbitMQ password
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        StartListening();
    }

    private void StartListening()
    {
        Task.Run(() =>
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received: {message}");

                // Handle the received message as needed

                // Acknowledge the message to RabbitMQ (optional)
                channel.BasicAck(e.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            // This loop will run indefinitely until the application is stopped.
            while (true)
            {
                // Optionally, introduce a short delay to avoid tight CPU usage
                Thread.Sleep(1000); // Adjust the sleep duration as needed
            }
        });
    }

    private void ReconnectChannel()
    {
        const int maxRetryCount = 3; // Maximum number of reconnection attempts
        int retryCount = 0;

        while (retryCount < maxRetryCount)
        {
            try
            {
                // Attempt to create a new channel and set up the message listener again
                var newChannel = connection.CreateModel();
                newChannel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                // Update the channel reference with the new channel
                var t = new RabbitMQReceiver();
                channel = t.channel;

                // Reinitialize the message listener on the new channel
                //InitializeMessageListener();

                Console.WriteLine("Channel reconnected successfully.");
                return; // Exit the loop if reconnection is successful
            }
            catch (Exception ex)
            {
                // Log the reconnection error
                Console.WriteLine($"Channel reconnection attempt {retryCount + 1} failed: {ex.Message}");

                // Increment the retry count
                retryCount++;

                // Implement an exponential backoff delay (e.g., wait 1 second before retrying)
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        // If maxRetryCount is reached without successful reconnection, handle it as needed
        Console.WriteLine("Max reconnection attempts reached. Handle the issue.");
    }

    public void Dispose()
    {
        channel.Close();
        connection.Close();
    }
}
