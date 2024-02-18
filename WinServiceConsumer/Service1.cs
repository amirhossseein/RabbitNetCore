using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WinServiceConsumer
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;
        private readonly RabbitMqSettings _rabbitMqSettings;
        public Service1(RabbitMqSettings options)
        {
            InitializeComponent();
            _rabbitMqSettings = options;
            _connectionFactory = new ConnectionFactory()
            {
                HostName = _rabbitMqSettings.Host,
                Port = _rabbitMqSettings.Port,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password
            };

            CreateChannel();
            ConfigExchange();
            WriteToFile("Service run here");
        }

        private void CreateChannel()
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumer = new EventingBasicConsumer(_channel);
        }
        private void ConfigExchange()
        {
            if (_rabbitMqSettings.Exchanges.Any())
            {
                //foreach (var exchange in _rabbitMqSettings.Exchanges)
                //{
                //    _channel.ExchangeDeclare(exchange.Name, exchange.Type, true, false, null);
                //    if (exchange.Queues.Any())
                //    {
                //        foreach (var queue in exchange.Queues)
                //        {
                //            _channel.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete, null);
                //            _channel.QueueBind(queue.Name, exchange.Name, queue.Key);
                //        }
                //    }
                //}
            }
            else
            {
                throw new Exception("RabbitMq Exchegs Not Defined.");
            }
        }
        protected override void OnStart(string[] args)
        {
            if (_channel.IsOpen == false)
            {
                CreateChannel();
            }
            _channel.BasicConsume("firstQ", false, _consumer);
            _consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                WriteToFile($"SMessage is {message} ");

                _channel.BasicNack(e.DeliveryTag, false, false);
                //Task.Delay(500);
            };
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 100000; //number in milisecinds
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}

