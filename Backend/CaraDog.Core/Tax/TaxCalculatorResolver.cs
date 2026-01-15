using CaraDog.Core.Abstractions.Tax;

namespace CaraDog.Core.Tax;

public sealed class TaxCalculatorResolver : ITaxCalculatorResolver
{
    private readonly Dictionary<string, ITaxCalculator> _calculators;

    public TaxCalculatorResolver(IEnumerable<ITaxCalculator> calculators)
    {
        _calculators = calculators.ToDictionary(
            calculator => calculator.CountryCode,
            calculator => calculator,
            StringComparer.OrdinalIgnoreCase);
    }

    public ITaxCalculator Resolve(string countryCode)
    {
        if (_calculators.TryGetValue(countryCode, out var calculator))
        {
            return calculator;
        }

        return _calculators.TryGetValue("AT", out var fallback) ? fallback : _calculators.Values.First();
    }
}
