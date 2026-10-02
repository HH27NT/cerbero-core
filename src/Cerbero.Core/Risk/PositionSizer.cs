namespace Cerbero.Core.Risk
{
    /// <summary>
    /// Sizes a position from the risk backwards: you fix how much of the account
    /// you're willing to lose, not how many contracts "feel right".
    /// </summary>
    public static class PositionSizer
    {
        public static PositionSizeResult FromAccountRiskPercent(
            double accountEquity,
            double riskPercent,
            double entryPrice,
            double stopPrice,
            InstrumentRiskProfile profile)
        {
            if (accountEquity <= 0) throw new ArgumentOutOfRangeException(nameof(accountEquity));
            if (riskPercent <= 0 || riskPercent > 100) throw new ArgumentOutOfRangeException(nameof(riskPercent));

            double dollarBudget = accountEquity * (riskPercent / 100.0);
            return FromDollarBudget(dollarBudget, entryPrice, stopPrice, profile, accountEquity);
        }

        public static PositionSizeResult FromDollarBudget(
            double dollarBudget,
            double entryPrice,
            double stopPrice,
            InstrumentRiskProfile profile,
            double? accountEquityForPercent = null)
        {
            if (dollarBudget <= 0) throw new ArgumentOutOfRangeException(nameof(dollarBudget));

            double priceDistance = Math.Abs(entryPrice - stopPrice);
            if (priceDistance <= 0) throw new ArgumentException("Entry and stop cannot be equal.");

            double ticksAtRisk = priceDistance / profile.TickSize;
            double dollarsPerUnit = ticksAtRisk * profile.DollarPerTick;

            int quantity = (int)Math.Floor(dollarBudget / dollarsPerUnit);
            double actualDollars = quantity * dollarsPerUnit;
            double actualPercent = accountEquityForPercent is double equity && equity > 0
                ? actualDollars / equity * 100.0
                : 0.0;

            return new PositionSizeResult(quantity, ticksAtRisk, actualDollars, actualPercent);
        }

        /// <summary>Price target implied by a desired reward multiple of the initial risk (R).</summary>
        public static double TargetForRiskReward(double entryPrice, double stopPrice, double riskRewardRatio)
        {
            if (riskRewardRatio <= 0) throw new ArgumentOutOfRangeException(nameof(riskRewardRatio));
            double risk = entryPrice - stopPrice; // signed: negative when short (stop above entry)
            return entryPrice + risk * riskRewardRatio;
        }
    }
}
