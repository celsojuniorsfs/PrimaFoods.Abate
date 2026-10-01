using Microsoft.AspNetCore.Mvc;

using PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;
using PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

namespace PrimaFoods.Abate.Web.Controllers;

public sealed class PaymentsController(ILogger<PaymentsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromServices] IGetPaymentDashboardUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);

        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(
        [FromServices] ICalculateAnimalPaymentsUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await useCase.ExecuteAsync(cancellationToken);

            TempData["SuccessMessage"] = $"Cálculo concluído: {result.ProcessedAnimals:N0} animais processados.";

            if (result.SkippedAnimals > 0)
            {
                TempData["WarningMessage"] =
                    $"{result.SkippedAnimals:N0} animais foram ignorados por dados inválidos (sexo, peso, dentes ou pedido).";
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Falha ao calcular o pagamento dos animais.");

            TempData["ErrorMessage"] = "Não foi possível calcular os pagamentos. Tente novamente ou contate o suporte.";
        }

        return RedirectToAction(nameof(Index));
    }
}
