using Loans.Indebtedness.Kafka.Events;

namespace Loans.Indebtedness.Services;

public interface ICalculationService<TRequest, TResult>
{
    Task<TResult> CalculateAsync(TRequest request, CancellationToken cancellationToken);
}