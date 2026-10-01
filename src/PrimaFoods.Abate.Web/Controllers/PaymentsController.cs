using Microsoft.AspNetCore.Mvc;

using PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;
using PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

namespace PrimaFoods.Abate.Web.Controllers;

public sealed class PaymentsController : Controller
{
    [HttpGet]
    public async Task<IActionResult> index(
        [FromServices] IGetPaymentDashboardUseCase useCase,
        CancellationToken cancellationToken)
    {
        {
            var result = await useCase.ExecuteAsync(cancellationToken);
            
            return View(result);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(
        [FromServices] ICalculateAnimalPaymentsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var processedAnimals = await useCase.ExecuteAsync(cancellationToken);

        TempData["SuccessMessage"] = $"Cálculo concluído: {processedAnimals:N0} animais processados.";

        return RedirectToAction(nameof(index));
    }
}
