namespace Cerbero.Core.Stats
{
    public readonly struct TradeStatisticsSnapshot
    {
        public int Count { get; }
        public double WinRate { get; }
        public double AverageWinR { get; }
        public double AverageLossR { get; }
        public double ExpectancyR { get; }
        public double ProfitFactor { get; }
        public double StdDevR { get; }
        public double MaxDrawdownR { get; }

        public TradeStatisticsSnapshot(int count, double winRate, double averageWinR, double averageLossR,
            double expectancyR, double profitFactor, double stdDevR, double maxDrawdownR)
        {
            Count = count;
            WinRate = winRate;
            AverageWinR = averageWinR;
            AverageLossR = averageLossR;
            ExpectancyR = expectancyR;
            ProfitFactor = profitFactor;
            StdDevR = stdDevR;
            MaxDrawdownR = maxDrawdownR;
        }
    }

    /// <summary>
    /// Tracks trade outcomes in R-multiples (P&amp;L ÷ initial risk, per Van Tharp) so
    /// performance stays comparable across instruments, position sizes and account size.
    /// </summary>
    public sealed class TradeStatistics
    {
        private readonly List<double> _rMultiples = new List<double>();

        public void AddTrade(double realizedPnL, double dollarsRisked)
        {
            if (dollarsRisked <= 0) throw new ArgumentOutOfRangeException(nameof(dollarsRisked));
            _rMultiples.Add(realizedPnL / dollarsRisked);
        }

        public void AddRMultiple(double rMultiple) => _rMultiples.Add(rMultiple);

        public TradeStatisticsSnapshot Snapshot()
        {
            int count = _rMultiples.Count;
            if (count == 0)
                return new TradeStatisticsSnapshot(0, 0, 0, 0, 0, 0, 0, 0);

            var wins = _rMultiples.Where(r => r > 0).ToList();
            var losses = _rMultiples.Where(r => r < 0).ToList();

            double winRate = (double)wins.Count / count;
            double avgWin = wins.Count > 0 ? wins.Average() : 0.0;
            double avgLoss = losses.Count > 0 ? losses.Average() : 0.0;
            double expectancy = _rMultiples.Average();

            double grossWin = wins.Sum();
            double grossLoss = Math.Abs(losses.Sum());
            double profitFactor = grossLoss > 0 ? grossWin / grossLoss : double.PositiveInfinity;

            double mean = expectancy;
            double variance = count > 1 ? _rMultiples.Sum(r => (r - mean) * (r - mean)) / (count - 1) : 0.0;
            double stdDev = Math.Sqrt(variance);

            double maxDrawdown = ComputeMaxDrawdownR();

            return new TradeStatisticsSnapshot(count, winRate, avgWin, avgLoss, expectancy, profitFactor, stdDev, maxDrawdown);
        }

        private double ComputeMaxDrawdownR()
        {
            double equity = 0, peak = 0, maxDrawdown = 0;
            foreach (double r in _rMultiples)
            {
                equity += r;
                peak = Math.Max(peak, equity);
                maxDrawdown = Math.Max(maxDrawdown, peak - equity);
            }
            return maxDrawdown;
        }
    }
}
