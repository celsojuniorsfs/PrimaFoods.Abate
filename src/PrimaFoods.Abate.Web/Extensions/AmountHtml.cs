using Microsoft.AspNetCore.Html;

namespace PrimaFoods.Abate.Web.Extensions;

public static class AmountHtml
{
    public static IHtmlContent Premium(decimal value) => Signed(value, "+", "text-success");

    public static IHtmlContent Discount(decimal value) => Signed(value, "-", "text-danger");

    private static IHtmlContent Signed(decimal value, string sign, string cssClass)
    {
        if (value == 0)
            return new HtmlString($"<span class=\"text-body-secondary\">{value:C}</span>");

        return new HtmlString($"<span class=\"{cssClass} fw-semibold\">{sign} {value:C}</span>");
    }
}
