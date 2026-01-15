namespace CaraDog.Core.Abstractions.Tax;

public interface ITaxCalculator
{
    string CountryCode { get; }
    decimal Rate { get; }
    decimal CalculateTax(decimal netAmount);
}
