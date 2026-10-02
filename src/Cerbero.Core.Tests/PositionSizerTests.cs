using Cerbero.Core.Risk;
using Xunit;

namespace Cerbero.Core.Tests;

public class PositionSizerTests
{
    [Fact]
    public void FromAccountRiskPercent_ES_ComputesExpectedContracts()
    {
        // Cuenta $50,000, arriesgar 1% = $500. Entrada 5000.00, stop 4996.00 -> 4 puntos = 16 ticks.
        // ES: $12.50/tick -> $200 de riesgo por contrato. 500 / 200 = 2.5 -> floor = 2 contratos.
        var result = PositionSizer.FromAccountRiskPercent(50_000, 1.0, 5000.00, 4996.00, InstrumentRiskProfile.ES);

        Assert.Equal(2, result.Quantity);
        Assert.Equal(16, result.TicksAtRisk, 3);
        Assert.Equal(400, result.DollarsAtRisk, 3);
        Assert.Equal(0.8, result.ActualRiskPercent, 3);
    }

    [Fact]
    public void FromAccountRiskPercent_Equity_ComputesExpectedShares()
    {
        // Cuenta $10,000, arriesgar 0.5% = $50. Entrada 100.00, stop 98.00 -> $2 de riesgo/acción.
        // 50 / 2 = 25 acciones.
        var result = PositionSizer.FromAccountRiskPercent(10_000, 0.5, 100.00, 98.00, InstrumentRiskProfile.Equity1Share);

        Assert.Equal(25, result.Quantity);
        Assert.Equal(50, result.DollarsAtRisk, 3);
    }

    [Fact]
    public void FromDollarBudget_WhenBudgetBelowOneUnit_ReturnsZeroQuantity()
    {
        var result = PositionSizer.FromDollarBudget(50, 5000.00, 4996.00, InstrumentRiskProfile.ES);

        Assert.Equal(0, result.Quantity);
        Assert.Equal(0, result.DollarsAtRisk, 3);
    }

    [Fact]
    public void TargetForRiskReward_Long_ProjectsAboveEntry()
    {
        double target = PositionSizer.TargetForRiskReward(entryPrice: 5000.00, stopPrice: 4996.00, riskRewardRatio: 2.0);

        Assert.Equal(5008.00, target, 3);
    }

    [Fact]
    public void TargetForRiskReward_Short_ProjectsBelowEntry()
    {
        double target = PositionSizer.TargetForRiskReward(entryPrice: 4996.00, stopPrice: 5000.00, riskRewardRatio: 2.0);

        Assert.Equal(4988.00, target, 3);
    }

    [Fact]
    public void FromAccountRiskPercent_EntryEqualsStop_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PositionSizer.FromAccountRiskPercent(10_000, 1.0, 100.00, 100.00, InstrumentRiskProfile.Equity1Share));
    }
}
