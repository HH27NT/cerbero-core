using Cerbero.Core.Risk;
using Xunit;

namespace Cerbero.Core.Tests;

public class RiskGovernorTests
{
    private static readonly DateTime Day1_0900 = new DateTime(2026, 8, 17, 9, 0, 0);
    private static readonly DateTime Day1_1000 = new DateTime(2026, 8, 17, 10, 0, 0);
    private static readonly DateTime Day2_0900 = new DateTime(2026, 8, 18, 9, 0, 0);

    [Fact]
    public void IsTradingAllowed_BeforeAnyLoss_ReturnsOk()
    {
        var governor = new RiskGovernor(maxDailyLoss: 1000);
        Assert.True(governor.IsTradingAllowed(Day1_0900).Allowed);
    }

    [Fact]
    public void IsTradingAllowed_AfterBreachingDailyLoss_Blocks()
    {
        var governor = new RiskGovernor(maxDailyLoss: 1000);
        governor.RegisterTradeResult(-600, Day1_0900);
        governor.RegisterTradeResult(-500, Day1_1000);

        var result = governor.IsTradingAllowed(Day1_1000);

        Assert.False(result.Allowed);
        Assert.Contains("pérdida diaria", result.Reason);
    }

    [Fact]
    public void IsTradingAllowed_AfterMaxTradesPerDay_Blocks()
    {
        var governor = new RiskGovernor(maxDailyLoss: 10_000, maxTradesPerDay: 2);
        governor.RegisterTradeResult(50, Day1_0900);
        governor.RegisterTradeResult(30, Day1_1000);

        Assert.False(governor.IsTradingAllowed(Day1_1000).Allowed);
    }

    [Fact]
    public void IsTradingAllowed_AfterMaxConsecutiveLosses_Blocks()
    {
        var governor = new RiskGovernor(maxDailyLoss: 10_000, maxConsecutiveLosses: 3);
        governor.RegisterTradeResult(-50, Day1_0900);
        governor.RegisterTradeResult(-50, Day1_0900);
        governor.RegisterTradeResult(-50, Day1_0900);

        Assert.False(governor.IsTradingAllowed(Day1_0900).Allowed);
    }

    [Fact]
    public void ConsecutiveLosses_ResetByAWin()
    {
        var governor = new RiskGovernor(maxDailyLoss: 10_000, maxConsecutiveLosses: 3);
        governor.RegisterTradeResult(-50, Day1_0900);
        governor.RegisterTradeResult(-50, Day1_0900);
        governor.RegisterTradeResult(100, Day1_0900);

        Assert.Equal(0, governor.ConsecutiveLosses);
        Assert.True(governor.IsTradingAllowed(Day1_0900).Allowed);
    }

    [Fact]
    public void NewCalendarDay_ResetsDailyState()
    {
        var governor = new RiskGovernor(maxDailyLoss: 1000);
        governor.RegisterTradeResult(-1000, Day1_0900);
        Assert.False(governor.IsTradingAllowed(Day1_1000).Allowed);

        Assert.True(governor.IsTradingAllowed(Day2_0900).Allowed);
        Assert.Equal(0, governor.DailyTradeCount);
    }
}
