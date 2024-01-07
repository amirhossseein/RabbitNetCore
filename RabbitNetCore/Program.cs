using RabbitNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//var RabbitMqSettings = new RabbitMqSettings();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
//builder.Configuration.GetSection("RabbitMqSettings").Bind(RabbitMqSettings);
builder.Services.AddSingleton<RabbitBase>();
builder.Services.AddScoped<IRabbitMqMessage,RabbitMqMessage>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
