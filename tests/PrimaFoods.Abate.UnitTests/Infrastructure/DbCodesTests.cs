using PrimaFoods.Abate.Domain.Enums;
using PrimaFoods.Abate.Infrastructure.DataAccess.Payments;

namespace PrimaFoods.Abate.UnitTests.Infrastructure;

public class DbCodesTests
{
    [Theory]
    [InlineData("M", Sex.Male)]
    [InlineData("F", Sex.Female)]
    public void ToSex_MapsKnownCodes(string code, Sex expected)
        => Assert.Equal(expected, DbCodes.ToSex(code));

    [Theory]
    [InlineData("N", AdjustmentType.None)]
    [InlineData("A", AdjustmentType.Premium)]
    [InlineData("D", AdjustmentType.Discount)]
    public void ToAdjustmentType_MapsKnownCodes(string code, AdjustmentType expected)
        => Assert.Equal(expected, DbCodes.ToAdjustmentType(code));

    [Theory]
    [InlineData("X")]
    [InlineData("m")]
    [InlineData("")]
    [InlineData(null)]
    public void ToSex_ThrowsOnUnknownCode(string? code)
        => Assert.Throws<InvalidDataException>(() => DbCodes.ToSex(code));

    [Theory]
    [InlineData("X")]
    [InlineData("a")]
    [InlineData("")]
    [InlineData(null)]
    public void ToAdjustmentType_ThrowsOnUnknownCode(string? code)
        => Assert.Throws<InvalidDataException>(() => DbCodes.ToAdjustmentType(code));
}
