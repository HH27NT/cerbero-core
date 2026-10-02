# Cerbero Core — Risk & Trade Statistics

> **Educational software, not financial advice.** This code is published to show software design
> and testing practices. It is not a trading system and makes no claim about trading results.
> Use at your own risk.

Framework-agnostic C# library with the risk-management and performance-statistics core of
*Cerbero*, a personal trading-tools project. It has no dependency on any trading platform:
instrument facts (tick size, point value) are passed in as plain values, so all the math can
be unit-tested in isolation.

This repository contains only the risk and statistics modules. The rest of Cerbero
(indicators, market reading and order execution) is not published.

## What's inside

**`Cerbero.Core.Risk`**

| Type | Purpose |
|---|---|
| `PositionSizer` | Sizes a position backwards from risk: account-percent or fixed-dollar budget, entry and stop → quantity, ticks and dollars at risk, actual risk %. Also computes the target price for a given reward/risk multiple. |
| `InstrumentRiskProfile` | Tick size and point value of an instrument (presets for ES, MES, NQ, MNQ and 1 equity share). |
| `AtrStopCalculator` | ATR-based stop placement and a clamped volatility adjustment factor for position size. |
| `RiskGovernor` | Daily circuit breaker: blocks new entries after a max daily loss, max trades per day or max consecutive losses; resets automatically on a new day. |
| `ForexConversion` | Converts point value to account currency when the account currency is the pair's quote or base currency (returns `null` for crosses); rounds units to micro lots. |

**`Cerbero.Core.Stats`**

| Type | Purpose |
|---|---|
| `TradeStatistics` | Tracks trades as R-multiples (P&L ÷ initial risk) and reports count, win rate, average win/loss, expectancy, profit factor, standard deviation and max drawdown (in R). |

## Build and test

Requirements: .NET SDK 10 (the library targets `netstandard2.0` and `net48`; the tests run on
`net10.0` with xUnit).

```bash
dotnet build Cerbero.Core.slnx
dotnet test  Cerbero.Core.slnx
dotnet test  Cerbero.Core.slnx --collect:"XPlat Code Coverage"   # coverage (coverlet)
```

Latest local run: **28 tests passing**.

## Example

```csharp
using Cerbero.Core.Risk;

// $50,000 account, risking 1% on an ES long: entry 5000.00, stop 4996.00
var size = PositionSizer.FromAccountRiskPercent(50_000, 1.0, 5000.00, 4996.00, InstrumentRiskProfile.ES);
// size.Quantity == 2, size.TicksAtRisk == 16, size.DollarsAtRisk == 400

var governor = new RiskGovernor(maxDailyLoss: 500, maxTradesPerDay: 5, maxConsecutiveLosses: 3);
governor.RegisterTradeResult(-250, DateTime.Now);
bool canTrade = governor.IsTradingAllowed(DateTime.Now).Allowed;
```

## Project structure

```
src/Cerbero.Core/Risk/          position sizing, stops, daily risk limits, forex conversion
src/Cerbero.Core/Stats/         R-multiple trade statistics
src/Cerbero.Core.Tests/         xUnit tests
```

## License

**Copyright © 2026 Hector Hugo Naranjo. All rights reserved.**

No license is granted. The source is visible for evaluation and reference only; you may not
copy, modify, distribute or use it without written permission from the author.

---

## Resumen en español

Biblioteca en C#, independiente de cualquier plataforma de trading, con el núcleo de gestión de
riesgo (tamaño de posición, stops por ATR, límites diarios de pérdida/operaciones/rachas y
conversión forex) y estadísticas de operaciones en múltiplos R de *Cerbero*. Los indicadores, la
lectura de mercado y la ejecución no se publican. Compilar y probar con `dotnet test
Cerbero.Core.slnx` (28 pruebas). **Software educativo, no es asesoría financiera. Todos los
derechos reservados.**
