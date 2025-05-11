using Loans.Indebtedness.Kafka.Events;
using Loans.Indebtedness.Kafka.Handlers;
using Newtonsoft.Json.Linq;

namespace Loans.Indebtedness.Kafka.Consumers;

public class CalculateIndebtednessConsumer : KafkaBackgroundConsumer
{
    public CalculateIndebtednessConsumer(
        IConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<CalculateIndebtednessConsumer> logger)
        : base(config, serviceProvider, logger,
            topic: config["Kafka:Topics:CalculateIndebtedness"],
            groupId: "indebtedness-service-group",
            consumerName: nameof(CalculateIndebtednessConsumer)) { }

    protected override async Task HandleMessageAsync(JObject message, CancellationToken cancellationToken)
    {
        var eventType = message["EventType"]?.ToString();

        if (eventType?.Contains("CalculateFullLoanValueEvent") == true)
        {
            var @event = message.ToObject<CalculateFullLoanValueEvent>();
            if (@event != null) await ProcessCalculateFullLoanValueEventAsync(@event, cancellationToken);
        }
    }
    private async Task ProcessCalculateFullLoanValueEventAsync(CalculateFullLoanValueEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<CalculateFullLoanValueEvent>>();
            await handler.HandleAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события CalculateFullLoanValueEvent: {EventId}, {OperationId}", @event.EventId, @event.OperationId);
            // Тут можно реализовать retry или логирование в dead-letter-topic
        }
    }
}