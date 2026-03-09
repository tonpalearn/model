# IMPLEMENTATION_MAP.md

## Strategy
`MTA_GOLD_BreakoutRetestConfirm_v0.1`

## ไฟล์หลัก
- `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`

---

## Rule Mapping

### 1) H1 Bias / Market Structure
**Spec:** ใช้ H1 midpoint + break ล่าสุดของ swing high/low

**Code:**
- `CalculateH1Bias()`
- `FindLastSwingHigh()`
- `FindLastSwingLow()`

**หมายเหตุ:**
- ใช้ fractal-style swing ตาม `SwingStrength`
- เลี่ยงใช้แท่ง H1 ที่ยังไม่ปิด โดยอ้างอิง `lastClosedIndex = _h1Bars.Count - 2`

---

### 2) M15 Setup Range
**Spec:** ใช้ high/low ของ M15 ย้อนหลัง 8 แท่งล่าสุด ไม่นับแท่งปัจจุบัน

**Code:**
- `TryArmBreakout()`

**หมายเหตุ:**
- range ถูกคำนวณจาก `lastClosedIndex - RangeLookbackM15` ถึง `lastClosedIndex - 1`

---

### 3) Valid Breakout
**Spec:** close ทะลุ range พร้อม buffer ขั้นต่ำ และ breakout candle ไม่ใหญ่ผิดปกติ

**Code:**
- `TryArmBreakout()`
- ใช้ `BreakBufferAtr`, `FixedMinBreakBuffer`, `MaxBreakoutCandleAtr`

---

### 4) Retest Logic
**Spec:** รอ retest ภายใน `RetestTimeoutBars`

**Code:**
- `AdvanceOrInvalidateSetup()`
- `TryProcessRetestAndConfirmation()`

**หมายเหตุ:**
- setup ถูก invalid ถ้า timeout หรือปิดทะลุฝั่งตรงข้ามของ range

---

### 5) Confirmation Candle
**Spec:** ต้องมี bar close confirmation หลังแตะ retest zone

**Code:**
- `TryProcessRetestAndConfirmation()`

**ค่าที่ใช้:**
- `ConfirmationBodyMin`
- `ConfirmationClosePercent`

---

### 6) Entry
**Spec:** เข้า market order หลัง confirmation

**Code:**
- `ExecuteConfirmedEntry()`

**หมายเหตุ:**
- implementation นี้ยิง market order จาก event ของ bar close
- practical fill จะใกล้กับการเข้าแท่งถัดไป แต่ยังขึ้นกับ engine/backtest model ของ cTrader

---

### 7) Stop Loss / Take Profit
**Spec:** SL อยู่หลัง confirmation/retest extreme + buffer, TP แบบ fixed R

**Code:**
- `ExecuteConfirmedEntry()`

**ค่าที่ใช้:**
- `SlBufferAtr`
- `FixedMinSlBuffer`
- `MinStopDistance`
- `MaxStopDistance`
- `TakeProfitR`

---

### 8) Break-even Rule
**Spec:** เมื่อกำไรถึง +1R ให้ย้าย SL ไป BE

**Code:**
- `ManageOpenTrade()`

**ค่าที่ใช้:**
- `BreakEvenAtR`
- `BreakEvenOffset`

---

### 9) Time Exit
**Spec:** ถ้าเกิน `MaxBarsInTrade` ให้ปิด

**Code:**
- `ManageOpenTrade()`

---

### 10) Risk Sizing
**Spec:** fixed-fractional risk ต่อเทรด

**Code:**
- `CalculateVolumeInUnits()`

**หมายเหตุ:**
- ใช้ `Account.Equity`, `Symbol.PipValue`, `Symbol.LotSize`
- จุดนี้ Test Agent ต้องตรวจให้ละเอียดกับ broker จริง

---

### 11) Daily Risk Guards
**Spec:** จำกัดจำนวนเทรด/วัน, จำนวน loss/วัน, daily loss cap

**Code:**
- `CanOpenNewTrade()`
- `ResetDailyCountersIfNeeded()`
- `OnPositionClosed()`

---

### 12) Session / No-trade Filters
**Spec:** เทรดเฉพาะ London/NY early session, หลีกเลี่ยง Friday cutoff, spread/ATR ผิดปกติ

**Code:**
- `IsEntryWindow()`
- `IsHourWithin()`
- `IsSpreadTooHigh()`
- `IsAtrTradable()`

---

## State Flow ที่ implement

1. ไม่มี setup active → `TryArmBreakout()`
2. breakout valid → `_activeSetup.State = BreakoutArmed`
3. ราคาแตะ retest zone → `_activeSetup.State = RetestObserved`
4. confirmation ผ่าน → `ExecuteConfirmedEntry()`
5. position active → `ManageOpenTrade()`
6. ปิด order / invalidation / timeout → reset state

---

## Known Deviations / Approximation

1. bias logic ใน spec เป็นแนวคิดเชิงโครงสร้าง แต่ implementation รุ่นนี้ simplify ให้ deterministic มากขึ้น
2. rule “ห้าม re-enter setup เดิมเกิน 1 ครั้ง” ยังไม่ได้ทำเป็น persistent range memory เต็มรูปแบบ
3. filter เรื่อง reward-to-next-structure target ยังใช้ค่า `MinRewardRisk` แบบ simplified แทน adaptive structure target
4. daily loss cap คำนวณจาก equity ปัจจุบันขณะตรวจ ไม่ได้ lock equity ตอนเริ่มวัน
5. ไม่มี news blackout จาก external calendar
