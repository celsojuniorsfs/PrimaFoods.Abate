using System.Globalization;

using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.Web.Extensions;

public static class DisplayExtensions
{
    public static string ToDisplay(this Sex sex) => sex switch
    {
        Sex.Male => "Macho",
        Sex.Female => "Fêmea",
        _ => sex.ToString()
    };

    public static string ToDisplay(this AdjustmentType type) => type switch
    {
        AdjustmentType.Premium => "Ágio",
        AdjustmentType.Discount => "Deságio",
        _ => "Sem ajuste"
    };

    public static string ToBadgeClass(this AdjustmentType type) => type switch
    {
        AdjustmentType.Premium => "text-bg-success",
        AdjustmentType.Discount => "text-bg-danger",
        _ => "text-bg-secondary"
    };

    public static string ToInvariant(this decimal value) => value.ToString(CultureInfo.InvariantCulture);
}
