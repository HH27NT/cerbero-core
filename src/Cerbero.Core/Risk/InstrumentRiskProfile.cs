namespace Cerbero.Core.Risk
{
    /// <summary>
    /// Mirrors the pricing facts NinjaTrader exposes via Instrument.MasterInstrument
    /// (TickSize, PointValue) so risk math stays framework-agnostic and testable
    /// outside NinjaScript.
    /// </summary>
    public readonly struct InstrumentRiskProfile
    {
        public double TickSize { get; }
        public double PointValue { get; }

        public InstrumentRiskProfile(double tickSize, double pointValue)
        {
            if (tickSize <= 0) throw new ArgumentOutOfRangeException(nameof(tickSize));
            if (pointValue <= 0) throw new ArgumentOutOfRangeException(nameof(pointValue));
            TickSize = tickSize;
            PointValue = pointValue;
        }

        public double DollarPerTick => TickSize * PointValue;

        public static InstrumentRiskProfile ES => new InstrumentRiskProfile(0.25, 50.0);
        public static InstrumentRiskProfile MES => new InstrumentRiskProfile(0.25, 5.0);
        public static InstrumentRiskProfile NQ => new InstrumentRiskProfile(0.25, 20.0);
        public static InstrumentRiskProfile MNQ => new InstrumentRiskProfile(0.25, 2.0);
        public static InstrumentRiskProfile Equity1Share => new InstrumentRiskProfile(0.01, 1.0);
    }
}
