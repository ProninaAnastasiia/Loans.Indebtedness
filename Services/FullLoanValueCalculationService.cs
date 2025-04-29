using Loans.Indebtedness.Kafka.Events;

namespace Loans.Indebtedness.Services;

public class FullLoanValueCalculationService : ICalculationService<CalculateFullLoanValueEvent>
{
    private readonly ILogger<FullLoanValueCalculationService> _logger;

    public FullLoanValueCalculationService(ILogger<FullLoanValueCalculationService> logger)
    {
        _logger = logger;
    }
    
    public async Task<decimal> CalculateAsync(CalculateFullLoanValueEvent calculationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало расчета ПСК для контракта {ContractId}", calculationEvent.ContractId);

        try
        {
            // Имитация сложного расчета с помощью коэффициента
            var coefficient = await GenerateRandomCoefficient(cancellationToken);
            _logger.LogInformation("Имитация сложного расчета с коэффициентом {Coefficient}", coefficient);

            decimal psk = calculationEvent.PaymentType.ToLower() switch
            {
                "annuity" => await CalculateAnnuityPskAsync(calculationEvent, coefficient, cancellationToken),
                "differentiated" => await CalculateDifferentiatedPskAsync(calculationEvent, coefficient, cancellationToken),
                _ => throw new ArgumentException($"Неизвестный тип платежа: {calculationEvent.PaymentType}")
            };

            _logger.LogInformation("ПСК для контракта {ContractId} рассчитана: {Psk}", calculationEvent.ContractId, psk);
            return psk;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Расчет ПСК для контракта {ContractId} отменен", calculationEvent.ContractId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при расчете ПСК для контракта {ContractId}", calculationEvent.ContractId);
            throw;
        }
    }

    private async Task<decimal> CalculateAnnuityPskAsync(CalculateFullLoanValueEvent calculationEvent, decimal coefficient, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Расчет ПСК для аннуитетного типа платежа");
        await Task.Yield();
        return calculationEvent.InterestRate*coefficient;
    }

    private async Task<decimal> CalculateDifferentiatedPskAsync(CalculateFullLoanValueEvent calculationEvent, decimal coefficient, CancellationToken cancellationToken)
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