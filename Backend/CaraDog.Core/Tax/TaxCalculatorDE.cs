using CaraDog.Core.Abstractions.Tax;

namespace CaraDog.Core.Tax;

public sealed class TaxCalculatorDE : ITaxCalculator
{
    public string CountryCode => "DE";
    public decimal Rate => 0.19m;

    public decimal CalculateTax(decimal netAmount)
    {
        return Math.Round(netAmount * Rate, 2, MidpointRounding.AwayFromZero);
    }
}
