using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.Infrastructure.DataAccess.Payments;

/// <summary>Converte os códigos CHAR(1) gravados no banco para os enums do domínio, falhando em valor desconhecido.</summary>
internal static class DbCodes
{
    public static Sex ToSex(string? code) => code switch
    {
        "M" => Sex.Male,
        "F" => Sex.Female,
        _ => throw new InvalidDataException($"Código de sexo desconhecido no banco: '{code}'.")
    };

    public static AdjustmentType ToAdjustmentType(string? code) => code switch
    {
        "N" => AdjustmentType.None,
        "A" => AdjustmentType.Premium,
        "D" => AdjustmentType.Discount,
        _ => throw new InvalidDataException($"Código de ajuste desconhecido no banco: '{code}'.")
    };
}
