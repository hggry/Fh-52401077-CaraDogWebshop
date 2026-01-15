using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Tax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CaraDog.Unittests;

[TestClass]
public sealed class TaxCalculatorTests
{
    [TestMethod]
    public void TaxCalculatorAT_UsesTwentyPercent()
    {
        var calculator = new TaxCalculatorAT();
        var tax = calculator.CalculateTax(100m);

        Assert.AreEqual(20m, tax);
    }

    [TestMethod]
    public void TaxCalculatorDE_UsesNineteenPercent()
    {
        var calculator = new TaxCalculatorDE();
        var tax = calculator.CalculateTax(100m);

        Assert.AreEqual(19m, tax);
    }

    [TestMethod]
    public void Resolver_FallsBackToAT()
    {
        ITaxCalculator[] calculators = { new TaxCalculatorAT(), new TaxCalculatorDE() };
        var resolver = new TaxCalculatorResolver(calculators);

        var calculator = resolver.Resolve("FR");

        Assert.AreEqual("AT", calculator.CountryCode);
    }
}
