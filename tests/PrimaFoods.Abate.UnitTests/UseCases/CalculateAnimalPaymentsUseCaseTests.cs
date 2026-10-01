using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;

namespace PrimaFoods.Abate.UnitTests.UseCases;

public class CalculateAnimalPaymentsUseCaseTests
{
    private sealed class FakeCalculator(CalculationResult result) : IPaymentCalculator
    {
        public CancellationToken ReceivedToken { get; private set; }

        public Task<CalculationResult> CalculateAsync(CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task Execute_ReturnsCalculationResultAndForwardsCancellationToken()
    {
        var calculator = new FakeCalculator(new CalculationResult(1120, 8));
        using var cts = new CancellationTokenSource();

        var result = await new CalculateAnimalPaymentsUseCase(calculator).ExecuteAsync(cts.Token);

        Assert.Equal(new CalculationResult(1120, 8), result);
        Assert.Equal(cts.Token, calculator.ReceivedToken);
    }
}
