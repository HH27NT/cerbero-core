using Cerbero.Core.Risk;
using Xunit;

namespace Cerbero.Core.Tests;

public class AtrStopCalculatorTests
{
    [Fact]
    public void SuggestStop_Long_SubtractsAtrMultiple()
    {
        double stop = AtrStopCalculator.SuggestStop(entryPrice: 5000.00, atr: 4.0, multiplier: 1.5, isLong: true);
        Assert.Equal(4994.00, stop, 3);
    }

    [Fact]
    public void SuggestStop_Short_AddsAtrMultiple()
    {
        double stop = AtrStopCalculator.SuggestStop(entryPrice: 5000.00, atr: 4.0, multiplier: 1.5, isLong: false);
        Assert.Equal(5006.00, stop, 3);
    }

    [Fact]
    public void VolatilityAdjustment_HighCurrentAtr_ShrinksSize()
    {
        // ATR actual el doble del promedio -> reduce tamaño a la mitad.
        double adjustment = AtrStopCalculator.VolatilityAdjustment(currentAtr: 8.0, averageAtr: 4.0);
        Assert.Equal(0.5, adjustment, 3);
    }

    [Fact]
    public void VolatilityAdjustment_ExtremeLowCurrentAtr_ClampsToMax()
    {
        double adjustment = AtrStopCalculator.VolatilityAdjustment(currentAtr: 0.1, averageAtr: 4.0, maxAdjustment: 2.0);
        Assert.Equal(2.0, adjustment, 3);
    }
}
