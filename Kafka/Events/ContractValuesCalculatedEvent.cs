﻿namespace Loans.Indebtedness.Kafka.Events;

public record ContractValuesCalculatedEvent(Guid ContractId, decimal MonthlyPaymentAmount,
    decimal TotalPaymentAmount,decimal TotalInterestPaid, decimal FullLoanValue, Guid OperationId) : EventBase;