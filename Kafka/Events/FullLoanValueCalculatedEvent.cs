namespace Loans.Indebtedness.Kafka.Events;

public record FullLoanValueCalculatedEvent(decimal FullLoanValue, Guid OperationId) : EventBase;