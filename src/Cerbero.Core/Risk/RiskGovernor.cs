namespace Cerbero.Core.Risk
{
    public readonly struct TradingAllowedResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public TradingAllowedResult(bool allowed, string reason)
        {
            Allowed = allowed;
            Reason = reason;
        }

        public static readonly TradingAllowedResult Ok = new TradingAllowedResult(true, string.Empty);
    }

    /// <summary>
    /// Cerbero's gatekeeper: blocks new entries once the day's losses or trade count
    /// breach limits set in advance, when the account can least be trusted to set them.
    /// Resets automatically on the first trade registered on a new calendar day.
    /// </summary>
    public sealed class RiskGovernor
    {
        private readonly double _maxDailyLoss;
        private readonly int? _maxTradesPerDay;
        private readonly int? _maxConsecutiveLosses;

        private DateTime _currentDay;
        private double _dailyRealizedPnL;
        private int _dailyTradeCount;
        private int _consecutiveLosses;

        public RiskGovernor(double maxDailyLoss, int? maxTradesPerDay = null, int? maxConsecutiveLosses = null)
        {
            if (maxDailyLoss <= 0) throw new ArgumentOutOfRangeException(nameof(maxDailyLoss));
            _maxDailyLoss = maxDailyLoss;
            _maxTradesPerDay = maxTradesPerDay;
            _maxConsecutiveLosses = maxConsecutiveLosses;
            _currentDay = DateTime.MinValue;
        }

        public double DailyRealizedPnL => _dailyRealizedPnL;
        public int DailyTradeCount => _dailyTradeCount;
        public int ConsecutiveLosses => _consecutiveLosses;

        public void RegisterTradeResult(double realizedPnL, DateTime closeTime)
        {
            RollDayIfNeeded(closeTime.Date);

            _dailyRealizedPnL += realizedPnL;
            _dailyTradeCount++;
            _consecutiveLosses = realizedPnL < 0 ? _consecutiveLosses + 1 : 0;
        }

        public TradingAllowedResult IsTradingAllowed(DateTime asOf)
        {
            RollDayIfNeeded(asOf.Date);

            if (_dailyRealizedPnL <= -_maxDailyLoss)
                return new TradingAllowedResult(false, $"Límite de pérdida diaria alcanzado ({_dailyRealizedPnL:C} / -{_maxDailyLoss:C}).");

            if (_maxTradesPerDay is int maxTrades && _dailyTradeCount >= maxTrades)
                return new TradingAllowedResult(false, $"Límite de operaciones diarias alcanzado ({_dailyTradeCount}/{maxTrades}).");

            if (_maxConsecutiveLosses is int maxLosses && _consecutiveLosses >= maxLosses)
                return new TradingAllowedResult(false, $"Racha de pérdidas consecutivas alcanzada ({_consecutiveLosses}/{maxLosses}).");

            return TradingAllowedResult.Ok;
        }

        public void Reset(DateTime day)
        {
            _currentDay = day.Date;
            _dailyRealizedPnL = 0;
            _dailyTradeCount = 0;
            _consecutiveLosses = 0;
        }

        private void RollDayIfNeeded(DateTime day)
        {
            if (day != _currentDay)
                Reset(day);
        }
    }
}
