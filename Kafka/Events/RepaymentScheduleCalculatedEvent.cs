namespace Loans.Indebtedness.Kafka.Events;

public record RepaymentScheduleCalculatedEvent(Guid ContractId, Guid ScheduleId, Guid OperationId) : EventBase;