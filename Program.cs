using Loans.Indebtedness.Kafka;
using Loans.Indebtedness.Kafka.Consumers;
using Loans.Indebtedness.Kafka.Events;
using Loans.Indebtedness.Kafka.Handlers;
using Loans.Indebtedness.Mappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IEventHandler<CalculateFullLoanValueEvent>, CalculateFullLoanValueHandler>();

builder.Services.AddHostedService<CalculateIndebtednessConsumer>();
builder.Services.AddSingleton<KafkaProducerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
