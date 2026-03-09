# CODE_AUDIT_MTA_ALIGNMENT

Date: 2026-03-09
Scope: Audit current cTrader bot and docs against the originally intended **MTA-inspired** framing.

> Important boundary: this audit does **not** claim the bot clones MTA. It evaluates whether the project still matches the project’s own documented interpretation of MTA’s public framing: **daily gold analysis + actionable setup + confirmation before entry**.

---

## 1) Files reviewed

### Research / framing
- `MTA_DEEP_RESEARCH.md`
- `MTA_TEXT_VS_VIDEO_MAP.md`
- `MTA_CONCEPT_SIGNALS.md`
- `SETUP_CANDIDATES.md`
- `STRATEGY_SPEC.md`
- `PROJECT_BRIEF.md`

### Strategy-to-code / implementation docs
- `IMPLEMENTATION_MAP.md`
- `DEV_NOTES.md`
- `SETUP.md`
- `PARAMETER_PRESETS.md`
- `TEST_REVIEW.md`

### Change history reviewed for drift
- `CHANGELOG_ROUND4.md`
- `CHANGELOG_ROUND5.md`
- `CHANGELOG_ROUND6.md`
- `CHANGELOG_ROUND7.md`
- `CHANGELOG_ROUND8.md`
- commit `800b8e7` diff (Round 8)

### Current bot code
- `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`

---

## 2) What the original intended strategic core actually was

## Evidence from the research docs
The strongest evidence from public MTA-facing material is limited but consistent:
- MTA presents itself as **Forex education**
- MTA says it does **daily gold chart analysis**
- MTA uses **signal-room framing**

That supports a conservative project interpretation of:
- **XAUUSD-first** focus
- **chart-based / price-action-friendly** logic
- **actionable setup + confirmation before entry**
- a bot that is explainable on a chart, not indicator-heavy and not black-box

## Evidence from the project’s own strategy docs
The project then formalized that into a specific first implementation:
- **Primary setup:** Breakout + Retest + Confirmation
- **Bias timeframe:** H1
- **Execution timeframe:** M15
- **Only trade with higher-timeframe bias**
- **Wait for retest** after breakout
- **Wait for a bar-close confirmation** before entry
- **Conservative risk posture**
- **Session-bounded intraday trading**
- **Simple structure-based stop, fixed-R target, BE, and daily guards**

## What was explicitly *not* strong MTA evidence
The docs are also careful that these are project choices, not proven MTA facts:
- H1/M15 specifically
- fixed `2R` TP and `1R` break-even
- exact ATR thresholds
- exact stop-distance bands
- exact confirmation ratios
- exact broker sizing model
- probe / diagnostic / fixed-lot operator tooling

That distinction matters for this audit.

---

## 3) Audit of the current bot: where it is still aligned

## Still aligned with the intended MTA-inspired core

These parts still match the project’s original strategic framing well:

### A. Gold-first, chart-first, setup-first design
Evidence:
- `TargetSymbolName` default is `XAUUSD`
- logic is still price-action / OHLC-driven
- no EMA/VWAP/news-indicator system took over the strategy

Conclusion:
- This is still honestly a **gold-focused, chart-based system** rather than a generic execution shell.

### B. Breakout -> retest -> confirmation remains the main entry engine
Evidence in code:
- `TryArmBreakout()`
- `TryProcessRetestAndConfirmation()`
- `ExecuteConfirmedEntry()`

Conclusion:
- The core entry sequence is still the same one selected in `SETUP_CANDIDATES.md` and `STRATEGY_SPEC.md`.

### C. Multi-timeframe bias is still central
Evidence in code:
- `CalculateH1Bias()`
- H1 midpoint + swing break + intact opposite swing logic

Conclusion:
- The bot still trades as a structured intraday continuation system, not as a random breakout bot.

### D. Conservative trade gating still exists
Evidence in code/docs:
- session windows
- spread gate
- ATR gate (even if now often bypassed)
- max trades/day
- max losses/day
- daily loss cap
- one active position at a time

Conclusion:
- The system still expresses a conservative, guarded posture.

### E. Structure-based stop + fixed-R management still match the project spec
Evidence:
- stop built from confirmation/retest extreme + buffer
- TP = `TakeProfitR`
- BE after `BreakEvenAtR`
- time exit after `MaxBarsInTrade`

Conclusion:
- This is still recognizably the original v0.1 system architecture.

---

## 4) Reasonable engineering/debug additions that are useful but not really “MTA-inspired”

These additions are not strategic evidence about MTA. They are engineering layers added during live debugging, and most of them are reasonable.

### A. ATR observability and bypass controls (Round 4)
Examples:
- `Enable ATR Filter`
- `ATR Filter Mode`
- `Bypass ATR In Diagnostic`
- `Bypass ATR In Probe`
- expanded ATR logs

Assessment:
- Good engineering for XAUUSD calibration
- not an MTA-style insight
- should be treated as an execution/debug wrapper, not part of the strategy identity

### B. Multiple confirmation modes and confirmation trace tooling (Round 5)
Examples:
- `StrictBody`
- `DirectionalClose`
- `WeakBodyProbe`
- `Hybrid`
- `ProbeConfirm`
- `CONFIRM TRACE`

Assessment:
- Very useful for diagnosing why the pipeline blocks
- but this is already a mini confirmation framework, not a single clean strategy rule set
- not something the research on MTA supports directly

### C. Stop-distance diagnostics and relaxed probe bands (Round 6)
Examples:
- stop-distance object/logging
- explicit strict vs relaxed contexts
- `Allow Stop Relax In Probe`
- `Allow Stop Relax In Diagnostic`
- relaxed multipliers

Assessment:
- sensible broker-facing hardening
- definitely engineering realism, not MTA-inspired concept logic

### D. Risk calibration / realized-vs-plan telemetry (Round 7)
Examples:
- `RISK CALIBRATION`
- `RISK WARNING`
- `REALIZED VS PLAN`
- broker-min-volume safety reject

Assessment:
- strong addition from a safety and honesty perspective
- belongs to the execution/risk layer, not the strategy thesis

### E. Fixed-lot mode (Round 8)
Examples:
- `Sizing Mode`
- `Fixed Size (Lots)`
- fixed-lot risk warnings

Assessment:
- practical operator control for cTrader
- not MTA-inspired strategy logic
- also not inherently bad, but it moves the system further toward a configurable trading tool and away from a single defined strategy expression

---

## 5) Where drift or overfitting has likely appeared

## A. The codebase now has two identities mixed together
1. **Core strategy**: H1 bias + M15 breakout/retest/confirmation
2. **Diagnostic execution framework**: probe mode, diagnostic mode, bypasses, relaxed bands, alternate confirmation modes, fixed-lot mode, near-miss logging, rich telemetry

Assessment:
- This is the main architectural drift.
- The strategy itself did not disappear.
- But it is now wrapped in enough calibration machinery that the project can become hard to describe cleanly.

## B. Current defaults have drifted away from the original spec baseline
Evidence: the code defaults are materially softer / different than the spec defaults in several places.

Examples:
- `Break Buffer ATR`: spec `0.15` vs code `0.10`
- `Retest Zone ATR`: spec `0.20` vs code `0.30`
- `Retest Timeout Bars`: spec `4` vs code `6`
- `ConfirmationBodyMin`: spec `0.50` vs code `0.35`
- `ConfirmationClosePercent`: spec `0.35` vs code `0.45`
- `MaxBreakoutCandleATR`: spec `2.0` vs code `2.5`
- ATR gate is now default-disabled in code
- `Allow Same-Bar Retest Confirm`: code default `true`

Assessment:
- This is evidence of behavioral drift from the original “clean conservative baseline” toward a more permissive and test-through-the-pipeline posture.
- Some changes are justified by runtime evidence.
- But the docs do not yet present a clean split between **baseline strategy defaults** and **debug defaults**.

## C. Probe/diagnostic modes now change real signal behavior, not just logging
Evidence in code:
- `GetBreakBufferAtrEffective()` reduces breakout strictness in probe mode
- `GetRetestZoneAtrEffective()` widens retest zone in probe mode
- `GetConfirmationBodyMinEffective()` loosens confirmation in probe mode
- `GetConfirmationClosePercentEffective()` loosens close quality in probe mode
- `GetMaxBreakoutCandleAtrEffective()` loosens breakout candle limit
- `GetMaxSpreadEffective()` loosens spread gate
- ATR and stop-distance may also be bypassed/relaxed

Assessment:
- This is more than observability.
- It is a **parameter-transforming alternate behavior layer**.
- Useful for diagnosis, but also the clearest source of possible overfitting if used casually in testing.

## D. Confirmation has become the biggest strategy-drift surface
The original spirit was: wait for a clear confirming bar.
The current code supports five interpretations of what “confirmation” means.

Assessment:
- This helps answer debugging questions.
- But if left architecturally undifferentiated, it makes the bot less like “one MTA-inspired strategy” and more like “a family of related confirmation experiments.”

## E. Fixed-lot mode can dilute the conservative programmable-risk framing
The original posture was strongly risk-based and conservative.
Fixed lot adds operator freedom, which is practical, but:
- it can decouple entry size from the nominal risk target
- it shifts more responsibility to the operator
- it makes it easier to run the strategy as a semi-manual tool rather than a disciplined risk-normalized system

Assessment:
- acceptable as an explicit operator mode
- mild drift if presented as part of the core strategy rather than as an optional execution profile

## F. Some docs are now stale relative to current code
Examples:
- `IMPLEMENTATION_MAP.md` still mentions `Symbol.PipValue` + `Symbol.LotSize` under sizing notes, while Round 7 intentionally changed the calibration basis
- `STRATEGY_SPEC.md` still reads like the clean baseline, but current code defaults are noticeably different
- older review docs describe issues already partly fixed, so readers can misjudge current state

Assessment:
- this is documentation drift, not just code drift
- it makes the project look less architecturally honest than it actually is

---

## 6) Missing pieces if the goal is to feel closer to the original MTA-style framing

These are not proven MTA rules. They are gaps relative to the project’s own intended framing.

### A. A clearer “daily analysis / key level” layer
Current implementation is heavily rule-driven from a rolling M15 range.
What is somewhat missing versus the MTA-inspired narrative is a more explicit daily-plan layer such as:
- daily key level framing
- explicit scenario states for buy/sell map
- stronger separation between analysis phase and execution phase

This is an inference, not direct evidence about MTA.

### B. A single canonical baseline profile
Right now the code is one bot with many behavior toggles.
What is missing is an explicit, protected baseline profile such as:
- `CoreBaseline`
- `Diagnostic`
- `Probe`
- `FixedLotOperator`

That would make it much easier to say which behavior is the strategy and which behavior is scaffolding.

### C. Stronger enforcement of one-range / one-attempt discipline
The docs have long acknowledged this as only partially enforced.
Current code does have `RangeKey` and attempt counting, which is progress, but the project still frames this as an area needing careful interpretation and review.

### D. Cleaner documentation of “project inference” vs “runtime necessity” vs “operator override”
This is now needed more than more code changes.

---

## 7) Bottom-line judgment

## Is the current bot still honestly describable as MTA-inspired?
**Yes — with an important qualifier.**

Accurate description:
- It is still an **MTA-inspired XAUUSD cTrader bot** at the level of **public framing and strategic skeleton**:
  - gold-focused
  - chart-based
  - intraday
  - breakout/retest/confirmation
  - conservative guards

Inaccurate description:
- It would be misleading to describe the full current implementation stack — especially probe/diagnostic behavior, sizing calibration, and fixed-lot mode — as “MTA-inspired logic.”

More honest phrasing:
- **MTA-inspired strategy core with substantial broker/debug/execution scaffolding added during live calibration**.

That wording best fits the evidence.

---

## 8) Recommended next actions

## Top 3 actions

### 1) Split the project conceptually into **Core Strategy** vs **Execution/Debug Scaffolding**
Recommended implementation/documentation split:
- **Core Strategy Profile**
  - one canonical confirmation mode
  - strict/default breakout/retest/confirmation thresholds
  - risk-based sizing only
  - no probe relaxations
- **Diagnostic Profile**
  - observability on
  - controlled bypasses/relaxations
- **Operator FixedLot Profile**
  - explicitly marked as manual-size override

This is the single highest-value architectural cleanup.

### 2) Freeze and document a canonical “baseline MTA-inspired” parameter set
Create one clearly named baseline in docs and maybe code comments, using a deliberately conservative set near the original strategy intent.
Then separately document which defaults were changed during Rounds 4–8 for debugging practicality.

Without this, the project keeps drifting because the live-debug defaults become mistaken for the intended strategy.

### 3) Update mapping docs so they explicitly mark drift and non-core layers
At minimum, refresh:
- `IMPLEMENTATION_MAP.md`
- maybe `STRATEGY_SPEC.md` or add an addendum

Specifically mark:
- core rules
- debug-only relaxations
- operator overrides
- current code defaults that differ from the original spec defaults

---

## 9) Final assessment in one paragraph

The current bot has **not drifted so far that it stops being MTA-inspired**; the strategic backbone is still the originally chosen breakout-retest-confirmation system for XAUUSD with H1 bias and conservative session/risk controls. The real drift is architectural: repeated debugging rounds turned the bot into a hybrid of **strategy implementation + diagnostic laboratory + broker-calibration harness + optional manual sizing tool**. That is mostly reasonable engineering, but it now needs a cleaner separation so the project can stay honest about what is truly part of the MTA-inspired thesis and what is simply necessary scaffolding for real cTrader/XAUUSD execution.
