namespace CaraDog.Core.Abstractions.Tax;

public interface ITaxCalculatorResolver
{
    ITaxCalculator Resolve(string countryCode);
}
