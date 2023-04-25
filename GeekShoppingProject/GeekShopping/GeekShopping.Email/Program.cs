using GeekShopping.Email.MessageConsumer;
using GeekShopping.Email.Model.Context;
using GeekShopping.Email.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration["MySqlConnection:MySqlConnectionString"];

builder.Services.AddDbContext<MySqlContext>(options => options
    .UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 25))));

var builderContext = new DbContextOptionsBuilder<MySqlContext>();

builderContext.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 25)));

builder.Services.AddSingleton(new EmailRepository(builderContext.Options));

builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddHostedService<RabbitMQPaymentConsumer>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
