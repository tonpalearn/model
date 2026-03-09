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
        private const string BotLabel = "MTA_GOLD_BRC_v03_DIAG";

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
        private readonly Dictionary<string, int> _stats = new Dictionary<string, int>();

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

        [Parameter("Enable ATR Filter", DefaultValue = false)]
        public bool EnableAtrFilter { get; set; }

        [Parameter("ATR Filter Mode", DefaultValue = "RawPrice")]
        public string AtrFilterMode { get; set; }

        [Parameter("Break Buffer ATR", DefaultValue = 0.10, MinValue = 0.01, Step = 0.01)]
        public double BreakBufferAtr { get; set; }

        [Parameter("Fixed Min Break Buffer", DefaultValue = 0.30, MinValue = 0.0, Step = 0.01)]
        public double FixedMinBreakBuffer { get; set; }

        [Parameter("Retest Zone ATR", DefaultValue = 0.30, MinValue = 0.01, Step = 0.01)]
        public double RetestZoneAtr { get; set; }

        [Parameter("Retest Timeout Bars", DefaultValue = 6, MinValue = 1, MaxValue = 12)]
        public int RetestTimeoutBars { get; set; }

        [Parameter("Confirmation Mode", DefaultValue = "Hybrid")]
        public string ConfirmationMode { get; set; }

        [Parameter("Confirm Body Min", DefaultValue = 0.35, MinValue = 0.00, MaxValue = 0.95, Step = 0.05)]
        public double ConfirmationBodyMin { get; set; }

        [Parameter("Confirm Close Percent", DefaultValue = 0.45, MinValue = 0.05, MaxValue = 0.95, Step = 0.05)]
        public double ConfirmationClosePercent { get; set; }

        [Parameter("Probe Confirm Body Min", DefaultValue = 0.10, MinValue = 0.00, MaxValue = 0.95, Step = 0.05)]
        public double ProbeConfirmationBodyMin { get; set; }

        [Parameter("Probe Confirm Close Percent", DefaultValue = 0.75, MinValue = 0.05, MaxValue = 0.95, Step = 0.05)]
        public double ProbeConfirmationClosePercent { get; set; }

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

        [Parameter("Allow Stop Relax In Probe", DefaultValue = true)]
        public bool AllowStopDistanceRelaxInProbe { get; set; }

        [Parameter("Allow Stop Relax In Diagnostic", DefaultValue = false)]
        public bool AllowStopDistanceRelaxInDiagnostic { get; set; }

        [Parameter("Relaxed Stop Max Multiplier", DefaultValue = 1.60, MinValue = 1.0, Step = 0.05)]
        public double RelaxedStopDistanceMaxMultiplier { get; set; }

        [Parameter("Relaxed Stop Min Multiplier", DefaultValue = 0.80, MinValue = 0.10, MaxValue = 1.0, Step = 0.05)]
        public double RelaxedStopDistanceMinMultiplier { get; set; }

        [Parameter("Sizing Mode", DefaultValue = "RiskBased")]
        public string SizingMode { get; set; }

        [Parameter("Fixed Size (Lots)", DefaultValue = 0.01, MinValue = 0.0, Step = 0.01)]
        public double FixedLotSize { get; set; }

        [Parameter("Risk %", DefaultValue = 0.25, MinValue = 0.01, MaxValue = 5.0, Step = 0.01)]
        public double RiskPercent { get; set; }

        [Parameter("Risk Calibration Warning Mult", DefaultValue = 1.20, MinValue = 1.0, MaxValue = 10.0, Step = 0.05)]
        public double RiskCalibrationWarningMultiple { get; set; }

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

        [Parameter("Min ATR M15", DefaultValue = 0.0, MinValue = 0.0, Step = 0.1)]
        public double MinAtrM15 { get; set; }

        [Parameter("Max ATR M15", DefaultValue = 60.0, MinValue = 0.0, Step = 0.1)]
        public double MaxAtrM15 { get; set; }

        [Parameter("Bypass ATR In Diagnostic", DefaultValue = true)]
        public bool BypassAtrInDiagnostic { get; set; }

        [Parameter("Bypass ATR In Probe", DefaultValue = true)]
        public bool BypassAtrInProbe { get; set; }

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

        [Parameter("Diagnostic Mode", DefaultValue = true)]
        public bool DiagnosticMode { get; set; }

        [Parameter("Probe Mode", DefaultValue = false)]
        public bool ProbeMode { get; set; }

        [Parameter("Near-Miss Logging", DefaultValue = true)]
        public bool NearMissLogging { get; set; }

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

            Print("{0} started | symbol={1} | serverTime={2:yyyy-MM-dd HH:mm:ss} UTC | tradingDayOffset={3} | diagnostic={4} | probe={5}", InstanceLabel, SymbolName, Server.Time, TradingDayOffsetHours, DiagnosticMode, ProbeMode);
            Print("ATR FILTER | enabled={0} mode={1} min={2:F4} max={3:F4} unit={4} compare={5} bypassDiagnostic={6} bypassProbe={7}",
                EnableAtrFilter,
                GetAtrFilterMode(),
                GetMinAtrEffective(),
                GetMaxAtrEffective(),
                GetAtrFilterUnitLabel(),
                GetAtrFilterCompareLabel(),
                BypassAtrInDiagnostic,
                BypassAtrInProbe);
            Print("SYMBOL SPEC | pipSize={0} pipValue={1} tickValue={2} tickSize={3} lotSize={4} volMin={5} volMax={6} volStep={7}",
                Symbol.PipSize,
                Symbol.PipValue,
                Symbol.TickValue,
                Symbol.TickSize,
                Symbol.LotSize,
                Symbol.VolumeInUnitsMin,
                Symbol.VolumeInUnitsMax,
                Symbol.VolumeInUnitsStep);
            double configuredFixedVolumeInUnits = GetFixedVolumeInUnits(FixedLotSize);
            double configuredFixedVolumeNormalized = configuredFixedVolumeInUnits > 0
                ? Symbol.NormalizeVolumeInUnits(configuredFixedVolumeInUnits, RoundingMode.Down)
                : 0;
            Print("RISK CALIBRATION | riskPct={0:F2} pipValueMode=DirectSymbolPipValue warningMult={1:F2} minVolume={2:F2}",
                RiskPercent,
                RiskCalibrationWarningMultiple,
                Symbol.VolumeInUnitsMin);
            Print("SIZING CONFIG | mode={0} fixedLots={1:F2} fixedVolRaw={2:F2} fixedVolNorm={3:F2} fixedLotsNorm={4:F2} riskPct={5:F2} warningMult={6:F2}",
                GetSizingModeLabel(),
                FixedLotSize,
                configuredFixedVolumeInUnits,
                configuredFixedVolumeNormalized,
                VolumeInUnitsToQuantity(configuredFixedVolumeNormalized),
                RiskPercent,
                RiskCalibrationWarningMultiple);
            Print("DIAG PARAMS | breakBufferAtr={0:F2}->{1:F2} retestZoneAtr={2:F2}->{3:F2} confirmMode={4}->{5} confirmBody={6:F2}->{7:F2} confirmClosePct={8:F2}->{9:F2} probeBody={10:F2} probeClosePct={11:F2} maxSpread={12:F2}->{13:F2} maxBreakoutAtr={14:F2}->{15:F2}",
                BreakBufferAtr,
                GetBreakBufferAtrEffective(),
                RetestZoneAtr,
                GetRetestZoneAtrEffective(),
                ConfirmationMode,
                GetConfirmationModeEffective(),
                ConfirmationBodyMin,
                GetConfirmationBodyMinEffective(),
                ConfirmationClosePercent,
                GetConfirmationClosePercentEffective(),
                ProbeConfirmationBodyMin,
                ProbeConfirmationClosePercent,
                MaxSpread,
                GetMaxSpreadEffective(),
                MaxBreakoutCandleAtr,
                GetMaxBreakoutCandleAtrEffective());
            Print("STOP DISTANCE LIMITS | baseMin={0:F2} baseMax={1:F2} effectiveMin={2:F2} effectiveMax={3:F2} relaxContext={4} allowProbeRelax={5} allowDiagnosticRelax={6} relaxMaxMult={7:F2} relaxMinMult={8:F2}",
                MinStopDistance,
                MaxStopDistance,
                GetMinStopDistanceEffective(),
                GetMaxStopDistanceEffective(),
                GetStopDistanceRelaxationContextLabel(),
                AllowStopDistanceRelaxInProbe,
                AllowStopDistanceRelaxInDiagnostic,
                RelaxedStopDistanceMaxMultiplier,
                RelaxedStopDistanceMinMultiplier);
        }

        protected override void OnStop()
        {
            PrintStatsSummary("STOP SUMMARY");
        }

        protected override void OnBar()
        {
            ResetDailyCountersIfNeeded();
            RefreshActiveTradeSnapshot();
            IncrementStat("bar.total");

            if (Bars.Count < Math.Max(RangeLookbackM15 + 5, AtrPeriodM15 + 5))
            {
                IncrementStat("bar.skip.notEnoughM15History");
                return;
            }

            if (_biasBars == null || _biasBars.Count < BiasLookbackH1 + (SwingStrength * 2) + 10)
            {
                IncrementStat("bar.skip.notEnoughBiasHistory");
                return;
            }

            if (_activeTrade != null)
            {
                IncrementStat("trade.manage.called");
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
            IncrementStat("pipeline.entered");

            string atrReason;
            string atrDetail;
            if (!IsAtrTradable(atr, lastClosedIndex, out atrReason, out atrDetail))
            {
                LogGateReject("ATR", atrReason, lastClosedIndex, atrDetail);
                InvalidateSetup("ATR out of allowed band");
                return;
            }
            LogGatePass("ATR", lastClosedIndex, atrDetail);

            if (!IsEntryWindow(barTime))
            {
                LogGateReject("SESSION", "Outside session window", lastClosedIndex, string.Format("hour={0} dow={1}", barTime.Hour, barTime.DayOfWeek));
                InvalidateSetup("Outside session window");
                return;
            }
            LogGatePass("SESSION", lastClosedIndex, string.Format("hour={0} dow={1}", barTime.Hour, barTime.DayOfWeek));

            double spread = Symbol.Spread;
            if (IsSpreadTooHigh())
            {
                LogGateReject("SPREAD", "Spread too high", lastClosedIndex, string.Format("spread={0:F2} max={1:F2}", spread, GetMaxSpreadEffective()));
                InvalidateSetup("Spread too high");
                return;
            }
            LogGatePass("SPREAD", lastClosedIndex, string.Format("spread={0:F2} max={1:F2}", spread, GetMaxSpreadEffective()));

            BiasSnapshot bias = CalculateH1Bias();
            LogBiasSnapshotIfNeeded(bias);
            if (bias.Bias == TradeBias.Neutral)
            {
                LogGateReject("BIAS", bias.Reason, lastClosedIndex, string.Format("close={0:F2} midpoint={1:F2}", bias.Close, bias.Midpoint));
                InvalidateSetup("Neutral H1 bias");
                return;
            }
            LogGatePass("BIAS", lastClosedIndex, string.Format("bias={0} reason={1}", bias.Bias, bias.Reason));

            string guardReason;
            if (!CanOpenNewTrade(out guardReason))
            {
                LogGateReject("RISK_GUARD", guardReason, lastClosedIndex, string.Format("tradesToday={0} lossesToday={1} closedNetToday={2:F2}", _tradesToday, _lossesToday, _closedNetToday));
                InvalidateSetup(guardReason);
                return;
            }
            LogGatePass("RISK_GUARD", lastClosedIndex, string.Format("tradesToday={0} lossesToday={1} closedNetToday={2:F2}", _tradesToday, _lossesToday, _closedNetToday));

            if (_activeSetup == null)
            {
                TryArmBreakout(lastClosedIndex, bias, atr);
                return;
            }

            if (_activeSetup.Bias != bias.Bias)
            {
                LogGateReject("BIAS_SYNC", "Bias changed before entry", lastClosedIndex, string.Format("setupBias={0} currentBias={1}", _activeSetup.Bias, bias.Bias));
                InvalidateSetup("Bias changed before entry");
                return;
            }
            LogGatePass("BIAS_SYNC", lastClosedIndex, string.Format("setupBias={0}", _activeSetup.Bias));

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
                IncrementStat("retest.timeout");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("RETEST", "Retest timeout", lastClosedIndex, string.Format("barsSinceBreak={0} timeout={1}", barsSinceBreak, RetestTimeoutBars));
                InvalidateSetup("Retest timeout");
                return;
            }

            double close = Bars.ClosePrices[lastClosedIndex];
            if (_activeSetup.Direction == TradeType.Buy && close < _activeSetup.RangeLow)
            {
                IncrementStat("setup.invalidated.oppositeRange");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("SETUP_INVALIDATE", "Price closed below opposite side of range", lastClosedIndex, string.Format("close={0:F2} rangeLow={1:F2}", close, _activeSetup.RangeLow));
                InvalidateSetup("Price closed below opposite side of range");
                return;
            }

            if (_activeSetup.Direction == TradeType.Sell && close > _activeSetup.RangeHigh)
            {
                IncrementStat("setup.invalidated.oppositeRange");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("SETUP_INVALIDATE", "Price closed above opposite side of range", lastClosedIndex, string.Format("close={0:F2} rangeHigh={1:F2}", close, _activeSetup.RangeHigh));
                InvalidateSetup("Price closed above opposite side of range");
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
            double maxBreakoutAtr = GetMaxBreakoutCandleAtrEffective();
            if (candleRange > atr * maxBreakoutAtr)
            {
                LogGateReject("BREAKOUT_ARM", "Breakout candle too large", lastClosedIndex, string.Format("candleRange={0:F2} atr={1:F2} limitAtr={2:F2}", candleRange, atr, maxBreakoutAtr));
                return;
            }

            double breakBuffer = Math.Max(atr * GetBreakBufferAtrEffective(), FixedMinBreakBuffer);
            double close = Bars.ClosePrices[lastClosedIndex];
            string rangeKeyBuy = BuildRangeKey(TradeType.Buy, rangeHigh, rangeLow, Bars.OpenTimes[lastClosedIndex]);
            string rangeKeySell = BuildRangeKey(TradeType.Sell, rangeHigh, rangeLow, Bars.OpenTimes[lastClosedIndex]);

            if (bias.Bias == TradeBias.Bullish && close > rangeHigh + breakBuffer)
            {
                if (GetRangeAttempts(rangeKeyBuy) >= MaxAttemptsPerRange)
                {
                    LogGateReject("BREAKOUT_ARM", "Range attempt limit reached (buy)", lastClosedIndex, string.Format("attempts={0} max={1}", GetRangeAttempts(rangeKeyBuy), MaxAttemptsPerRange));
                    return;
                }

                LogGatePass("BREAKOUT_ARM", lastClosedIndex, string.Format("dir=Buy close={0:F2} rangeHigh={1:F2} buffer={2:F2}", close, rangeHigh, breakBuffer));
                ArmSetup(TradeType.Buy, bias.Bias, rangeHigh, rangeLow, lastClosedIndex, atr, rangeKeyBuy);
                return;
            }

            if (bias.Bias == TradeBias.Bearish && close < rangeLow - breakBuffer)
            {
                if (GetRangeAttempts(rangeKeySell) >= MaxAttemptsPerRange)
                {
                    LogGateReject("BREAKOUT_ARM", "Range attempt limit reached (sell)", lastClosedIndex, string.Format("attempts={0} max={1}", GetRangeAttempts(rangeKeySell), MaxAttemptsPerRange));
                    return;
                }

                LogGatePass("BREAKOUT_ARM", lastClosedIndex, string.Format("dir=Sell close={0:F2} rangeLow={1:F2} buffer={2:F2}", close, rangeLow, breakBuffer));
                ArmSetup(TradeType.Sell, bias.Bias, rangeHigh, rangeLow, lastClosedIndex, atr, rangeKeySell);
                return;
            }

            LogNearMissBreakout(lastClosedIndex, bias, close, rangeHigh, rangeLow, breakBuffer);
        }

        private void ArmSetup(TradeType direction, TradeBias bias, double rangeHigh, double rangeLow, int breakoutBarIndex, double atr, string rangeKey)
        {
            _setupSequence++;
            double breakoutLevel = direction == TradeType.Buy ? rangeHigh : rangeLow;
            double zoneWidth = Math.Max(Symbol.PipSize, atr * GetRetestZoneAtrEffective());

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

            IncrementStat("breakout.armed");
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
                    IncrementStat("retest.seen");
                    LogGatePass("RETEST", lastClosedIndex, string.Format("setupId={0} dir={1} low={2:F2} high={3:F2} zone=[{4:F2},{5:F2}]",
                        _activeSetup.Id, _activeSetup.Direction, low, high, _activeSetup.RetestZoneLow, _activeSetup.RetestZoneHigh));
                }
            }
            else
            {
                IncrementStat("retest.missed.bar");
                LogGatePass("RETEST_WAIT", lastClosedIndex, string.Format("setupId={0} dir={1} low={2:F2} high={3:F2} zone=[{4:F2},{5:F2}]",
                    _activeSetup.Id, _activeSetup.Direction, low, high, _activeSetup.RetestZoneLow, _activeSetup.RetestZoneHigh));
            }

            if (_activeSetup.State != SetupState.RetestObserved)
                return;

            IncrementStat("confirm.attempt");

            if (!AllowSameBarRetestConfirmation && _activeSetup.RetestSeenBarIndex == lastClosedIndex)
            {
                IncrementStat("confirm.wait.nextBar");
                LogGateReject("CONFIRM", "Waiting next bar because same-bar confirmation disabled", lastClosedIndex, string.Format("setupId={0} mode={1} retestBar={2} currentBar={3}", _activeSetup.Id, GetConfirmationModeEffective(), _activeSetup.RetestSeenBarIndex, lastClosedIndex));
                return;
            }

            ConfirmationCheckResult result = EvaluateConfirmation(lastClosedIndex, open, high, low, close);
            LogConfirmationTrace(lastClosedIndex, result);

            if (!result.Passed)
            {
                IncrementStat("confirm.fail." + result.FailureCode);
                LogGateReject("CONFIRM", result.FailureReason, lastClosedIndex, BuildConfirmationMetricsDetail(result));

                if (NearMissLogging && result.BodyRangeOk && !result.BodyOk && result.BodyRatio >= Math.Max(0.0, result.BodyThreshold - 0.10))
                    Print("NEAR_MISS CONFIRM_BODY #{0} | dir={1} mode={2} bodyRatio={3:F2} min={4:F2}", _activeSetup.Id, _activeSetup.Direction, result.Mode, result.BodyRatio, result.BodyThreshold);

                if (result.BodyRangeOk && !result.CloseOk)
                    LogNearMissConfirmation(lastClosedIndex, result.BarRange, high, low, close, result.CloseThreshold);

                return;
            }

            IncrementStat("confirm.pass");
            IncrementStat("confirm.pass." + result.Mode);
            LogGatePass("CONFIRM", lastClosedIndex, BuildConfirmationMetricsDetail(result));
            ExecuteConfirmedEntry(lastClosedIndex, atr);
        }

        private void ExecuteConfirmedEntry(int lastClosedIndex, double atr)
        {
            if (_activeSetup == null)
                return;

            if (IsSpreadTooHigh())
            {
                IncrementStat("entry.reject.spread");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("ORDER_PRECHECK", "Spread too high at entry", lastClosedIndex, string.Format("spread={0:F2} max={1:F2}", Symbol.Spread, GetMaxSpreadEffective()));
                InvalidateSetup("Spread too high at entry");
                return;
            }
            LogGatePass("ORDER_PRECHECK", lastClosedIndex, string.Format("spread={0:F2}", Symbol.Spread));

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

            StopDistanceCheck stopCheck = EvaluateStopDistance(lastClosedIndex, entryPrice, stopPrice, atr, slBuffer);
            LogStopDistanceCheck(lastClosedIndex, stopCheck);
            if (!stopCheck.IsValid)
            {
                IncrementStat("entry.reject.stopDistance");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("SIZING", "Stop distance invalid", lastClosedIndex, BuildStopDistanceDetail(stopCheck));
                if (NearMissLogging && (Math.Abs(stopCheck.StopDistancePrice - stopCheck.EffectiveMinPrice) <= 0.30 || Math.Abs(stopCheck.StopDistancePrice - stopCheck.EffectiveMaxPrice) <= 0.50))
                    Print("NEAR_MISS STOP_DISTANCE #{0} | dir={1} stopDistance={2:F2} band=[{3:F2},{4:F2}] context={5}", _activeSetup.Id, _activeSetup.Direction, stopCheck.StopDistancePrice, stopCheck.EffectiveMinPrice, stopCheck.EffectiveMaxPrice, stopCheck.RelaxationContext);
                InvalidateSetup(string.Format("Stop distance invalid ({0:F2})", stopCheck.StopDistancePrice));
                return;
            }
            LogGatePass("SIZING", lastClosedIndex, BuildStopDistanceDetail(stopCheck));

            double stopPips = stopCheck.StopDistancePips;
            double tpPips = stopPips * TakeProfitR;
            double riskAmount = _startOfDayEquity * (RiskPercent / 100.0);
            double pipValuePerUnit = GetPipValuePerUnit();
            RiskCalibrationResult riskPlan = EvaluateSizingPlan(stopCheck, tpPips, riskAmount, pipValuePerUnit);

            LogSizing(stopCheck, tpPips, riskPlan);

            if (!riskPlan.IsSizingMathValid)
            {
                IncrementStat("entry.reject.sizingMath");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("SIZING", "Sizing math invalid", lastClosedIndex, riskPlan.RejectDetail);
                InvalidateSetup("Sizing math invalid");
                return;
            }

            if (!riskPlan.CanTrade)
            {
                IncrementStat(riskPlan.RejectStatKey);
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("SIZING", riskPlan.RejectReason, lastClosedIndex, riskPlan.RejectDetail);
                InvalidateSetup(riskPlan.RejectReason);
                return;
            }

            double volumeInUnits = riskPlan.NormalizedVolumeInUnits;
            LogOrderRequest(lastClosedIndex, stopCheck, tpPips, riskPlan);

            TradeResult result = ExecuteMarketOrder(_activeSetup.Direction, SymbolName, volumeInUnits, InstanceLabel, stopPips, tpPips);
            if (!result.IsSuccessful || result.Position == null)
            {
                IncrementStat("order.submit.fail");
                RegisterRangeAttempt(_activeSetup.RangeKey);
                LogGateReject("ORDER_SUBMIT", "Order rejected", lastClosedIndex, string.Format("error={0}", result.Error));
                InvalidateSetup(string.Format("Order rejected: {0}", result.Error));
                return;
            }

            IncrementStat("order.submit.pass");
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
                RangeKey = _activeSetup.RangeKey,
                VolumeInUnits = volumeInUnits,
                SizingModeLabel = riskPlan.SizingModeLabel,
                RequestedLots = riskPlan.RequestedLots,
                TargetRiskAmount = riskPlan.RiskAmount,
                ExpectedLossAtStop = riskPlan.EstimatedLossNormalized,
                EstimatedLossAtMinVolume = riskPlan.EstimatedLossAtMinVolume,
                ExpectedLossAtRawVolume = riskPlan.EstimatedLossRaw,
                StopDistancePips = stopPips,
                PipValuePerUnit = riskPlan.PipValuePerUnit,
                UsedMinVolumeConstraint = riskPlan.RawVolumeBelowMin
            };

            _tradesToday++;
            LogGatePass("ORDER_SUBMIT", lastClosedIndex, string.Format("positionId={0} volume={1:F2}", result.Position.Id, volumeInUnits));
            Print("ENTRY {0} #{1} | sizingMode={2} fixedLots={3:F2} volume={4:F2} stopPips={5:F1} tpPips={6:F1} targetRisk={7:F2} expectedStopLoss={8:F2} entry={9:F2} sl={10:F2}",
                _activeSetup.Direction,
                _activeSetup.Id,
                riskPlan.SizingModeLabel,
                riskPlan.RequestedLots,
                volumeInUnits,
                stopPips,
                tpPips,
                riskPlan.RiskAmount,
                riskPlan.EstimatedLossNormalized,
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
                    IncrementStat("trade.be.pass");
                    Print("BE moved for position {0} to {1:F2}", position.Id, breakEvenPrice);
                }
                else
                {
                    IncrementStat("trade.be.fail");
                    Print("BE move failed for position {0} | error={1}", position.Id, modifyResult.Error);
                }
            }

            int barsOpen = Math.Max(0, (Bars.Count - 1) - _activeTrade.EntryBarIndex);
            if (barsOpen >= MaxBarsInTrade)
            {
                TradeResult closeResult = ClosePosition(position);
                if (closeResult.IsSuccessful)
                    IncrementStat("trade.timeExit.pass");
                else
                    IncrementStat("trade.timeExit.fail");
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

        private double CalculateVolumeInUnits(double stopLossPips, double riskAmount, double pipValuePerUnit)
        {
            if (stopLossPips <= 0 || riskAmount <= 0 || pipValuePerUnit <= 0)
                return 0;

            double volume = riskAmount / (stopLossPips * pipValuePerUnit);
            if (!IsFinitePositive(volume))
                return 0;

            return volume;
        }

        private double GetFixedVolumeInUnits(double fixedLots)
        {
            if (!IsFinitePositive(fixedLots))
                return 0;

            double volumeInUnits = Symbol.QuantityToVolumeInUnits(fixedLots);
            return IsFinitePositive(volumeInUnits) ? volumeInUnits : 0;
        }

        private double VolumeInUnitsToQuantity(double volumeInUnits)
        {
            if (!IsFinitePositive(volumeInUnits))
                return 0;

            double quantity = Symbol.VolumeInUnitsToQuantity(volumeInUnits);
            return IsFinitePositive(quantity) ? quantity : 0;
        }

        private RiskCalibrationResult EvaluateSizingPlan(StopDistanceCheck stopCheck, double tpPips, double riskAmount, double pipValuePerUnit)
        {
            SizingModeKind sizingMode = GetSizingMode();
            bool isFixedLotMode = sizingMode == SizingModeKind.FixedLot;
            double requestedLots = isFixedLotMode ? FixedLotSize : 0;
            double rawVolumeInUnits = isFixedLotMode
                ? GetFixedVolumeInUnits(FixedLotSize)
                : CalculateVolumeInUnits(stopCheck.StopDistancePips, riskAmount, pipValuePerUnit);
            double normalizedVolumeInUnits = rawVolumeInUnits > 0
                ? Symbol.NormalizeVolumeInUnits(rawVolumeInUnits, RoundingMode.Down)
                : 0;
            double minVolumeInUnits = Symbol.VolumeInUnitsMin;
            double estimatedLossRaw = EstimateLossAtStop(stopCheck.StopDistancePips, rawVolumeInUnits, pipValuePerUnit);
            double estimatedLossNormalized = EstimateLossAtStop(stopCheck.StopDistancePips, normalizedVolumeInUnits, pipValuePerUnit);
            double estimatedLossAtMinVolume = EstimateLossAtStop(stopCheck.StopDistancePips, minVolumeInUnits, pipValuePerUnit);
            double normalizedRiskMultiple = riskAmount > 0 ? estimatedLossNormalized / riskAmount : 0;
            double minVolumeRiskMultiple = riskAmount > 0 ? estimatedLossAtMinVolume / riskAmount : 0;
            bool sizingMathValid = Symbol.PipSize > 0 && Symbol.TickSize > 0 && stopCheck.StopDistancePips > 0 && tpPips > 0 && pipValuePerUnit > 0 && IsFinitePositive(rawVolumeInUnits);

            var result = new RiskCalibrationResult
            {
                SizingModeLabel = GetSizingModeLabel(sizingMode),
                RequestedLots = requestedLots,
                RequestedVolumeInUnits = rawVolumeInUnits,
                NormalizedLots = VolumeInUnitsToQuantity(normalizedVolumeInUnits),
                IsFixedLotMode = isFixedLotMode,
                RiskAmount = riskAmount,
                PipValuePerUnit = pipValuePerUnit,
                RawVolumeInUnits = rawVolumeInUnits,
                NormalizedVolumeInUnits = normalizedVolumeInUnits,
                MinVolumeInUnits = minVolumeInUnits,
                EstimatedLossRaw = estimatedLossRaw,
                EstimatedLossNormalized = estimatedLossNormalized,
                EstimatedLossAtMinVolume = estimatedLossAtMinVolume,
                NormalizedRiskMultiple = normalizedRiskMultiple,
                MinVolumeRiskMultiple = minVolumeRiskMultiple,
                NormalizedRiskWarning = riskAmount > 0 && estimatedLossNormalized >= riskAmount * RiskCalibrationWarningMultiple,
                MinVolumeRiskWarning = riskAmount > 0 && estimatedLossAtMinVolume >= riskAmount * RiskCalibrationWarningMultiple,
                IsSizingMathValid = sizingMathValid,
                CanTrade = sizingMathValid,
                RejectStatKey = "entry.reject.sizingMath",
                RejectReason = "Sizing math invalid",
                RejectDetail = string.Format("mode={0} pipSize={1} tickSize={2} stopPips={3:F2} tpPips={4:F2} pipValuePerUnit={5} rawVolume={6:F4} fixedLots={7:F2}", GetSizingModeLabel(sizingMode), Symbol.PipSize, Symbol.TickSize, stopCheck.StopDistancePips, tpPips, pipValuePerUnit, rawVolumeInUnits, requestedLots)
            };

            if (!sizingMathValid)
            {
                if (isFixedLotMode && !IsFinitePositive(requestedLots))
                {
                    result.RejectReason = "Fixed lot size must be greater than zero";
                    result.RejectDetail = string.Format("mode={0} fixedLots={1:F2} rawVol={2:F4} stopPips={3:F2} tpPips={4:F2} pipValuePerUnit={5}", result.SizingModeLabel, requestedLots, rawVolumeInUnits, stopCheck.StopDistancePips, tpPips, pipValuePerUnit);
                }

                return result;
            }

            if (rawVolumeInUnits < minVolumeInUnits)
            {
                result.CanTrade = false;
                result.RejectStatKey = "entry.reject.volumeBelowMin";
                result.RejectReason = isFixedLotMode ? "Fixed lot converts below broker minimum volume" : "Broker min volume exceeds target risk";
                result.RejectDetail = string.Format("mode={0} targetRisk={1:F2} fixedLots={2:F2} rawVol={3:F4} normVol={4:F2} minVol={5:F2} rawLoss={6:F2} normLoss={7:F2} minVolLoss={8:F2} minRiskMult={9:F2} pipValuePerUnit={10} stopPips={11:F2}",
                    result.SizingModeLabel,
                    riskAmount,
                    requestedLots,
                    rawVolumeInUnits,
                    normalizedVolumeInUnits,
                    minVolumeInUnits,
                    estimatedLossRaw,
                    estimatedLossNormalized,
                    estimatedLossAtMinVolume,
                    minVolumeRiskMultiple,
                    pipValuePerUnit,
                    stopCheck.StopDistancePips);
                return result;
            }

            if (normalizedVolumeInUnits < minVolumeInUnits)
            {
                result.CanTrade = false;
                result.RejectStatKey = "entry.reject.volumeBelowMin";
                result.RejectReason = isFixedLotMode ? "Normalized fixed lot dropped below broker minimum" : "Normalized volume dropped below broker minimum";
                result.RejectDetail = string.Format("mode={0} targetRisk={1:F2} fixedLots={2:F2} rawVol={3:F4} normVol={4:F2} minVol={5:F2} rawLoss={6:F2} normLoss={7:F2} stopPips={8:F2}",
                    result.SizingModeLabel,
                    riskAmount,
                    requestedLots,
                    rawVolumeInUnits,
                    normalizedVolumeInUnits,
                    minVolumeInUnits,
                    estimatedLossRaw,
                    estimatedLossNormalized,
                    stopCheck.StopDistancePips);
                return result;
            }

            if (normalizedVolumeInUnits > Symbol.VolumeInUnitsMax)
            {
                result.CanTrade = false;
                result.RejectStatKey = "entry.reject.volumeAboveMax";
                result.RejectReason = isFixedLotMode ? "Fixed lot above symbol maximum volume" : "Calculated volume above symbol maximum";
                result.RejectDetail = string.Format("mode={0} targetRisk={1:F2} fixedLots={2:F2} rawVol={3:F4} normVol={4:F2} maxVol={5:F2} rawLoss={6:F2} normLoss={7:F2} stopPips={8:F2}",
                    result.SizingModeLabel,
                    riskAmount,
                    requestedLots,
                    rawVolumeInUnits,
                    normalizedVolumeInUnits,
                    Symbol.VolumeInUnitsMax,
                    estimatedLossRaw,
                    estimatedLossNormalized,
                    stopCheck.StopDistancePips);
                return result;
            }

            result.CanTrade = true;
            result.RejectStatKey = null;
            result.RejectReason = null;
            result.RejectDetail = null;
            return result;
        }

        private double EstimateLossAtStop(double stopLossPips, double volumeInUnits, double pipValuePerUnit)
        {
            if (stopLossPips <= 0 || volumeInUnits <= 0 || pipValuePerUnit <= 0)
                return 0;

            double estimatedLoss = stopLossPips * pipValuePerUnit * volumeInUnits;
            return IsFinite(estimatedLoss) ? estimatedLoss : 0;
        }

        private double GetPipValuePerUnit()
        {
            return Symbol.PipValue;
        }

        private bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private bool IsFinitePositive(double value)
        {
            return IsFinite(value) && value > 0;
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

        private bool IsAtrTradable(double atrRaw, int barIndex, out string reason, out string detail)
        {
            AtrFilterModeKind mode = GetAtrFilterMode();
            double atrComparable = ConvertAtrForFilter(atrRaw, barIndex, mode);
            double minAtr = GetMinAtrEffective();
            double maxAtr = GetMaxAtrEffective();

            detail = BuildAtrFilterDetail(atrRaw, atrComparable, minAtr, maxAtr, mode, barIndex);

            if (!EnableAtrFilter)
            {
                reason = null;
                detail += " | filter=disabled";
                return true;
            }

            if (ShouldBypassAtrFilter())
            {
                reason = null;
                detail += string.Format(" | filter=bypassed context={0}", GetAtrBypassContextLabel());
                return true;
            }

            if (minAtr > 0 && atrComparable < minAtr)
            {
                reason = "ATR below minimum";
                return false;
            }

            if (maxAtr > 0 && atrComparable > maxAtr)
            {
                reason = "ATR above maximum";
                return false;
            }

            reason = null;
            return true;
        }

        private bool ShouldBypassAtrFilter()
        {
            if (ProbeMode && BypassAtrInProbe)
                return true;

            if (DiagnosticMode && BypassAtrInDiagnostic)
                return true;

            return false;
        }

        private string GetAtrBypassContextLabel()
        {
            if (ProbeMode && BypassAtrInProbe)
                return "ProbeMode";

            if (DiagnosticMode && BypassAtrInDiagnostic)
                return "DiagnosticMode";

            return "None";
        }

        private string BuildAtrFilterDetail(double atrRaw, double atrComparable, double minAtr, double maxAtr, AtrFilterModeKind mode, int barIndex)
        {
            return string.Format("raw={0:F4} compare={1:F4} unit={2} mode={3} threshold=[{4:F4},{5:F4}] close={6:F2}",
                atrRaw,
                atrComparable,
                GetAtrFilterUnitLabel(mode),
                mode,
                minAtr,
                maxAtr,
                Bars.ClosePrices[barIndex]);
        }

        private double ConvertAtrForFilter(double atrRaw, int barIndex, AtrFilterModeKind mode)
        {
            switch (mode)
            {
                case AtrFilterModeKind.Pips:
                    return Symbol.PipSize > 0 ? atrRaw / Symbol.PipSize : atrRaw;
                case AtrFilterModeKind.PercentOfPrice:
                    double close = Bars.ClosePrices[barIndex];
                    return Math.Abs(close) > double.Epsilon ? (atrRaw / close) * 100.0 : 0.0;
                default:
                    return atrRaw;
            }
        }

        private SizingModeKind GetSizingMode()
        {
            string mode = SizingMode ?? string.Empty;

            if (string.Equals(mode, "Fixed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode, "FixedLot", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode, "FixedLots", StringComparison.OrdinalIgnoreCase))
                return SizingModeKind.FixedLot;

            return SizingModeKind.RiskBased;
        }

        private string GetSizingModeLabel()
        {
            return GetSizingModeLabel(GetSizingMode());
        }

        private string GetSizingModeLabel(SizingModeKind mode)
        {
            return mode == SizingModeKind.FixedLot ? "FixedLot" : "RiskBased";
        }

        private AtrFilterModeKind GetAtrFilterMode()
        {
            string mode = AtrFilterMode ?? string.Empty;

            if (string.Equals(mode, "Pips", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode, "Points", StringComparison.OrdinalIgnoreCase))
                return AtrFilterModeKind.Pips;

            if (string.Equals(mode, "Percent", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode, "PercentOfPrice", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode, "RelativePercent", StringComparison.OrdinalIgnoreCase))
                return AtrFilterModeKind.PercentOfPrice;

            return AtrFilterModeKind.RawPrice;
        }

        private string GetAtrFilterUnitLabel()
        {
            return GetAtrFilterUnitLabel(GetAtrFilterMode());
        }

        private string GetAtrFilterUnitLabel(AtrFilterModeKind mode)
        {
            switch (mode)
            {
                case AtrFilterModeKind.Pips:
                    return "pips";
                case AtrFilterModeKind.PercentOfPrice:
                    return "% of close";
                default:
                    return "price";
            }
        }

        private string GetAtrFilterCompareLabel()
        {
            return "min <= converted ATR <= max";
        }

        private bool IsSpreadTooHigh()
        {
            return Symbol.Spread > GetMaxSpreadEffective();
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

            PrintStatsSummary("DAY SUMMARY");
            _currentTradeDay = currentDay;
            _tradesToday = 0;
            _lossesToday = 0;
            _closedNetToday = 0;
            _startOfDayEquity = Account.Equity;
            _activeSetup = null;
            _rangeAttempts.Clear();
            _stats.Clear();
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
            string rangeKey = _activeTrade != null && _activeTrade.PositionId == args.Position.Id ? _activeTrade.RangeKey : _activeSetup != null ? _activeSetup.RangeKey : null;
            PositionSnapshot closedTrade = _activeTrade != null && _activeTrade.PositionId == args.Position.Id ? _activeTrade : null;

            _closedNetToday += net;
            if (net < 0)
                _lossesToday++;

            if (net >= 0)
                IncrementStat("trade.closed.winOrFlat");
            else
                IncrementStat("trade.closed.loss");

            if (_activeTrade != null && _activeTrade.PositionId == args.Position.Id)
                _activeTrade = null;

            RegisterRangeAttempt(rangeKey);
            _activeSetup = null;
            Print("Position closed #{0}, net={1:F2}, todayNet={2:F2}, lossesToday={3}", args.Position.Id, net, _closedNetToday, _lossesToday);
            LogRealizedVsPlannedRisk(args.Position, closedTrade, net);
            PrintStatsSummary("POST-TRADE STATS");
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

        private void LogRealizedVsPlannedRisk(Position position, PositionSnapshot closedTrade, double net)
        {
            if (closedTrade == null)
                return;

            double expectedStopLoss = closedTrade.ExpectedLossAtStop;
            double realizedLossMagnitude = Math.Abs(net);
            double realizedVsExpected = expectedStopLoss > 0 ? realizedLossMagnitude / expectedStopLoss : 0;
            double realizedVsTarget = closedTrade.TargetRiskAmount > 0 ? realizedLossMagnitude / closedTrade.TargetRiskAmount : 0;

            Print("REALIZED VS PLAN #{0} | positionId={1} dir={2} sizingMode={3} fixedLots={4:F2} volume={5:F2} targetRisk={6:F2} expectedStopLoss={7:F2} rawPlannedLoss={8:F2} minVolLoss={9:F2} net={10:F2} absNet={11:F2} realizedVsExpected={12:F2} realizedVsTarget={13:F2} stopPips={14:F2} pipValuePerUnit={15}",
                closedTrade.SetupId,
                position.Id,
                closedTrade.Direction,
                closedTrade.SizingModeLabel,
                closedTrade.RequestedLots,
                closedTrade.VolumeInUnits,
                closedTrade.TargetRiskAmount,
                expectedStopLoss,
                closedTrade.ExpectedLossAtRawVolume,
                closedTrade.EstimatedLossAtMinVolume,
                net,
                realizedLossMagnitude,
                realizedVsExpected,
                realizedVsTarget,
                closedTrade.StopDistancePips,
                closedTrade.PipValuePerUnit);
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

        private void LogSizing(StopDistanceCheck stopCheck, double tpPips, RiskCalibrationResult riskPlan)
        {
            Print("SIZING | mode={0} fixedLots={1:F2} fixedVolRaw={2:F2} fixedVolNorm={3:F2} fixedLotsNorm={4:F2} equityStartDay={5:F2} equityNow={6:F2} riskAmt={7:F2} entry={8:F2} stop={9:F2} stopDist={10:F2} stopPips={11:F2} stopTicks={12:F2} tpPips={13:F2} spread={14:F2} pipSize={15} tickSize={16} pipValue={17} pipValuePerUnit={18} lotSize={19} volStep={20} rawVol={21:F4} normVol={22:F2} minVol={23:F2} rawLoss={24:F2} normLoss={25:F2} minVolLoss={26:F2} normRiskMult={27:F2} minRiskMult={28:F2} warnNorm={29} warnMin={30} rawBelowMin={31} relaxContext={32} bandBase=[{33:F2},{34:F2}] bandEff=[{35:F2},{36:F2}] slBuffer={37:F2} atr={38:F2}",
                riskPlan.SizingModeLabel,
                riskPlan.RequestedLots,
                riskPlan.RequestedVolumeInUnits,
                riskPlan.NormalizedVolumeInUnits,
                riskPlan.NormalizedLots,
                _startOfDayEquity,
                Account.Equity,
                riskPlan.RiskAmount,
                stopCheck.EntryPrice,
                stopCheck.StopPrice,
                stopCheck.StopDistancePrice,
                stopCheck.StopDistancePips,
                stopCheck.StopDistanceTicks,
                tpPips,
                Symbol.Spread,
                Symbol.PipSize,
                Symbol.TickSize,
                Symbol.PipValue,
                riskPlan.PipValuePerUnit,
                Symbol.LotSize,
                Symbol.VolumeInUnitsStep,
                riskPlan.RawVolumeInUnits,
                riskPlan.NormalizedVolumeInUnits,
                riskPlan.MinVolumeInUnits,
                riskPlan.EstimatedLossRaw,
                riskPlan.EstimatedLossNormalized,
                riskPlan.EstimatedLossAtMinVolume,
                riskPlan.NormalizedRiskMultiple,
                riskPlan.MinVolumeRiskMultiple,
                riskPlan.NormalizedRiskWarning,
                riskPlan.MinVolumeRiskWarning,
                riskPlan.RawVolumeBelowMin,
                stopCheck.RelaxationContext,
                stopCheck.BaseMinPrice,
                stopCheck.BaseMaxPrice,
                stopCheck.EffectiveMinPrice,
                stopCheck.EffectiveMaxPrice,
                stopCheck.SlBuffer,
                stopCheck.Atr);

            if (riskPlan.IsFixedLotMode)
            {
                Print("FIXED LOT CHECK | fixedLots={0:F2} fixedVolRaw={1:F2} fixedVolNorm={2:F2} fixedLotsNorm={3:F2} targetRisk={4:F2} expectedStopLoss={5:F2} warningMult={6:F2}",
                    riskPlan.RequestedLots,
                    riskPlan.RequestedVolumeInUnits,
                    riskPlan.NormalizedVolumeInUnits,
                    riskPlan.NormalizedLots,
                    riskPlan.RiskAmount,
                    riskPlan.EstimatedLossNormalized,
                    RiskCalibrationWarningMultiple);
            }

            if (riskPlan.NormalizedRiskWarning || riskPlan.MinVolumeRiskWarning || riskPlan.RawVolumeBelowMin)
            {
                Print("RISK WARNING | mode={0} targetRisk={1:F2} rawLoss={2:F2} normLoss={3:F2} minVolLoss={4:F2} rawVol={5:F4} normVol={6:F2} minVol={7:F2} normRiskMult={8:F2} minRiskMult={9:F2}",
                    riskPlan.SizingModeLabel,
                    riskPlan.RiskAmount,
                    riskPlan.EstimatedLossRaw,
                    riskPlan.EstimatedLossNormalized,
                    riskPlan.EstimatedLossAtMinVolume,
                    riskPlan.RawVolumeInUnits,
                    riskPlan.NormalizedVolumeInUnits,
                    riskPlan.MinVolumeInUnits,
                    riskPlan.NormalizedRiskMultiple,
                    riskPlan.MinVolumeRiskMultiple);
            }

            if (riskPlan.IsFixedLotMode && riskPlan.NormalizedRiskWarning)
            {
                Print("FIXED LOT RISK WARNING | targetRisk={0:F2} expectedStopLoss={1:F2} warningMult={2:F2} fixedLots={3:F2} fixedVolNorm={4:F2} stopPips={5:F2} normRiskMult={6:F2}",
                    riskPlan.RiskAmount,
                    riskPlan.EstimatedLossNormalized,
                    RiskCalibrationWarningMultiple,
                    riskPlan.RequestedLots,
                    riskPlan.NormalizedVolumeInUnits,
                    stopCheck.StopDistancePips,
                    riskPlan.NormalizedRiskMultiple);
            }
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

        private void LogGatePass(string gate, int barIndex, string detail)
        {
            IncrementStat("gate." + gate + ".pass");
            if (DiagnosticMode)
                Print("GATE {0} PASS | bar={1} time={2:yyyy-MM-dd HH:mm} | {3}", gate, barIndex, Bars.OpenTimes[barIndex], detail);
        }

        private void LogGateReject(string gate, string reason, int barIndex, string detail)
        {
            IncrementStat("gate." + gate + ".reject");
            Print("GATE {0} REJECT | bar={1} time={2:yyyy-MM-dd HH:mm} | reason={3} | {4}", gate, barIndex, Bars.OpenTimes[barIndex], reason, detail);
        }

        private void LogNearMissBreakout(int barIndex, BiasSnapshot bias, double close, double rangeHigh, double rangeLow, double breakBuffer)
        {
            if (!NearMissLogging)
                return;

            if (bias.Bias == TradeBias.Bullish)
            {
                double distance = (rangeHigh + breakBuffer) - close;
                if (distance >= 0 && distance <= Math.Max(Symbol.PipSize * 8, breakBuffer * 0.35))
                {
                    IncrementStat("nearMiss.breakout");
                    Print("NEAR_MISS BREAKOUT | dir=Buy close={0:F2} trigger={1:F2} missBy={2:F2}", close, rangeHigh + breakBuffer, distance);
                }
            }
            else if (bias.Bias == TradeBias.Bearish)
            {
                double distance = close - (rangeLow - breakBuffer);
                if (distance >= 0 && distance <= Math.Max(Symbol.PipSize * 8, breakBuffer * 0.35))
                {
                    IncrementStat("nearMiss.breakout");
                    Print("NEAR_MISS BREAKOUT | dir=Sell close={0:F2} trigger={1:F2} missBy={2:F2}", close, rangeLow - breakBuffer, distance);
                }
            }
        }

        private void LogNearMissConfirmation(int barIndex, double barRange, double high, double low, double close, double closePercentThreshold)
        {
            if (!NearMissLogging || _activeSetup == null || barRange <= 0)
                return;

            double closeLocation = _activeSetup.Direction == TradeType.Buy ? (high - close) / barRange : (close - low) / barRange;
            if (closeLocation <= closePercentThreshold + 0.10)
            {
                IncrementStat("nearMiss.confirm");
                Print("NEAR_MISS CONFIRM_CLOSE #{0} | dir={1} closeLocation={2:F2} max={3:F2} close={4:F2} breakout={5:F2}",
                    _activeSetup.Id,
                    _activeSetup.Direction,
                    closeLocation,
                    closePercentThreshold,
                    close,
                    _activeSetup.BreakoutLevel);
            }
        }

        private ConfirmationCheckResult EvaluateConfirmation(int barIndex, double open, double high, double low, double close)
        {
            var mode = GetConfirmationModeEffective();
            double barRange = high - low;
            double body = Math.Abs(close - open);
            double bodyRatio = barRange > 0 ? body / barRange : 0.0;
            double closeLocation = 1.0;
            bool directional = false;
            bool breakoutReclaimed = false;
            bool closeStrengthOk = false;
            bool touchedRetestZone = high >= _activeSetup.RetestZoneLow && low <= _activeSetup.RetestZoneHigh;

            double bodyThreshold = GetConfirmationBodyMinEffective(mode);
            double closeThreshold = GetConfirmationClosePercentEffective(mode);

            if (barRange > 0)
            {
                if (_activeSetup.Direction == TradeType.Buy)
                {
                    closeLocation = (high - close) / barRange;
                    directional = close > open;
                    breakoutReclaimed = close > _activeSetup.BreakoutLevel;
                }
                else
                {
                    closeLocation = (close - low) / barRange;
                    directional = close < open;
                    breakoutReclaimed = close < _activeSetup.BreakoutLevel;
                }

                closeStrengthOk = closeLocation <= closeThreshold;
            }

            bool bodyRangeOk = barRange > Symbol.PipSize;
            bool bodyOk = bodyRangeOk && bodyRatio >= bodyThreshold;
            bool closeOk = touchedRetestZone && breakoutReclaimed && closeStrengthOk;

            bool passed;
            switch (mode)
            {
                case ConfirmationModeKind.StrictBody:
                    passed = bodyOk && closeOk && directional;
                    break;
                case ConfirmationModeKind.DirectionalClose:
                    passed = closeOk && directional;
                    break;
                case ConfirmationModeKind.WeakBodyProbe:
                    passed = bodyRangeOk && touchedRetestZone && breakoutReclaimed && directional;
                    break;
                case ConfirmationModeKind.ProbeConfirm:
                    passed = bodyRangeOk && touchedRetestZone && breakoutReclaimed;
                    break;
                default:
                    passed = closeOk && directional && (bodyOk || bodyRatio >= Math.Max(0.05, bodyThreshold * 0.70));
                    break;
            }

            string failureCode = "ok";
            string failureReason = "ok";
            if (!bodyRangeOk)
            {
                failureCode = "smallRange";
                failureReason = "Bar range too small";
            }
            else if (!touchedRetestZone)
            {
                failureCode = "zoneMiss";
                failureReason = "Confirmation bar missed retest zone";
            }
            else if (!breakoutReclaimed)
            {
                failureCode = "breakoutReclaim";
                failureReason = "Close did not reclaim breakout level";
            }
            else if (!directional && mode != ConfirmationModeKind.ProbeConfirm)
            {
                failureCode = "direction";
                failureReason = "Close direction did not match setup";
            }
            else if ((mode == ConfirmationModeKind.StrictBody || mode == ConfirmationModeKind.Hybrid) && !bodyOk)
            {
                failureCode = "bodyRatio";
                failureReason = mode == ConfirmationModeKind.Hybrid ? "Body ratio too weak for hybrid confirmation" : "Body ratio below minimum";
            }
            else if ((mode == ConfirmationModeKind.StrictBody || mode == ConfirmationModeKind.DirectionalClose || mode == ConfirmationModeKind.Hybrid) && !closeStrengthOk)
            {
                failureCode = "closeLocation";
                failureReason = "Close location too weak";
            }

            return new ConfirmationCheckResult
            {
                Mode = mode.ToString(),
                Passed = passed,
                FailureCode = failureCode,
                FailureReason = failureReason,
                BarIndex = barIndex,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                BarRange = barRange,
                Body = body,
                BodyRatio = bodyRatio,
                BodyThreshold = bodyThreshold,
                CloseLocation = closeLocation,
                CloseThreshold = closeThreshold,
                Directional = directional,
                BreakoutReclaimed = breakoutReclaimed,
                CloseStrengthOk = closeStrengthOk,
                BodyRangeOk = bodyRangeOk,
                BodyOk = bodyOk,
                CloseOk = closeOk,
                TouchedRetestZone = touchedRetestZone
            };
        }

        private void LogConfirmationTrace(int barIndex, ConfirmationCheckResult result)
        {
            if (!VerboseLogging)
                return;

            Print("CONFIRM TRACE #{0} | bar={1} time={2:yyyy-MM-dd HH:mm} mode={3} dir={4} touched={5} range={6:F2} body={7:F2} bodyRatio={8:F2}/{9:F2} closeLoc={10:F2}/{11:F2} directional={12} reclaim={13} closeOk={14} pass={15}",
                _activeSetup != null ? _activeSetup.Id : 0,
                barIndex,
                Bars.OpenTimes[barIndex],
                result.Mode,
                _activeSetup != null ? _activeSetup.Direction.ToString() : "n/a",
                result.TouchedRetestZone,
                result.BarRange,
                result.Body,
                result.BodyRatio,
                result.BodyThreshold,
                result.CloseLocation,
                result.CloseThreshold,
                result.Directional,
                result.BreakoutReclaimed,
                result.CloseOk,
                result.Passed);
        }

        private string BuildConfirmationMetricsDetail(ConfirmationCheckResult result)
        {
            return string.Format("setupId={0} dir={1} mode={2} touched={3} open={4:F2} high={5:F2} low={6:F2} close={7:F2} breakout={8:F2} range={9:F2} body={10:F2} bodyRatio={11:F2} minBody={12:F2} closeLoc={13:F2} maxCloseLoc={14:F2} directional={15} reclaim={16} closeStrength={17}",
                _activeSetup != null ? _activeSetup.Id : 0,
                _activeSetup != null ? _activeSetup.Direction.ToString() : "n/a",
                result.Mode,
                result.TouchedRetestZone,
                result.Open,
                result.High,
                result.Low,
                result.Close,
                _activeSetup != null ? _activeSetup.BreakoutLevel : 0,
                result.BarRange,
                result.Body,
                result.BodyRatio,
                result.BodyThreshold,
                result.CloseLocation,
                result.CloseThreshold,
                result.Directional,
                result.BreakoutReclaimed,
                result.CloseStrengthOk);
        }

        private void IncrementStat(string key)
        {
            int current;
            _stats.TryGetValue(key, out current);
            _stats[key] = current + 1;
        }

        private void PrintStatsSummary(string header)
        {
            Print("{0} | day={1:yyyy-MM-dd} tradesToday={2} lossesToday={3} netToday={4:F2}", header, _currentTradeDay, _tradesToday, _lossesToday, _closedNetToday);
            if (_stats.Count == 0)
            {
                Print("{0} | no stats collected yet", header);
                return;
            }

            foreach (var kv in _stats.OrderBy(k => k.Key))
                Print("STAT {0}={1}", kv.Key, kv.Value);
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
            if (DiagnosticMode)
                Print("RANGE_ATTEMPT | key={0} attempts={1}", rangeKey, attempts);
        }

        private double GetBreakBufferAtrEffective()
        {
            return ProbeMode ? Math.Max(0.01, BreakBufferAtr * 0.70) : BreakBufferAtr;
        }

        private double GetRetestZoneAtrEffective()
        {
            return ProbeMode ? RetestZoneAtr * 1.25 : RetestZoneAtr;
        }

        private ConfirmationModeKind GetConfirmationModeEffective()
        {
            ConfirmationModeKind mode;
            if (!Enum.TryParse(ConfirmationMode, true, out mode))
                mode = ConfirmationModeKind.Hybrid;

            if (ProbeMode && mode == ConfirmationModeKind.StrictBody)
                return ConfirmationModeKind.Hybrid;

            return mode;
        }

        private double GetConfirmationBodyMinEffective()
        {
            return GetConfirmationBodyMinEffective(GetConfirmationModeEffective());
        }

        private double GetConfirmationBodyMinEffective(ConfirmationModeKind mode)
        {
            if (mode == ConfirmationModeKind.WeakBodyProbe || mode == ConfirmationModeKind.ProbeConfirm)
                return ProbeConfirmationBodyMin;

            return ProbeMode ? Math.Max(0.15, ConfirmationBodyMin - 0.10) : ConfirmationBodyMin;
        }

        private double GetConfirmationClosePercentEffective()
        {
            return GetConfirmationClosePercentEffective(GetConfirmationModeEffective());
        }

        private double GetConfirmationClosePercentEffective(ConfirmationModeKind mode)
        {
            if (mode == ConfirmationModeKind.WeakBodyProbe || mode == ConfirmationModeKind.ProbeConfirm)
                return ProbeConfirmationClosePercent;

            return ProbeMode ? Math.Min(0.75, ConfirmationClosePercent + 0.10) : ConfirmationClosePercent;
        }

        private double GetMaxBreakoutCandleAtrEffective()
        {
            return ProbeMode ? MaxBreakoutCandleAtr * 1.35 : MaxBreakoutCandleAtr;
        }

        private double GetMinAtrEffective()
        {
            if (ShouldBypassAtrFilter())
                return 0.0;

            return ProbeMode ? Math.Max(0.0, MinAtrM15 * 0.70) : MinAtrM15;
        }

        private double GetMaxAtrEffective()
        {
            if (ShouldBypassAtrFilter())
                return 0.0;

            return ProbeMode && MaxAtrM15 > 0 ? MaxAtrM15 * 1.25 : MaxAtrM15;
        }

        private double GetMaxSpreadEffective()
        {
            return ProbeMode ? MaxSpread * 1.30 : MaxSpread;
        }

        private bool ShouldRelaxStopDistanceLimits()
        {
            if (ProbeMode && AllowStopDistanceRelaxInProbe)
                return true;

            if (DiagnosticMode && AllowStopDistanceRelaxInDiagnostic)
                return true;

            return false;
        }

        private string GetStopDistanceRelaxationContextLabel()
        {
            if (ProbeMode && AllowStopDistanceRelaxInProbe)
                return "ProbeModeRelaxed";

            if (DiagnosticMode && AllowStopDistanceRelaxInDiagnostic)
                return "DiagnosticModeRelaxed";

            return "Strict";
        }

        private double GetMinStopDistanceEffective()
        {
            if (ShouldRelaxStopDistanceLimits())
                return Math.Max(Symbol.PipSize, MinStopDistance * RelaxedStopDistanceMinMultiplier);

            return MinStopDistance;
        }

        private double GetMaxStopDistanceEffective()
        {
            if (ShouldRelaxStopDistanceLimits())
                return MaxStopDistance > 0 ? MaxStopDistance * Math.Max(1.0, RelaxedStopDistanceMaxMultiplier) : 0.0;

            return MaxStopDistance;
        }

        private StopDistanceCheck EvaluateStopDistance(int barIndex, double entryPrice, double stopPrice, double atr, double slBuffer)
        {
            double stopDistancePrice = Math.Abs(entryPrice - stopPrice);
            double stopDistancePips = Symbol.PipSize > 0 ? stopDistancePrice / Symbol.PipSize : 0.0;
            double stopDistanceTicks = Symbol.TickSize > 0 ? stopDistancePrice / Symbol.TickSize : 0.0;
            double baseMinPrice = MinStopDistance;
            double baseMaxPrice = MaxStopDistance;
            double effectiveMinPrice = GetMinStopDistanceEffective();
            double effectiveMaxPrice = GetMaxStopDistanceEffective();
            double baseMinPips = Symbol.PipSize > 0 ? baseMinPrice / Symbol.PipSize : 0.0;
            double baseMaxPips = Symbol.PipSize > 0 ? baseMaxPrice / Symbol.PipSize : 0.0;
            double effectiveMinPips = Symbol.PipSize > 0 ? effectiveMinPrice / Symbol.PipSize : 0.0;
            double effectiveMaxPips = Symbol.PipSize > 0 ? effectiveMaxPrice / Symbol.PipSize : 0.0;
            bool isValid = stopDistancePrice >= effectiveMinPrice && (effectiveMaxPrice <= 0 || stopDistancePrice <= effectiveMaxPrice);

            return new StopDistanceCheck
            {
                BarIndex = barIndex,
                EntryPrice = entryPrice,
                StopPrice = stopPrice,
                StopDistancePrice = stopDistancePrice,
                StopDistancePips = stopDistancePips,
                StopDistanceTicks = stopDistanceTicks,
                BaseMinPrice = baseMinPrice,
                BaseMaxPrice = baseMaxPrice,
                EffectiveMinPrice = effectiveMinPrice,
                EffectiveMaxPrice = effectiveMaxPrice,
                BaseMinPips = baseMinPips,
                BaseMaxPips = baseMaxPips,
                EffectiveMinPips = effectiveMinPips,
                EffectiveMaxPips = effectiveMaxPips,
                Atr = atr,
                SlBuffer = slBuffer,
                RelaxationContext = GetStopDistanceRelaxationContextLabel(),
                IsValid = isValid
            };
        }

        private void LogStopDistanceCheck(int barIndex, StopDistanceCheck stopCheck)
        {
            Print("STOP CHECK #{0} | bar={1} time={2:yyyy-MM-dd HH:mm} dir={3} entry={4:F2} stop={5:F2} distPrice={6:F2} distPips={7:F2} distTicks={8:F2} bandBasePrice=[{9:F2},{10:F2}] bandEffPrice=[{11:F2},{12:F2}] bandBasePips=[{13:F2},{14:F2}] bandEffPips=[{15:F2},{16:F2}] atr={17:F2} slBuffer={18:F2} context={19} valid={20}",
                _activeSetup != null ? _activeSetup.Id : 0,
                barIndex,
                Bars.OpenTimes[barIndex],
                _activeSetup != null ? _activeSetup.Direction.ToString() : "n/a",
                stopCheck.EntryPrice,
                stopCheck.StopPrice,
                stopCheck.StopDistancePrice,
                stopCheck.StopDistancePips,
                stopCheck.StopDistanceTicks,
                stopCheck.BaseMinPrice,
                stopCheck.BaseMaxPrice,
                stopCheck.EffectiveMinPrice,
                stopCheck.EffectiveMaxPrice,
                stopCheck.BaseMinPips,
                stopCheck.BaseMaxPips,
                stopCheck.EffectiveMinPips,
                stopCheck.EffectiveMaxPips,
                stopCheck.Atr,
                stopCheck.SlBuffer,
                stopCheck.RelaxationContext,
                stopCheck.IsValid);
        }

        private string BuildStopDistanceDetail(StopDistanceCheck stopCheck)
        {
            return string.Format("setupId={0} dir={1} entry={2:F2} stop={3:F2} stopDistance={4:F2} stopPips={5:F2} stopTicks={6:F2} baseBand=[{7:F2},{8:F2}] effectiveBand=[{9:F2},{10:F2}] baseBandPips=[{11:F2},{12:F2}] effectiveBandPips=[{13:F2},{14:F2}] atr={15:F2} slBuffer={16:F2} context={17}",
                _activeSetup != null ? _activeSetup.Id : 0,
                _activeSetup != null ? _activeSetup.Direction.ToString() : "n/a",
                stopCheck.EntryPrice,
                stopCheck.StopPrice,
                stopCheck.StopDistancePrice,
                stopCheck.StopDistancePips,
                stopCheck.StopDistanceTicks,
                stopCheck.BaseMinPrice,
                stopCheck.BaseMaxPrice,
                stopCheck.EffectiveMinPrice,
                stopCheck.EffectiveMaxPrice,
                stopCheck.BaseMinPips,
                stopCheck.BaseMaxPips,
                stopCheck.EffectiveMinPips,
                stopCheck.EffectiveMaxPips,
                stopCheck.Atr,
                stopCheck.SlBuffer,
                stopCheck.RelaxationContext);
        }

        private void LogOrderRequest(int barIndex, StopDistanceCheck stopCheck, double tpPips, RiskCalibrationResult riskPlan)
        {
            Print("ORDER REQUEST #{0} | bar={1} time={2:yyyy-MM-dd HH:mm} dir={3} sizingMode={4} fixedLots={5:F2} volume={6:F2} stopPips={7:F2} tpPips={8:F2} targetRisk={9:F2} expectedStopLoss={10:F2} rawLoss={11:F2} minVolLoss={12:F2} normRiskMult={13:F2} expectedEntry={14:F2} expectedStop={15:F2} stopDist={16:F2} context={17}",
                _activeSetup != null ? _activeSetup.Id : 0,
                barIndex,
                Bars.OpenTimes[barIndex],
                _activeSetup != null ? _activeSetup.Direction.ToString() : "n/a",
                riskPlan.SizingModeLabel,
                riskPlan.RequestedLots,
                riskPlan.NormalizedVolumeInUnits,
                stopCheck.StopDistancePips,
                tpPips,
                riskPlan.RiskAmount,
                riskPlan.EstimatedLossNormalized,
                riskPlan.EstimatedLossRaw,
                riskPlan.EstimatedLossAtMinVolume,
                riskPlan.NormalizedRiskMultiple,
                stopCheck.EntryPrice,
                stopCheck.StopPrice,
                stopCheck.StopDistancePrice,
                stopCheck.RelaxationContext);
        }

        private enum SizingModeKind
        {
            RiskBased,
            FixedLot
        }

        private enum AtrFilterModeKind
        {
            RawPrice,
            Pips,
            PercentOfPrice
        }

        private enum ConfirmationModeKind
        {
            StrictBody,
            DirectionalClose,
            WeakBodyProbe,
            Hybrid,
            ProbeConfirm
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

        private class ConfirmationCheckResult
        {
            public string Mode { get; set; }
            public bool Passed { get; set; }
            public string FailureCode { get; set; }
            public string FailureReason { get; set; }
            public int BarIndex { get; set; }
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public double BarRange { get; set; }
            public double Body { get; set; }
            public double BodyRatio { get; set; }
            public double BodyThreshold { get; set; }
            public double CloseLocation { get; set; }
            public double CloseThreshold { get; set; }
            public bool Directional { get; set; }
            public bool BreakoutReclaimed { get; set; }
            public bool CloseStrengthOk { get; set; }
            public bool BodyRangeOk { get; set; }
            public bool BodyOk { get; set; }
            public bool CloseOk { get; set; }
            public bool TouchedRetestZone { get; set; }
        }

        private class StopDistanceCheck
        {
            public int BarIndex { get; set; }
            public double EntryPrice { get; set; }
            public double StopPrice { get; set; }
            public double StopDistancePrice { get; set; }
            public double StopDistancePips { get; set; }
            public double StopDistanceTicks { get; set; }
            public double BaseMinPrice { get; set; }
            public double BaseMaxPrice { get; set; }
            public double EffectiveMinPrice { get; set; }
            public double EffectiveMaxPrice { get; set; }
            public double BaseMinPips { get; set; }
            public double BaseMaxPips { get; set; }
            public double EffectiveMinPips { get; set; }
            public double EffectiveMaxPips { get; set; }
            public double Atr { get; set; }
            public double SlBuffer { get; set; }
            public string RelaxationContext { get; set; }
            public bool IsValid { get; set; }
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
            public double VolumeInUnits { get; set; }
            public string SizingModeLabel { get; set; }
            public double RequestedLots { get; set; }
            public double TargetRiskAmount { get; set; }
            public double ExpectedLossAtStop { get; set; }
            public double EstimatedLossAtMinVolume { get; set; }
            public double ExpectedLossAtRawVolume { get; set; }
            public double StopDistancePips { get; set; }
            public double PipValuePerUnit { get; set; }
            public bool UsedMinVolumeConstraint { get; set; }
        }

        private class RiskCalibrationResult
        {
            public string SizingModeLabel { get; set; }
            public double RequestedLots { get; set; }
            public double RequestedVolumeInUnits { get; set; }
            public double NormalizedLots { get; set; }
            public bool IsFixedLotMode { get; set; }
            public double RiskAmount { get; set; }
            public double PipValuePerUnit { get; set; }
            public double RawVolumeInUnits { get; set; }
            public double NormalizedVolumeInUnits { get; set; }
            public double MinVolumeInUnits { get; set; }
            public double EstimatedLossRaw { get; set; }
            public double EstimatedLossNormalized { get; set; }
            public double EstimatedLossAtMinVolume { get; set; }
            public double NormalizedRiskMultiple { get; set; }
            public double MinVolumeRiskMultiple { get; set; }
            public bool NormalizedRiskWarning { get; set; }
            public bool MinVolumeRiskWarning { get; set; }
            public bool IsSizingMathValid { get; set; }
            public bool CanTrade { get; set; }
            public string RejectStatKey { get; set; }
            public string RejectReason { get; set; }
            public string RejectDetail { get; set; }

            public bool RawVolumeBelowMin
            {
                get { return RawVolumeInUnits < MinVolumeInUnits; }
            }
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
