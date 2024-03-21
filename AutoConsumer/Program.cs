// See https://aka.ms/new-console-template for more information
using AutoConsumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");




IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {


        services.AddHostedService<RabbitMqReceiverService>();
        services.Configure<RabbitMqSettings>(context.Configuration.GetSection("RabbitMqSettings"));
    })
    .UseWindowsService()
    .Build();

await host.RunAsync();

Console.ReadKey();
