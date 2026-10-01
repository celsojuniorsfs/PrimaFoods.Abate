namespace PrimaFoods.Abate.Application.Common;

internal static class Calculations
{
    public static decimal Percentage(int part, int total)
    => total == 0 ? 0 : Math.Round(part * 100m / total, 2, MidpointRounding.AwayFromZero);

    public static decimal Divide(decimal dividend, decimal divisor)
        => divisor == 0 ? 0 : Math.Round(dividend / divisor, 2, MidpointRounding.AwayFromZero);
}
