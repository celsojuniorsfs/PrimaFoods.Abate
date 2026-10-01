namespace PrimaFoods.Abate.Application.Models;

/// <param name="ProcessedAnimals">Animais que tiveram o pagamento calculado.</param>
/// <param name="SkippedAnimals">Animais ignorados por dados inválidos (sexo fora de M/F, ou sem peso, dentes ou pedido).</param>
public sealed record CalculationResult(int ProcessedAnimals, int SkippedAnimals);
