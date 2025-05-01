using AutoMapper;
using Loans.Indebtedness.Kafka.Events;
using Loans.Indebtedness.Services;
using Newtonsoft.Json;

namespace Loans.Indebtedness.Kafka.Handlers;

public class CalculateContractValuesHandler: IEventHandler<CalculateContractValuesEvent>
{
    private readonly ILogger<CalculateContractValuesHandler> _logger;
    private readonly ICalculationService<CalculateContractValuesEvent, ContractValuesCalculatedEvent> _calculationService;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateContractValuesHandler(ICalculationService<CalculateContractValuesEvent, ContractValuesCalculatedEvent> calculationService, ILogger<CalculateContractValuesHandler> logger, IMapper mapper,IConfiguration config, KafkaProducerService producer)
    {
        _logger = logger;
        _calculationService = calculationService;
        _config = config;
        _producer = producer;
    }
    
    public async Task HandleAsync(CalculateContractValuesEvent calculationEvent, CancellationToken cancellationToken)
    {
        try
        {
            var @event = await _calculationService.CalculateAsync(calculationEvent, cancellationToken);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateIndebtedness"];
    
            await _producer.PublishAsync(topic, jsonMessage);
            
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateContractValuesEvent. ContractId: {ContractId} OperationId: {OperationId}", calculationEvent.ContractId, calculationEvent.OperationId);
        }
    }
}