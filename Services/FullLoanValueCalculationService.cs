using Loans.Indebtedness.Kafka.Events;

namespace Loans.Indebtedness.Services;

public class FullLoanValueCalculationService : ICalculationService<CalculateContractValuesEvent, decimal>
{
    private readonly ILogger<FullLoanValueCalculationService> _logger;

    public FullLoanValueCalculationService(ILogger<FullLoanValueCalculationService> logger)
    {
        _logger = logger;
    }
    
    public async Task<decimal> CalculateAsync(CalculateContractValuesEvent calculationEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Имитация сложного расчета с помощью коэффициента
            var coefficient = await GenerateRandomCoefficient(cancellationToken);

            decimal psk = calculationEvent.PaymentType.ToLower() switch
            {
                "аннуитет" => await CalculateAnnuityPskAsync(calculationEvent, coefficient, cancellationToken),
                "дифф" => await CalculateDifferentiatedPskAsync(calculationEvent, coefficient, cancellationToken),
                _ => throw new ArgumentException($"Неизвестный тип платежа: {calculationEvent.PaymentType}")
            };

            return psk;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Расчет ПСК по операции {OperationId} отменен", calculationEvent.OperationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при расчете ПСК для контракта {OperationId}", calculationEvent.OperationId);
            throw;
        }
    }

    private async Task<decimal> CalculateAnnuityPskAsync(CalculateContractValuesEvent calculationEvent, decimal coefficient, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Расчет ПСК для аннуитетного типа платежа");
        await Task.Yield();
        return calculationEvent.InterestRate*coefficient;
    }

    private async Task<decimal> CalculateDifferentiatedPskAsync(CalculateContractValuesEvent calculationEvent, decimal coefficient, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Расчет ПСК для дифференцированного типа платежа");
        await Task.Yield();
        return calculationEvent.InterestRate * (1 - coefficient);
    }

    private async Task<decimal> GenerateRandomCoefficient(CancellationToken cancellationToken)
    {
        int delayMilliseconds = new Random().Next(100, 500); // Случайная задержка от 0.1 до 0.5 секунд
        await Task.Delay(delayMilliseconds, cancellationToken);
        decimal min = 1.1m;
        decimal max = 1.5m;
        Random random = new Random();
        return min + (decimal)random.NextDouble() * (max - min);
    }
}