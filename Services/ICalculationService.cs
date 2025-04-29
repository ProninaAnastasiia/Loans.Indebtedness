using Loans.Indebtedness.Kafka.Events;

namespace Loans.Indebtedness.Services;

public interface ICalculationService<T>
{
    Task<decimal> CalculateAsync(T calculationEvent, CancellationToken cancellationToken);
}