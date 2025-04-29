namespace Loans.Indebtedness.Kafka.Events;

public record FullLoanValueCalculatedEvent(Guid ContractId, decimal FullLoanValue, Guid OperationId) : EventBase;