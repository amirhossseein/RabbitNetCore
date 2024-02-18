namespace AutoConsumer
{
    //public class AppSettings
    //{
    //    public  RabbitMqSettings RabbitMqSettings { get; set; }
    //}
    public class Queues
    {
        public string Name { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
        public string Key { get; set; }
    }

    public class Exchanges
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public List<Queues> Queues { get; set; }
    }

    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<Exchanges> Exchanges { get; set; }
    }
}
