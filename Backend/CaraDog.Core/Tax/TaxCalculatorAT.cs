using CaraDog.Core.Abstractions.Tax;

namespace CaraDog.Core.Tax;

public sealed class TaxCalculatorAT : ITaxCalculator
{
    public string CountryCode => "AT";
    public decimal Rate => 0.20m;

    public decimal CalculateTax(decimal netAmount)
    {
        return Math.Round(netAmount * Rate, 2, MidpointRounding.AwayFromZero);
    }
}
