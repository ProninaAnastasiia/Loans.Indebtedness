using Loans.Indebtedness.Kafka.Events;

namespace Loans.Indebtedness.Services;

public class LoanCalculationService : ICalculationService <CalculateContractValuesEvent, ContractValuesCalculatedEvent>
{
    private readonly ILogger<LoanCalculationService> _logger;
    
    private readonly ICalculationService<CalculateContractValuesEvent, decimal> _calculationService;

    public LoanCalculationService(ICalculationService<CalculateContractValuesEvent, decimal> calculationService, ILogger<LoanCalculationService> logger)
    {
        _logger = logger;
        _calculationService = calculationService;
    }

    public async Task<ContractValuesCalculatedEvent> CalculateAsync(CalculateContractValuesEvent request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало комплексного расчета для контракта {ContractId}", request.ContractId);

        try
        {
            Task<decimal> monthlyPaymentTask;
            Task<decimal> totalPaymentTask;
            Task<decimal> totalInterestPaidTask;

            switch (request.PaymentType)
            {
                case "аннуитет":
                    monthlyPaymentTask = Task.FromResult(CalculateAnnuityMonthlyPayment(request.LoanAmount, request.InterestRate, request.LoanTermMonths));
                    totalPaymentTask = Task.FromResult(CalculateAnnuityTotalPayment(request.LoanAmount, request.InterestRate, request.LoanTermMonths));
                    totalInterestPaidTask = Task.FromResult(CalculateAnnuityTotalInterestPaid(request.LoanAmount, request.InterestRate, request.LoanTermMonths));
                    break;

                case "дифф":
                    monthlyPaymentTask = Task.FromResult(decimal.Round(CalculateMonthlyPayments(request).AveragePayment, 2));
                    totalPaymentTask = Task.FromResult(decimal.Round(CalculateMonthlyPayments(request).TotalInterest, 2));
                    totalInterestPaidTask = Task.FromResult(decimal.Round(CalculateMonthlyPayments(request).TotalPayment, 2));
                    break;

                default:
                    throw new ArgumentException($"Неизвестный тип платежа: {request.PaymentType}");
            }
            
            var fullLoanValueTask = CalculateFullLoanValueAsync(request, cancellationToken);

            await Task.WhenAll(monthlyPaymentTask, totalPaymentTask, totalInterestPaidTask, fullLoanValueTask);

            var res = new ContractValuesCalculatedEvent
            (request.ContractId, await monthlyPaymentTask, await totalPaymentTask, 
                await totalInterestPaidTask, await fullLoanValueTask, request.OperationId);

            _logger.LogInformation("Комплексный расчет для контракта {ContractId} завершен", request.ContractId);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при комплексном расчете для контракта {ContractId}", request.ContractId);
            throw;
        }
    }

    private async Task<decimal> CalculateFullLoanValueAsync(CalculateContractValuesEvent request, CancellationToken cancellationToken)
    {
        return await _calculationService.CalculateAsync(request, cancellationToken);
    }
    
    private decimal CalculateAnnuityMonthlyPayment(decimal loanAmount, decimal interestRate, int loanTermMonths)
    {
        decimal monthlyInterestRate = interestRate / 12 / 100;
        decimal annuityCoefficient = (monthlyInterestRate * (decimal)Math.Pow(1 + (double)monthlyInterestRate, loanTermMonths)) /
                                     (decimal)(Math.Pow(1 + (double)monthlyInterestRate, loanTermMonths) - 1);
        return decimal.Round(loanAmount * annuityCoefficient, 2);
    }
    
    private decimal CalculateAnnuityTotalPayment(decimal loanAmount, decimal interestRate, int loanTermMonths)
    {
        var res = CalculateAnnuityMonthlyPayment(loanAmount, interestRate, loanTermMonths) * loanTermMonths;
        return decimal.Round(res, 2);
    }
    
    private decimal CalculateAnnuityTotalInterestPaid(decimal loanAmount, decimal interestRate, int loanTermMonths)
    {
        var res = CalculateAnnuityTotalPayment(loanAmount, interestRate, loanTermMonths) - loanAmount;
        return decimal.Round(res, 2);
    }
    
    private (decimal AveragePayment, decimal TotalInterest, decimal TotalPayment) CalculateMonthlyPayments(CalculateContractValuesEvent request)
    {
        var loanAmount = request.LoanAmount;
        var months = request.LoanTermMonths;
        var annualRate = request.InterestRate;

        var monthlyRate = annualRate / 12 / 100;
        var principalPayment = loanAmount / months;

        decimal totalInterest = 0m;
        decimal totalPayment = 0m;

        for (int month = 0; month < months; month++)
        {
            var remainingPrincipal = loanAmount - principalPayment * month;
            var interestPayment = remainingPrincipal * monthlyRate;
            var monthlyPayment = principalPayment + interestPayment;

            totalInterest += interestPayment;
            totalPayment += monthlyPayment;
        }

        var averagePayment = totalPayment / months;

        return (averagePayment, totalInterest, totalPayment);
    }
}