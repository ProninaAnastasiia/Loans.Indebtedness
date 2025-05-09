using Loans.Indebtedness.Kafka;
using Loans.Indebtedness.Kafka.Consumers;
using Loans.Indebtedness.Kafka.Events;
using Loans.Indebtedness.Kafka.Handlers;
using Loans.Indebtedness.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEventHandler<CalculateContractValuesEvent>, CalculateContractValuesHandler>();
builder.Services.AddScoped<ICalculationService<CalculateContractValuesEvent, decimal>, FullLoanValueCalculationService>();
builder.Services.AddScoped<ICalculationService<CalculateContractValuesEvent, ContractValuesCalculatedEvent>, LoanCalculationService>();

builder.Services.AddHostedService<CalculateContractValuesConsumer>();
builder.Services.AddSingleton<KafkaProducerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Метрики HTTP
app.UseHttpMetrics(); 

// Экспонирование метрик на /metrics
app.MapMetrics();

app.Run();
