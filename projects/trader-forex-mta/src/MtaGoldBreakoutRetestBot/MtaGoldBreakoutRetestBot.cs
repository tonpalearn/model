using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MtaGoldBreakoutRetestBot : Robot
    {
        private const string BotLabel = "MTA_GOLD_BRC_v02";

        private AverageTrueRange _atrM15;
        private Bars _biasBars;
        private BreakoutSetup _activeSetup;
        private PositionSnapshot _activeTrade;
        private DateTime _currentTradeDay;
        private int _tradesToday;
        private int _lossesToday;
        private double _closedNetToday;
        private double _startOfDayEquity;
        private long _setupSequence;
        private long _lastBiasLogH1BarIndex = -1;
        private readonly Dictionary<string, int> _rangeAttempts = new Dictionary<string, int>();

        [Parameter("Bot Label", DefaultValue = BotLabel)]
        public string InstanceLabel { get; set; }

        [Parameter("Symbol Name", DefaultValue = "XAUUSD")]
        public string TargetSymbolName { get; set; }

        [Parameter("Bias TimeFrame", DefaultValue = "Hour")]
        public TimeFrame BiasTimeFrame { get; set; }

        [Parameter("Range Lookback M15", DefaultValue = 8, MinValue = 3)]
        public int RangeLookbackM15 { get; set; }

        [Parameter("Bias Lookback H1", DefaultValue = 20, MinValue = 10)]
        public int BiasLookbackH1 { get; set; }

        [Parameter("Swing Strength", DefaultValue = 2, MinValue = 1, MaxValue = 5)]
        public int SwingStrength { get; set; }

        [Parameter("ATR Period M15", DefaultValue = 14, MinValue = 5)]
        public int AtrPeriodM15 { get; set; }

        [Parameter("Break Buffer ATR", DefaultValue = 0.10, MinValue = 0.01, Step = 0.01)]
        public double BreakBufferAtr { get; set; }

        [Parameter("Fixed Min Break Buffer", DefaultValue = 0.30, MinValue = 0.0, Step = 0.01)]
        public double FixedMinBreakBuffer { get; set; }

        [Parameter("Retest Zone ATR", DefaultValue = 0.30, MinValue = 0.01, Step = 0.01)]
        public double RetestZoneAtr { get; set; }

        [Parameter("Retest Timeout Bars", DefaultValue = 6, MinValue = 1, MaxValue = 12)]
        public int RetestTimeoutBars { get; set; }

        [Parameter("Confirm Body Min", DefaultValue = 0.35, MinValue = 0.10, MaxValue = 0.95, Step = 0.05)]
        public double ConfirmationBodyMin { get; set; }

        [Parameter("Confirm Close Percent", DefaultValue = 0.45, MinValue = 0.05, MaxValue = 0.50, Step = 0.05)]
        public double ConfirmationClosePercent { get; set; }

        [Parameter("Allow Same-Bar Retest Confirm", DefaultValue = true)]
        public bool AllowSameBarRetestConfirmation { get; set; }

        [Parameter("Max Breakout Candle ATR", DefaultValue = 2.5, MinValue = 0.5, Step = 0.1)]
        public double MaxBreakoutCandleAtr { get; set; }

        [Parameter("SL Buffer ATR", DefaultValue = 0.08, MinValue = 0.01, Step = 0.01)]
        public double SlBufferAtr { get; set; }

        [Parameter("Fixed Min SL Buffer", DefaultValue = 0.20, MinValue = 0.0, Step = 0.01)]
        public double FixedMinSlBuffer { get; set; }

        [Parameter("Min Stop Distance", DefaultValue = 1.5, MinValue = 0.1, Step = 0.1)]
        public double MinStopDistance { get; set; }

        [Parameter("Max Stop Distance", DefaultValue = 20.0, MinValue = 1.0, Step = 0.1)]
        public double MaxStopDistance { get; set; }

        [Parameter("Risk %", DefaultValue = 0.25, MinValue = 0.01, MaxValue = 5.0, Step = 0.01)]
        public double RiskPercent { get; set; }

        [Parameter("Max Trades / Day", DefaultValue = 2, MinValue = 1, MaxValue = 10)]
        public int MaxTradesPerDay { get; set; }

        [Parameter("Max Losses / Day", DefaultValue = 2, MinValue = 1, MaxValue = 10)]
        public int MaxLossesPerDay { get; set; }

        [Parameter("Daily Loss Cap %", DefaultValue = 1.0, MinValue = 0.1, MaxValue = 10.0, Step = 0.1)]
        public double DailyLossCapPercent { get; set; }

        [Parameter("Break Even At R", DefaultValue = 1.0, MinValue = 0.1, Step = 0.1)]
        public double BreakEvenAtR { get; set; }

        [Parameter("Break Even Offset", DefaultValue = 0.10, MinValue = 0.0, Step = 0.01)]
        public double BreakEvenOffset { get; set; }

        [Parameter("Take Profit R", DefaultValue = 2.0, MinValue = 0.5, Step = 0.1)]
        public double TakeProfitR { get; set; }

        [Parameter("Max Bars In Trade", DefaultValue = 8, MinValue = 1, MaxValue = 48)]
        public int MaxBarsInTrade { get; set; }

        [Parameter("Min ATR M15", DefaultValue = 1.00, MinValue = 0.0, Step = 0.1)]
        public double MinAtrM15 { get; set; }

        [Parameter("Max ATR M15", DefaultValue = 20.0, MinValue = 0.0, Step = 0.1)]
        public double MaxAtrM15 { get; set; }

        [Parameter("Max Spread", DefaultValue = 2.50, MinValue = 0.0, Step = 0.01)]
        public double MaxSpread { get; set; }

        [Parameter("London Start Hour", DefaultValue = 7, MinValue = 0, MaxValue = 23)]
        public int LondonStartHour { get; set; }

        [Parameter("London End Hour", DefaultValue = 11, MinValue = 0, MaxValue = 24)]
        public int LondonEndHour { get; set; }

        [Parameter("New York Start Hour", DefaultValue = 12, MinValue = 0, MaxValue = 23)]
        public int NewYorkStartHour { get; set; }

        [Parameter("New York End Hour", DefaultValue = 17, MinValue = 0, MaxValue = 24)]
        public int NewYorkEndHour { get; set; }

        [Parameter("Friday Cutoff Hour", DefaultValue = 17, MinValue = 0, MaxValue = 23)]
        public int FridayCutoffHour { get; set; }

        [Parameter("Trading Day Offset Hours", DefaultValue = 0, MinValue = -12, MaxValue = 14)]
        public int TradingDayOffsetHours { get; set; }

        [Parameter("Allow London Session", DefaultValue = true)]
        public bool AllowLondonSession { get; set; }

        [Parameter("Allow New York Session", DefaultValue = true)]
        public bool AllowNewYorkSession { get; set; }

        [Parameter("Max Attempts Per Range", DefaultValue = 1, MinValue = 1, MaxValue = 3)]
        public int MaxAttemptsPerRange { get; set; }

        [Parameter("Verbose Logging", DefaultValue = true)]
        public bool VerboseLogging { get; set; }

        protected override void OnStart()
        {
            if (!string.Equals(SymbolName, TargetSymbolName, StringComparison.OrdinalIgnoreCase))
                Print("Warning: attached symbol={0}, parameter symbol={1}", SymbolName, TargetSymbolName);

            _atrM15 = Indicators.AverageTrueRange(Bars, AtrPeriodM15, MovingAverageType.Exponential);
            _biasBars = MarketData.GetBars(BiasTimeFrame, SymbolName);
            _currentTradeDay = GetTradingDay(Server.Time);
            _startOfDayEquity = Account.Equity;

            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;

            Print("{0} started | symbol={1} | serverTime={2:yyyy-MM-dd HH:mm:ss} UTC | tradingDayOffset={3}", InstanceLabel, SymbolName, Server.Time, TradingDayOffsetHours);
            Print("SYMBOL SPEC | pipSize={0} pipValue={1} tickSize={2} lotSize={3} volMin={4} volMax={5} volStep={6}",
                Symbol.PipSize,
                Symbol.PipValue,
                Symbol.TickSize,
                Symbol.LotSize,
                Symbol.VolumeInUnitsMin,
                Symbol.VolumeInUnitsMax,
                Symbol.VolumeInUnitsStep);
        }

        protected override void OnBar()
        {
            ResetDailyCountersIfNeeded();
            RefreshActiveTradeSnapshot();

            if (Bars.Count < Math.Max(RangeLookbackM15 + 5, AtrPeriodM15 + 5))
                return;

            if (_biasBars == null || _biasBars.Count < BiasLookbackH1 + (SwingStrength * 2) + 10)
                return;

            if (_activeTrade != null)
            {
                ManageOpenTrade();
                return;
            }

            AdvanceOrInvalidateSetup();
            TryCreateOrConfirmSetup();
        }

        private void TryCreateOrConfirmSetup()
        {
            var lastClosedIndex = Bars.Count - 2;
            var barTime = Bars.OpenTimes[lastClosedIndex];
            var atr = _atrM15.Result[lastClosedIndex];

            if (!IsAtrTradable(atr))
            {
                InvalidateSetup("ATR out of allowed band");
                return;
            }

            if (!IsEntryWindow(barTime))
            {
                InvalidateSetup("Outside session window");
                return;
            }

            if (IsSpreadTooHigh())
            {
                InvalidateSetup("Spread too high");
                return;
            }

            BiasSnapshot bias = CalculateH1Bias();
            LogBiasSnapshotIfNeeded(bias);
            if (bias.Bias == TradeBias.Neutral)
            {
                InvalidateSetup("Neutral H1 bias");
                return;
            }

            string guardReason;
            if (!CanOpenNewTrade(out guardReason))
            {
                InvalidateSetup(guardReason);
                return;
            }

            if (_activeSetup == null)
            {
                TryArmBreakout(lastClosedIndex, bias, atr);
                return;
            }

            if (_activeSetup.Bias != bias.Bias)
            {
                InvalidateSetup("Bias changed before entry");
                return;
            }

            if (_activeSetup.State == SetupState.BreakoutArmed || _activeSetup.State == SetupState.RetestObserved)
                TryProcessRetestAndConfirmation(lastClosedIndex, atr);
        }

        private void AdvanceOrInvalidateSetup()
        {
            if (_activeSetup == null)
                return;

            int lastClosedIndex = Bars.Count - 2;
            if (lastClosedIndex <= _activeSetup.BreakoutBarIndex)
                return;

            int barsSinceBreak = lastClosedIndex - _activeSetup.BreakoutBarIndex;
            if (barsSinceBreak > RetestTimeoutBars)
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup("Retest timeout");
                return;
            }

            double close = Bars.ClosePrices[lastClosedIndex];
            if (_activeSetup.Direction == TradeType.Buy && close < _activeSetup.RangeLow)
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup("Price closed below opposite side of range");
                return;
            }

            if (_activeSetup.Direction == TradeType.Sell && close > _activeSetup.RangeHigh)
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup("Price closed above opposite side of range");
                return;
            }
        }

        private void TryArmBreakout(int lastClosedIndex, BiasSnapshot bias, double atr)
        {
            int rangeStart = lastClosedIndex - RangeLookbackM15;
            if (rangeStart < 0)
                return;

            double rangeHigh = double.MinValue;
            double rangeLow = double.MaxValue;
            for (int i = rangeStart; i <= lastClosedIndex - 1; i++)
            {
                rangeHigh = Math.Max(rangeHigh, Bars.HighPrices[i]);
                rangeLow = Math.Min(rangeLow, Bars.LowPrices[i]);
            }

            double candleRange = Bars.HighPrices[lastClosedIndex] - Bars.LowPrices[lastClosedIndex];
            if (candleRange > atr * MaxBreakoutCandleAtr)
            {
                LogReject("Breakout candle too large", lastClosedIndex);
                return;
            }

            double breakBuffer = Math.Max(atr * BreakBufferAtr, FixedMinBreakBuffer);
            double close = Bars.ClosePrices[lastClosedIndex];
            string rangeKeyBuy = BuildRangeKey(TradeType.Buy, rangeHigh, rangeLow, Bars.OpenTimes[lastClosedIndex]);
            string rangeKeySell = BuildRangeKey(TradeType.Sell, rangeHigh, rangeLow, Bars.OpenTimes[lastClosedIndex]);

            if (bias.Bias == TradeBias.Bullish && close > rangeHigh + breakBuffer)
            {
                if (GetRangeAttempts(rangeKeyBuy) >= MaxAttemptsPerRange)
                {
                    LogReject("Range attempt limit reached (buy)", lastClosedIndex);
                    return;
                }

                ArmSetup(TradeType.Buy, bias.Bias, rangeHigh, rangeLow, lastClosedIndex, atr, rangeKeyBuy);
                return;
            }

            if (bias.Bias == TradeBias.Bearish && close < rangeLow - breakBuffer)
            {
                if (GetRangeAttempts(rangeKeySell) >= MaxAttemptsPerRange)
                {
                    LogReject("Range attempt limit reached (sell)", lastClosedIndex);
                    return;
                }

                ArmSetup(TradeType.Sell, bias.Bias, rangeHigh, rangeLow, lastClosedIndex, atr, rangeKeySell);
            }
        }

        private void ArmSetup(TradeType direction, TradeBias bias, double rangeHigh, double rangeLow, int breakoutBarIndex, double atr, string rangeKey)
        {
            _setupSequence++;
            double breakoutLevel = direction == TradeType.Buy ? rangeHigh : rangeLow;
            double zoneWidth = Math.Max(Symbol.PipSize, atr * RetestZoneAtr);

            _activeSetup = new BreakoutSetup
            {
                Id = _setupSequence,
                Bias = bias,
                Direction = direction,
                RangeHigh = rangeHigh,
                RangeLow = rangeLow,
                BreakoutLevel = breakoutLevel,
                BreakoutBarIndex = breakoutBarIndex,
                BreakoutTime = Bars.OpenTimes[breakoutBarIndex],
                RetestZoneLow = breakoutLevel - zoneWidth,
                RetestZoneHigh = breakoutLevel + zoneWidth,
                LowestRetestPrice = double.MaxValue,
                HighestRetestPrice = double.MinValue,
                State = SetupState.BreakoutArmed,
                RangeKey = rangeKey,
                RetestSeenBarIndex = -1
            };

            Print("ARM {0} #{1} | breakout={2:F2} range=[{3:F2},{4:F2}] zone=[{5:F2},{6:F2}] atr={7:F2}",
                direction,
                _activeSetup.Id,
                breakoutLevel,
                rangeLow,
                rangeHigh,
                _activeSetup.RetestZoneLow,
                _activeSetup.RetestZoneHigh,
                atr);
        }

        private void TryProcessRetestAndConfirmation(int lastClosedIndex, double atr)
        {
            if (_activeSetup == null)
                return;

            double low = Bars.LowPrices[lastClosedIndex];
            double high = Bars.HighPrices[lastClosedIndex];
            double close = Bars.ClosePrices[lastClosedIndex];
            double open = Bars.OpenPrices[lastClosedIndex];
            bool touchedZone = high >= _activeSetup.RetestZoneLow && low <= _activeSetup.RetestZoneHigh;

            if (touchedZone)
            {
                _activeSetup.State = SetupState.RetestObserved;
                _activeSetup.LowestRetestPrice = Math.Min(_activeSetup.LowestRetestPrice, low);
                _activeSetup.HighestRetestPrice = Math.Max(_activeSetup.HighestRetestPrice, high);
                if (_activeSetup.RetestSeenBarIndex < 0)
                {
                    _activeSetup.RetestSeenBarIndex = lastClosedIndex;
                    Print("RETEST #{0} | dir={1} low={2:F2} high={3:F2}", _activeSetup.Id, _activeSetup.Direction, low, high);
                }
            }

            if (_activeSetup.State != SetupState.RetestObserved)
                return;

            if (!AllowSameBarRetestConfirmation && _activeSetup.RetestSeenBarIndex == lastClosedIndex)
            {
                LogVerbose("Waiting next bar for confirmation because same-bar confirmation disabled");
                return;
            }

            double barRange = high - low;
            if (barRange <= Symbol.PipSize)
            {
                LogVerbose("Confirmation skipped: bar range too small");
                return;
            }

            double body = Math.Abs(close - open);
            double bodyRatio = body / barRange;
            if (bodyRatio < ConfirmationBodyMin)
            {
                LogVerbose(string.Format("Confirmation skipped: body ratio {0:F2} < min {1:F2}", bodyRatio, ConfirmationBodyMin));
                return;
            }

            bool buyConfirmed = _activeSetup.Direction == TradeType.Buy &&
                                low <= _activeSetup.RetestZoneHigh &&
                                close > _activeSetup.BreakoutLevel &&
                                ((high - close) / barRange) <= ConfirmationClosePercent;

            bool sellConfirmed = _activeSetup.Direction == TradeType.Sell &&
                                 high >= _activeSetup.RetestZoneLow &&
                                 close < _activeSetup.BreakoutLevel &&
                                 ((close - low) / barRange) <= ConfirmationClosePercent;

            if (!buyConfirmed && !sellConfirmed)
            {
                LogVerbose(string.Format("Confirmation failed #{0} | dir={1} close={2:F2} breakout={3:F2}", _activeSetup.Id, _activeSetup.Direction, close, _activeSetup.BreakoutLevel));
                return;
            }

            ExecuteConfirmedEntry(lastClosedIndex, atr);
        }

        private void ExecuteConfirmedEntry(int lastClosedIndex, double atr)
        {
            if (_activeSetup == null)
                return;

            if (IsSpreadTooHigh())
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup("Spread too high at entry");
                return;
            }

            double entryPrice = GetExpectedEntryPrice(_activeSetup.Direction);
            double slBuffer = Math.Max(atr * SlBufferAtr, FixedMinSlBuffer);
            double stopPrice;

            if (_activeSetup.Direction == TradeType.Buy)
            {
                double lowestRetest = _activeSetup.LowestRetestPrice == double.MaxValue ? Bars.LowPrices[lastClosedIndex] : _activeSetup.LowestRetestPrice;
                stopPrice = Math.Min(Bars.LowPrices[lastClosedIndex], lowestRetest) - slBuffer;
            }
            else
            {
                double highestRetest = _activeSetup.HighestRetestPrice == double.MinValue ? Bars.HighPrices[lastClosedIndex] : _activeSetup.HighestRetestPrice;
                stopPrice = Math.Max(Bars.HighPrices[lastClosedIndex], highestRetest) + slBuffer;
            }

            double stopDistance = Math.Abs(entryPrice - stopPrice);
            if (stopDistance < MinStopDistance || stopDistance > MaxStopDistance)
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup(string.Format("Stop distance invalid ({0:F2})", stopDistance));
                return;
            }

            double stopPips = stopDistance / Symbol.PipSize;
            double tpPips = (stopDistance * TakeProfitR) / Symbol.PipSize;
            double riskAmount = _startOfDayEquity * (RiskPercent / 100.0);
            double rawVolumeInUnits = CalculateVolumeInUnits(stopPips, riskAmount);
            double volumeInUnits = Symbol.NormalizeVolumeInUnits(rawVolumeInUnits, RoundingMode.Down);

            LogSizing(entryPrice, stopPrice, stopDistance, stopPips, tpPips, riskAmount, rawVolumeInUnits, volumeInUnits);

            if (volumeInUnits < Symbol.VolumeInUnitsMin)
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup("Calculated volume below symbol minimum");
                return;
            }

            TradeResult result = ExecuteMarketOrder(_activeSetup.Direction, SymbolName, volumeInUnits, InstanceLabel, stopPips, tpPips);
            if (!result.IsSuccessful || result.Position == null)
            {
                RegisterRangeAttempt(_activeSetup.RangeKey);
                InvalidateSetup(string.Format("Order rejected: {0}", result.Error));
                return;
            }

            _activeTrade = new PositionSnapshot
            {
                PositionId = result.Position.Id,
                Direction = result.Position.TradeType,
                EntryPrice = result.Position.EntryPrice,
                InitialStopLoss = result.Position.StopLoss ?? stopPrice,
                InitialRisk = Math.Abs(result.Position.EntryPrice - (result.Position.StopLoss ?? stopPrice)),
                EntryBarIndex = Bars.Count - 1,
                BreakEvenMoved = false,
                SetupId = _activeSetup.Id,
                RangeKey = _activeSetup.RangeKey
            };

            _tradesToday++;
            Print("ENTRY {0} #{1} | volume={2} stopPips={3:F1} tpPips={4:F1} entry={5:F2} sl={6:F2}",
                _activeSetup.Direction,
                _activeSetup.Id,
                volumeInUnits,
                stopPips,
                tpPips,
                result.Position.EntryPrice,
                result.Position.StopLoss);
            _activeSetup.State = SetupState.EntryFilled;
        }

        private void ManageOpenTrade()
        {
            Position position = FindTrackedPosition();
            if (position == null)
            {
                _activeTrade = null;
                _activeSetup = null;
                return;
            }

            double currentPrice = position.TradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask;
            double rewardDistance = position.TradeType == TradeType.Buy
                ? currentPrice - _activeTrade.EntryPrice
                : _activeTrade.EntryPrice - currentPrice;

            if (!_activeTrade.BreakEvenMoved && _activeTrade.InitialRisk > 0 && rewardDistance >= _activeTrade.InitialRisk * BreakEvenAtR)
            {
                double breakEvenPrice = position.TradeType == TradeType.Buy
                    ? _activeTrade.EntryPrice + BreakEvenOffset
                    : _activeTrade.EntryPrice - BreakEvenOffset;

                TradeResult modifyResult = ModifyPosition(position, breakEvenPrice, position.TakeProfit);
                if (modifyResult.IsSuccessful)
                {
                    _activeTrade.BreakEvenMoved = true;
                    Print("BE moved for position {0} to {1:F2}", position.Id, breakEvenPrice);
                }
                else
                {
                    Print("BE move failed for position {0} | error={1}", position.Id, modifyResult.Error);
                }
            }

            int barsOpen = Math.Max(0, (Bars.Count - 1) - _activeTrade.EntryBarIndex);
            if (barsOpen >= MaxBarsInTrade)
            {
                TradeResult closeResult = ClosePosition(position);
                Print("TIME EXIT for position {0} after {1} bars | success={2} error={3}", position.Id, barsOpen, closeResult.IsSuccessful, closeResult.Error);
            }
        }

        private BiasSnapshot CalculateH1Bias()
        {
            int lastClosedIndex = _biasBars.Count - 2;
            if (lastClosedIndex < BiasLookbackH1 + SwingStrength)
                return BiasSnapshot.Neutral(lastClosedIndex, "Not enough bias history");

            int start = lastClosedIndex - BiasLookbackH1 + 1;
            double highest = double.MinValue;
            double lowest = double.MaxValue;
            for (int i = start; i <= lastClosedIndex; i++)
            {
                highest = Math.Max(highest, _biasBars.HighPrices[i]);
                lowest = Math.Min(lowest, _biasBars.LowPrices[i]);
            }

            double midpoint = (highest + lowest) / 2.0;
            double close = _biasBars.ClosePrices[lastClosedIndex];
            SwingPoint lastSwingHigh = FindLastSwingHigh(_biasBars, lastClosedIndex - SwingStrength);
            SwingPoint lastSwingLow = FindLastSwingLow(_biasBars, lastClosedIndex - SwingStrength);

            if (lastSwingHigh == null || lastSwingLow == null)
                return new BiasSnapshot(lastClosedIndex, TradeBias.Neutral, midpoint, close, highest, lowest, lastSwingHigh, lastSwingLow, false, false, false, false, "Missing swing data");

            bool bullishBreak = close > lastSwingHigh.Price;
            bool bearishBreak = close < lastSwingLow.Price;
            bool swingLowIntact = !WasCloseBrokenBelow(_biasBars, lastSwingLow.Index + 1, lastClosedIndex, lastSwingLow.Price);
            bool swingHighIntact = !WasCloseBrokenAbove(_biasBars, lastSwingHigh.Index + 1, lastClosedIndex, lastSwingHigh.Price);

            if (close > midpoint && bullishBreak && swingLowIntact)
                return new BiasSnapshot(lastClosedIndex, TradeBias.Bullish, midpoint, close, highest, lowest, lastSwingHigh, lastSwingLow, bullishBreak, bearishBreak, swingLowIntact, swingHighIntact, "Bullish structure confirmed");

            if (close < midpoint && bearishBreak && swingHighIntact)
                return new BiasSnapshot(lastClosedIndex, TradeBias.Bearish, midpoint, close, highest, lowest, lastSwingHigh, lastSwingLow, bullishBreak, bearishBreak, swingLowIntact, swingHighIntact, "Bearish structure confirmed");

            return new BiasSnapshot(lastClosedIndex, TradeBias.Neutral, midpoint, close, highest, lowest, lastSwingHigh, lastSwingLow, bullishBreak, bearishBreak, swingLowIntact, swingHighIntact, "No clean directional structure");
        }

        private SwingPoint FindLastSwingHigh(Bars bars, int endIndex)
        {
            for (int i = endIndex; i >= SwingStrength; i--)
            {
                if (i + SwingStrength >= bars.Count)
                    continue;

                bool isSwing = true;
                for (int left = 1; left <= SwingStrength; left++)
                {
                    if (bars.HighPrices[i] <= bars.HighPrices[i - left] || bars.HighPrices[i] <= bars.HighPrices[i + left])
                    {
                        isSwing = false;
                        break;
                    }
                }

                if (isSwing)
                    return new SwingPoint { Index = i, Price = bars.HighPrices[i] };
            }

            return null;
        }

        private SwingPoint FindLastSwingLow(Bars bars, int endIndex)
        {
            for (int i = endIndex; i >= SwingStrength; i--)
            {
                if (i + SwingStrength >= bars.Count)
                    continue;

                bool isSwing = true;
                for (int left = 1; left <= SwingStrength; left++)
                {
                    if (bars.LowPrices[i] >= bars.LowPrices[i - left] || bars.LowPrices[i] >= bars.LowPrices[i + left])
                    {
                        isSwing = false;
                        break;
                    }
                }

                if (isSwing)
                    return new SwingPoint { Index = i, Price = bars.LowPrices[i] };
            }

            return null;
        }

        private bool WasCloseBrokenBelow(Bars bars, int startIndex, int endIndex, double level)
        {
            if (startIndex > endIndex)
                return false;

            for (int i = startIndex; i <= endIndex; i++)
                if (bars.ClosePrices[i] < level)
                    return true;

            return false;
        }

        private bool WasCloseBrokenAbove(Bars bars, int startIndex, int endIndex, double level)
        {
            if (startIndex > endIndex)
                return false;

            for (int i = startIndex; i <= endIndex; i++)
                if (bars.ClosePrices[i] > level)
                    return true;

            return false;
        }

        private double CalculateVolumeInUnits(double stopLossPips, double riskAmount)
        {
            if (stopLossPips <= 0 || riskAmount <= 0)
                return 0;

            double pipValuePerUnit = Symbol.LotSize > 0 ? Symbol.PipValue / Symbol.LotSize : 0;
            if (pipValuePerUnit <= 0)
                return 0;

            double volume = riskAmount / (stopLossPips * pipValuePerUnit);
            if (double.IsNaN(volume) || double.IsInfinity(volume) || volume <= 0)
                return 0;

            return volume;
        }

        private bool CanOpenNewTrade(out string reason)
        {
            if (_tradesToday >= MaxTradesPerDay)
            {
                reason = "Daily max trades reached";
                return false;
            }

            if (_lossesToday >= MaxLossesPerDay)
            {
                reason = "Daily max losses reached";
                return false;
            }

            double maxDailyLoss = _startOfDayEquity * (DailyLossCapPercent / 100.0);
            if (_closedNetToday <= -maxDailyLoss)
            {
                reason = string.Format("Daily loss cap reached ({0:F2}/{1:F2})", _closedNetToday, -maxDailyLoss);
                return false;
            }

            if (Positions.FindAll(InstanceLabel, SymbolName).Length > 0)
            {
                reason = "Existing position still open";
                return false;
            }

            reason = null;
            return true;
        }

        private bool IsAtrTradable(double atr)
        {
            if (MinAtrM15 > 0 && atr < MinAtrM15)
                return false;

            if (MaxAtrM15 > 0 && atr > MaxAtrM15)
                return false;

            return true;
        }

        private bool IsSpreadTooHigh()
        {
            return Symbol.Spread > MaxSpread;
        }

        private bool IsEntryWindow(DateTime barTime)
        {
            if (barTime.DayOfWeek == DayOfWeek.Friday && barTime.Hour >= FridayCutoffHour)
                return false;

            bool londonOpen = AllowLondonSession && IsHourWithin(barTime.Hour, LondonStartHour, LondonEndHour);
            bool newYorkOpen = AllowNewYorkSession && IsHourWithin(barTime.Hour, NewYorkStartHour, NewYorkEndHour);
            return londonOpen || newYorkOpen;
        }

        private bool IsHourWithin(int hour, int startHour, int endHour)
        {
            if (startHour == endHour)
                return false;

            if (startHour < endHour)
                return hour >= startHour && hour < endHour;

            return hour >= startHour || hour < endHour;
        }

        private void ResetDailyCountersIfNeeded()
        {
            DateTime currentDay = GetTradingDay(Server.Time);
            if (currentDay == _currentTradeDay)
                return;

            _currentTradeDay = currentDay;
            _tradesToday = 0;
            _lossesToday = 0;
            _closedNetToday = 0;
            _startOfDayEquity = Account.Equity;
            _activeSetup = null;
            _rangeAttempts.Clear();
            Print("Daily counters reset: {0:yyyy-MM-dd} | startEquity={1:F2}", currentDay, _startOfDayEquity);
        }

        private DateTime GetTradingDay(DateTime serverTime)
        {
            return serverTime.AddHours(TradingDayOffsetHours).Date;
        }

        private void OnPositionOpened(PositionOpenedEventArgs args)
        {
            if (args.Position.Label != InstanceLabel || args.Position.SymbolName != SymbolName)
                return;

            Print("Position opened #{0} {1} @ {2:F2}", args.Position.Id, args.Position.TradeType, args.Position.EntryPrice);
        }

        private void OnPositionClosed(PositionClosedEventArgs args)
        {
            if (args.Position.Label != InstanceLabel || args.Position.SymbolName != SymbolName)
                return;

            double net = args.Position.NetProfit;
            _closedNetToday += net;
            if (net < 0)
                _lossesToday++;

            if (_activeTrade != null && _activeTrade.PositionId == args.Position.Id)
                _activeTrade = null;

            if (_activeSetup != null)
                RegisterRangeAttempt(_activeSetup.RangeKey);
            else if (_activeTrade != null && !string.IsNullOrEmpty(_activeTrade.RangeKey))
                RegisterRangeAttempt(_activeTrade.RangeKey);

            _activeSetup = null;
            Print("Position closed #{0}, net={1:F2}, todayNet={2:F2}, lossesToday={3}", args.Position.Id, net, _closedNetToday, _lossesToday);
        }

        private void RefreshActiveTradeSnapshot()
        {
            if (_activeTrade == null)
                return;

            Position position = FindTrackedPosition();
            if (position == null)
            {
                _activeTrade = null;
                return;
            }

            if (position.StopLoss.HasValue)
                _activeTrade.InitialStopLoss = position.StopLoss.Value;
        }

        private Position FindTrackedPosition()
        {
            if (_activeTrade == null)
                return null;

            return Positions.FirstOrDefault(p => p.Id == _activeTrade.PositionId);
        }

        private void InvalidateSetup(string reason)
        {
            if (_activeSetup != null)
                Print("INVALIDATE #{0}: {1}", _activeSetup.Id, reason);

            _activeSetup = null;
        }

        private void LogReject(string reason, int barIndex)
        {
            Print("REJECT [{0}] {1} @ {2:yyyy-MM-dd HH:mm}", barIndex, reason, Bars.OpenTimes[barIndex]);
        }

        private void LogVerbose(string message)
        {
            if (VerboseLogging)
                Print("DEBUG {0}", message);
        }

        private void LogSizing(double entryPrice, double stopPrice, double stopDistance, double stopPips, double tpPips, double riskAmount, double rawVolumeInUnits, double normalizedVolumeInUnits)
        {
            Print("SIZING | equityStartDay={0:F2} equityNow={1:F2} riskAmt={2:F2} entry={3:F2} stop={4:F2} stopDist={5:F2} stopPips={6:F2} tpPips={7:F2} spread={8:F2} pipSize={9} pipValue={10} lotSize={11} rawVol={12:F2} normVol={13:F2}",
                _startOfDayEquity,
                Account.Equity,
                riskAmount,
                entryPrice,
                stopPrice,
                stopDistance,
                stopPips,
                tpPips,
                Symbol.Spread,
                Symbol.PipSize,
                Symbol.PipValue,
                Symbol.LotSize,
                rawVolumeInUnits,
                normalizedVolumeInUnits);
        }

        private void LogBiasSnapshotIfNeeded(BiasSnapshot snapshot)
        {
            if (!VerboseLogging)
                return;

            if (snapshot.BarIndex == _lastBiasLogH1BarIndex)
                return;

            _lastBiasLogH1BarIndex = snapshot.BarIndex;
            Print("BIAS | h1Bar={0} bias={1} reason={2} close={3:F2} midpoint={4:F2} high={5:F2} low={6:F2} swingHigh={7} swingLow={8} breakHigh={9} breakLow={10} lowIntact={11} highIntact={12}",
                snapshot.BarIndex,
                snapshot.Bias,
                snapshot.Reason,
                snapshot.Close,
                snapshot.Midpoint,
                snapshot.Highest,
                snapshot.Lowest,
                snapshot.LastSwingHigh != null ? snapshot.LastSwingHigh.Price.ToString("F2") : "n/a",
                snapshot.LastSwingLow != null ? snapshot.LastSwingLow.Price.ToString("F2") : "n/a",
                snapshot.BrokeSwingHigh,
                snapshot.BrokeSwingLow,
                snapshot.SwingLowIntact,
                snapshot.SwingHighIntact);
        }

        private double GetExpectedEntryPrice(TradeType direction)
        {
            return direction == TradeType.Buy ? Symbol.Ask : Symbol.Bid;
        }

        private string BuildRangeKey(TradeType direction, double rangeHigh, double rangeLow, DateTime barTime)
        {
            string sessionBucket = IsHourWithin(barTime.Hour, LondonStartHour, LondonEndHour) ? "LDN" : IsHourWithin(barTime.Hour, NewYorkStartHour, NewYorkEndHour) ? "NY" : "OTH";
            return string.Format("{0}|{1}|{2:yyyyMMdd}|{3:F2}|{4:F2}", direction, sessionBucket, GetTradingDay(barTime), rangeHigh, rangeLow);
        }

        private int GetRangeAttempts(string rangeKey)
        {
            int attempts;
            return _rangeAttempts.TryGetValue(rangeKey, out attempts) ? attempts : 0;
        }

        private void RegisterRangeAttempt(string rangeKey)
        {
            if (string.IsNullOrEmpty(rangeKey))
                return;

            int attempts = GetRangeAttempts(rangeKey) + 1;
            _rangeAttempts[rangeKey] = attempts;
            LogVerbose(string.Format("Range attempts updated | key={0} attempts={1}", rangeKey, attempts));
        }

        private enum TradeBias
        {
            Neutral,
            Bullish,
            Bearish
        }

        private enum SetupState
        {
            BreakoutArmed,
            RetestObserved,
            EntryFilled
        }

        private class SwingPoint
        {
            public int Index { get; set; }
            public double Price { get; set; }
        }

        private class BreakoutSetup
        {
            public long Id { get; set; }
            public TradeType Direction { get; set; }
            public TradeBias Bias { get; set; }
            public double RangeHigh { get; set; }
            public double RangeLow { get; set; }
            public double BreakoutLevel { get; set; }
            public int BreakoutBarIndex { get; set; }
            public DateTime BreakoutTime { get; set; }
            public double RetestZoneLow { get; set; }
            public double RetestZoneHigh { get; set; }
            public double LowestRetestPrice { get; set; }
            public double HighestRetestPrice { get; set; }
            public int RetestSeenBarIndex { get; set; }
            public string RangeKey { get; set; }
            public SetupState State { get; set; }
        }

        private class PositionSnapshot
        {
            public long PositionId { get; set; }
            public TradeType Direction { get; set; }
            public double EntryPrice { get; set; }
            public double InitialStopLoss { get; set; }
            public double InitialRisk { get; set; }
            public int EntryBarIndex { get; set; }
            public bool BreakEvenMoved { get; set; }
            public long SetupId { get; set; }
            public string RangeKey { get; set; }
        }

        private class BiasSnapshot
        {
            public int BarIndex { get; private set; }
            public TradeBias Bias { get; private set; }
            public double Midpoint { get; private set; }
            public double Close { get; private set; }
            public double Highest { get; private set; }
            public double Lowest { get; private set; }
            public SwingPoint LastSwingHigh { get; private set; }
            public SwingPoint LastSwingLow { get; private set; }
            public bool BrokeSwingHigh { get; private set; }
            public bool BrokeSwingLow { get; private set; }
            public bool SwingLowIntact { get; private set; }
            public bool SwingHighIntact { get; private set; }
            public string Reason { get; private set; }

            public BiasSnapshot(int barIndex, TradeBias bias, double midpoint, double close, double highest, double lowest, SwingPoint lastSwingHigh, SwingPoint lastSwingLow, bool brokeSwingHigh, bool brokeSwingLow, bool swingLowIntact, bool swingHighIntact, string reason)
            {
                BarIndex = barIndex;
                Bias = bias;
                Midpoint = midpoint;
                Close = close;
                Highest = highest;
                Lowest = lowest;
                LastSwingHigh = lastSwingHigh;
                LastSwingLow = lastSwingLow;
                BrokeSwingHigh = brokeSwingHigh;
                BrokeSwingLow = brokeSwingLow;
                SwingLowIntact = swingLowIntact;
                SwingHighIntact = swingHighIntact;
                Reason = reason;
            }

            public static BiasSnapshot Neutral(int barIndex, string reason)
            {
                return new BiasSnapshot(barIndex, TradeBias.Neutral, 0, 0, 0, 0, null, null, false, false, false, false, reason);
            }
        }
    }
}
