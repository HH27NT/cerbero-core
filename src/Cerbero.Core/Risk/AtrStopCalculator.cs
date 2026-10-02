namespace Cerbero.Core.Risk
{
    /// <summary>
    /// Places stops relative to current volatility instead of a fixed tick count,
    /// so the same setup doesn't get stopped out tighter on a loud day than a quiet one.
    /// </summary>
    public static class AtrStopCalculator
    {
        public static double SuggestStop(double entryPrice, double atr, double multiplier, bool isLong)
        {
            if (atr <= 0) throw new ArgumentOutOfRangeException(nameof(atr));
            if (multiplier <= 0) throw new ArgumentOutOfRangeException(nameof(multiplier));

            double distance = atr * multiplier;
            return isLong ? entryPrice - distance : entryPrice + distance;
        }

        /// <summary>
        /// Scales a base quantity so that dollar volatility stays roughly constant across
        /// regimes: shrink size when current ATR runs hot vs. its own average, grow it when
        /// quiet. Clamped to avoid absurd sizes when ATR briefly collapses near zero.
        /// </summary>
        public static double VolatilityAdjustment(double currentAtr, double averageAtr, double minAdjustment = 0.25, double maxAdjustment = 2.0)
        {
            if (currentAtr <= 0) throw new ArgumentOutOfRangeException(nameof(currentAtr));
            if (averageAtr <= 0) throw new ArgumentOutOfRangeException(nameof(averageAtr));

            double raw = averageAtr / currentAtr;
            return Math.Min(maxAdjustment, Math.Max(minAdjustment, raw));
        }
    }
}
