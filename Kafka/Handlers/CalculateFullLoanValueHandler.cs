using Loans.Indebtedness.Kafka.Events;
using Loans.Indebtedness.Services;
using Newtonsoft.Json;

namespace Loans.Indebtedness.Kafka.Handlers;

public class CalculateFullLoanValueHandler : IEventHandler<CalculateFullLoanValueEvent>
{
    private readonly ILogger<CalculateFullLoanValueHandler> _logger;
    private readonly ICalculationService<CalculateContractValuesEvent, decimal> _calculationService;
    private readonly IConfiguration _config;
    private KafkaProducerService _producer;
    
    public CalculateFullLoanValueHandler(ICalculationService<CalculateContractValuesEvent, decimal> calculationService, ILogger<CalculateFullLoanValueHandler> logger, IConfiguration config, KafkaProducerService producer)
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
            var mappedEvent= new CalculateContractValuesEvent(new Guid(), calculationEvent.LoanAmount, calculationEvent.LoanTermMonths, calculationEvent.InterestRate, calculationEvent.PaymentType, calculationEvent.OperationId);
            var res = await _calculationService.CalculateAsync(mappedEvent, cancellationToken);
            var @event = new FullLoanValueCalculatedEvent(res, calculationEvent.OperationId);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var topic = _config["Kafka:Topics:CalculateIndebtedness"];
    
            await _producer.PublishAsync(topic, jsonMessage);
            
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle CalculateFullLoanValueEvent. OperationId: {OperationId}. Exception: {e}", calculationEvent.OperationId, e.Message);
        }
    }
}