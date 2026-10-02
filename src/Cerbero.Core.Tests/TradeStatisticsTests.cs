using Cerbero.Core.Stats;
using Xunit;

namespace Cerbero.Core.Tests;

public class TradeStatisticsTests
{
    [Fact]
    public void Snapshot_MixedTrades_ComputesExpectedMetrics()
    {
        var stats = new TradeStatistics();
        // 3 ganadoras de 2R, 2 perdedoras de -1R -> winrate 60%, expectancy = (3*2 - 2*1)/5 = 0.8R
        stats.AddRMultiple(2);
        stats.AddRMultiple(2);
        stats.AddRMultiple(2);
        stats.AddRMultiple(-1);
        stats.AddRMultiple(-1);

        var snapshot = stats.Snapshot();

        Assert.Equal(5, snapshot.Count);
        Assert.Equal(0.6, snapshot.WinRate, 3);
        Assert.Equal(2.0, snapshot.AverageWinR, 3);
        Assert.Equal(-1.0, snapshot.AverageLossR, 3);
        Assert.Equal(0.8, snapshot.ExpectancyR, 3);
        Assert.Equal(3.0, snapshot.ProfitFactor, 3); // 6 ganado / 2 perdido
    }

    [Fact]
    public void Snapshot_NoLosses_ProfitFactorIsInfinite()
    {
        var stats = new TradeStatistics();
        stats.AddRMultiple(1.5);
        stats.AddRMultiple(2.0);

        Assert.True(double.IsPositiveInfinity(stats.Snapshot().ProfitFactor));
    }

    [Fact]
    public void Snapshot_Empty_ReturnsZeroedSnapshot()
    {
        var stats = new TradeStatistics();
        var snapshot = stats.Snapshot();

        Assert.Equal(0, snapshot.Count);
        Assert.Equal(0, snapshot.ExpectancyR);
    }

    [Fact]
    public void AddTrade_ConvertsDollarPnLToRMultiple()
    {
        var stats = new TradeStatistics();
        stats.AddTrade(realizedPnL: 400, dollarsRisked: 200); // +2R

        Assert.Equal(2.0, stats.Snapshot().ExpectancyR, 3);
    }

    [Fact]
    public void MaxDrawdownR_TracksWorstPeakToTroughDrop()
    {
        var stats = new TradeStatistics();
        // Equity curve en R: +3, +1(=4 pico), -5(=-1), +2(=1) -> drawdown máximo = 4 - (-1) = 5
        stats.AddRMultiple(3);
        stats.AddRMultiple(1);
        stats.AddRMultiple(-5);
        stats.AddRMultiple(2);

        Assert.Equal(5.0, stats.Snapshot().MaxDrawdownR, 3);
    }
}
