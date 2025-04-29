using AutoMapper;
using Loans.Indebtedness.Kafka.Events;
using Loans.Indebtedness.Services;
using Newtonsoft.Json;

namespace Loans.Indebtedness.Kafka.Handlers;

public class CalculateFullLoanValueHandler: IEventHandler<CalculateFullLoanValueEvent>
{
    private readonly ILogger<CalculateFullLoanValueHandler> _logger;
    private readonly ICalculationService<CalculateFullLoanValueEvent> _calculationService;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateFullLoanValueHandler(ICalculationService<CalculateFullLoanValueEvent> calculationService, ILogger<CalculateFullLoanValueHandler> logger, IMapper mapper,IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _calculationService = calculationService;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CalculateFullLoanValueEvent calculationEvent, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _calculationService.CalculateAsync(calculationEvent, cancellationToken);
            
            var @event = new FullLoanValueCalculatedEvent(calculationEvent.ContractId, result, calculationEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateFullLoanValue"];
    
            await _producer.PublishAsync(topic, jsonMessage);
            
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateFullLoanValueEvent. OperationId: {OperationId}", calculationEvent.OperationId);
        }
    }
}