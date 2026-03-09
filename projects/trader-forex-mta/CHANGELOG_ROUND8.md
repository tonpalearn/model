# CHANGELOG_ROUND8

## Summary
Round 8 adds an explicit operator-selectable sizing mode so the bot can stay on conservative risk-based sizing by default or switch to a manual fixed-lot entry size when the operator wants direct control.

## What changed
- Added `Sizing Mode` parameter with `RiskBased` as the conservative default.
- Added `Fixed Size (Lots)` parameter for manual lot selection in a cTrader-friendly format.
- Kept the existing stop-distance validation and order-submission flow intact.
- Extended sizing logs so both modes remain observable before order submission.
- In `FixedLot` mode the bot now:
  - logs the selected sizing mode
  - logs configured fixed lots and normalized broker volume
  - estimates expected stop-loss in account currency using current symbol pip data
  - emits a warning when the estimated loss is materially above nominal target risk using `Risk Calibration Warning Mult`
- Preserved broker min/max volume protections. Fixed-lot requests still normalize through symbol volume rules and reject cleanly if they fall outside broker limits.
- Extended post-trade telemetry so realized-vs-planned logs include sizing mode context.

## New / updated runtime log strings
- `SIZING CONFIG`
- `SIZING | mode=...`
- `FIXED LOT CHECK`
- `FIXED LOT RISK WARNING`
- `ORDER REQUEST` now includes `sizingMode=` and `fixedLots=`
- `ENTRY` now includes `sizingMode=` and `fixedLots=`
- `REALIZED VS PLAN` now includes `sizingMode=` and `fixedLots=`

## Operator note
This round is for safer operator control and clearer diagnostics, not for making profit claims. `RiskBased` remains the default starting point. Use `FixedLot` only when you intentionally want manual size selection and are checking the loss estimates in the log.