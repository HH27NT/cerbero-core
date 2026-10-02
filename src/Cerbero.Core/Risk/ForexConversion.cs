namespace Cerbero.Core.Risk
{
    /// <summary>
    /// Forex P&amp;L is earned in the pair's quote currency (EUR/USD pays in USD, USD/JPY in
    /// JPY), so a position sized in account currency needs the value of one price point per
    /// unit converted into that currency. Only the two cases that need no extra rate are
    /// handled: quote = account (value unchanged) and base = account (divide by the price).
    /// Crosses such as EUR/GBP for a USD account need a third rate and return null.
    /// </summary>
    public static class ForexConversion
    {
        /// <summary>Standard lot size in units of the base currency.</summary>
        public const int StandardLot = 100000;

        /// <summary>NinjaTrader forex orders go in multiples of a micro lot.</summary>
        public const int MicroLot = 1000;

        /// <param name="pointValueInQuote">Value of a one-point move per unit, in the quote currency (1 on NinjaTrader forex instruments).</param>
        /// <param name="price">Current or entry price, base expressed in quote.</param>
        public static double? PointValueInAccount(string pair, string accountCurrency, double pointValueInQuote, double price)
        {
            if (!TrySplitPair(pair, out string baseCurrency, out string quoteCurrency))
                return null;
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));

            string account = accountCurrency.Trim().ToUpperInvariant();
            if (quoteCurrency == account) return pointValueInQuote;
            if (baseCurrency == account) return pointValueInQuote / price;
            return null;
        }

        /// <summary>Rounds a unit count down to whole micro lots, the smallest tradable step.</summary>
        public static int RoundDownToMicroLots(int units) => units < 0 ? 0 : units / MicroLot * MicroLot;

        /// <summary>"EURUSD", "EUR/USD", "$EURUSD" or "EURUSD 12-26" → EUR, USD.</summary>
        public static bool TrySplitPair(string pair, out string baseCurrency, out string quoteCurrency)
        {
            baseCurrency = quoteCurrency = string.Empty;
            if (string.IsNullOrWhiteSpace(pair)) return false;

            string letters = new string(pair.Split(' ')[0].Where(char.IsLetter).ToArray()).ToUpperInvariant();
            if (letters.Length != 6) return false;

            baseCurrency = letters.Substring(0, 3);
            quoteCurrency = letters.Substring(3, 3);
            return true;
        }
    }
}
