using System.Globalization;

namespace BikeStore.Web.Models;

/// <summary>
/// Formato de importes independiente de la configuracion regional del equipo.
/// Evita que el simbolo de moneda cambie segun el idioma de Windows.
/// </summary>
public static class Formato
{
    private static readonly NumberFormatInfo Moneda = new()
    {
        CurrencySymbol = "$",
        CurrencyDecimalDigits = 2,
        CurrencyDecimalSeparator = ".",
        CurrencyGroupSeparator = ",",
        CurrencyPositivePattern = 0,   // $1,234.56
        CurrencyNegativePattern = 1    // -$1,234.56
    };

    public static string Dinero(decimal valor) => valor.ToString("C", Moneda);
}
