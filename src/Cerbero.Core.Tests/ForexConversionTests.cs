using Cerbero.Core.Risk;

namespace Cerbero.Core.Tests;

public class ForexConversionTests
{
    [Theory]
    [InlineData("$EURUSD", 1.0)]
    [InlineData("EUR/USD", 1.0)]
    public void QuoteIsAccount_ValueUnchanged(string pair, double expected)
    {
        Assert.Equal(expected, ForexConversion.PointValueInAccount(pair, "USD", 1.0, 1.17));
    }

    [Fact]
    public void BaseIsAccount_DividesByPrice()
    {
        // USD/JPY a 150: un yen por unidad vale 1/150 USD.
        Assert.Equal(1.0 / 150, ForexConversion.PointValueInAccount("$USDJPY", "USD", 1.0, 150)!.Value, 12);
    }

    [Fact]
    public void Cross_ReturnsNull()
    {
        Assert.Null(ForexConversion.PointValueInAccount("$EURGBP", "USD", 1.0, 0.86));
    }

    [Fact]
    public void SizingEurUsd_RiskMatchesBudget()
    {
        // 50 pips de stop (0.0050), riesgo $500 → 100,000 unidades (1 lote estándar).
        var profile = new InstrumentRiskProfile(0.00001, 1.0);
        var result = PositionSizer.FromDollarBudget(500, 1.17000, 1.16500, profile);
        Assert.Equal(100000, ForexConversion.RoundDownToMicroLots(result.Quantity));
    }

    [Theory]
    [InlineData(123456, 123000)]
    [InlineData(999, 0)]
    public void RoundDownToMicroLots(int units, int expected)
    {
        Assert.Equal(expected, ForexConversion.RoundDownToMicroLots(units));
    }
}
