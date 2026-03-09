# SETUP.md

## ภาพรวม
โปรเจกต์นี้เป็น cTrader/cAlgo cBot เวอร์ชันแรกสำหรับ `XAUUSD` ตาม setup หลัก:

- H1 bias
- M15 execution
- Breakout + Retest + Confirmation
- เน้น deterministic rules และ risk guard มากกว่าการ optimize ให้ดูดี

> หมายเหตุ: ในสภาพแวดล้อมนี้ยังไม่มีการยืนยัน compile ด้วย cTrader Automate โดยตรง จึงส่งมอบเป็นโค้ด C# สไตล์ cAlgo ที่จัดโครงไว้ให้ import/run ต่อใน cTrader ได้

---

## โครงสร้างไฟล์

```text
src/
  MtaGoldBreakoutRetestBot/
    MtaGoldBreakoutRetestBot.cs
SETUP.md
IMPLEMENTATION_MAP.md
DEV_NOTES.md
```

---

## วิธีนำเข้าใน cTrader

1. เปิด **cTrader**
2. ไปที่ **Automate**
3. สร้าง cBot ใหม่ชื่อใกล้เคียงกับ `MtaGoldBreakoutRetestBot`
4. แทนที่โค้ดเดิมด้วยไฟล์จาก:
   - `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`
5. กด Build/Compile ใน cTrader
6. แนบ cBot กับกราฟ `XAUUSD` บน timeframe `M15`
7. ตรวจค่า TimeZone ของ cBot และ session inputs ให้ตรงกับ broker/server time

---

## ค่าตั้งต้นสำคัญ

- Symbol Name: `XAUUSD`
- Bias TimeFrame: `Hour`
- Range Lookback M15: `8`
- Bias Lookback H1: `20`
- Swing Strength: `2`
- ATR Period M15: `14`
- Enable ATR Filter: `false`
- ATR Filter Mode: `RawPrice`
- Min ATR M15: `0.0`
- Max ATR M15: `60.0`
- Bypass ATR In Diagnostic: `true`
- Bypass ATR In Probe: `true`
- Break Buffer ATR: `0.10`
- Retest Zone ATR: `0.30`
- Retest Timeout Bars: `6`
- Risk %: `0.25`
- Max Trades / Day: `2`
- Max Losses / Day: `2`
- Daily Loss Cap %: `1.0`
- Break Even At R: `1.0`
- Take Profit R: `2.0`
- Max Bars In Trade: `8`

---

## สิ่งที่ต้องตรวจทันทีหลัง import

### 1) Time zone / session alignment
ตัวบอทใช้ `[Robot(TimeZone = TimeZones.UTC)]`
ดังนั้นค่าพารามิเตอร์ session เช่น London/NY ต้อง map ให้ตรงกับเวลา UTC หรือปรับโค้ดให้สอดคล้องกับ server time ที่ใช้จริงในการทดสอบ

### 2) Pip/point behavior ของ XAUUSD
โบรกเกอร์แต่ละรายอาจแสดง XAUUSD ต่างกัน เช่น:
- 0.01 = 1 point
- pip value / tick value / contract sizing ต่างกัน
- minimum volume ต่างกันมาก

ดังนั้น Test Agent ควรตรวจ:
- `Symbol.PipSize`
- `Symbol.PipValue`
- `Symbol.TickValue`
- `Symbol.LotSize`
- `Symbol.VolumeInUnitsMin`
- volume normalization

Round 7 เปลี่ยน risk calibration ให้ยึด `Symbol.PipValue` ตรงในการประเมิน expected stop loss แล้ว ดังนั้นค่าใน `SYMBOL SPEC` และ `RISK CALIBRATION` สำคัญมากกว่าก่อนหน้า

### 3) Stop distance practicality
ค่าตั้งต้น `MinStopDistance` และ `MaxStopDistance` เป็นค่าเชิงตรรกะเริ่มต้น ไม่ใช่ค่าที่ validate แล้วว่าดีที่สุดกับทุก broker

รอบ 6 เพิ่ม log และพารามิเตอร์สำหรับ stop-distance โดยตรง:
- `Allow Stop Relax In Probe`
- `Allow Stop Relax In Diagnostic`
- `Relaxed Stop Max Multiplier`
- `Relaxed Stop Min Multiplier`

รอบ 7 เพิ่ม risk-calibration observability ต่อจาก stop-distance โดยตรง:
- `Risk Calibration Warning Mult`
- log `RISK CALIBRATION`
- log `SIZING` ที่มี `rawLoss / normLoss / minVolLoss`
- reject แบบ explicit: `Broker min volume exceeds target risk`
- log หลังปิด order: `REALIZED VS PLAN`

ถ้าต้องการ debug downstream execution โดยไม่ผ่อน normal mode:
- ใช้ `Probe Mode = true`
- เปิด `Allow Stop Relax In Probe = true`
- ดู log `STOP DISTANCE LIMITS`, `STOP CHECK`, `RISK CALIBRATION`, `SIZING`, `ORDER REQUEST`, `REALIZED VS PLAN`

---

## วิธีทดสอบเบื้องต้น

1. Backtest บน `XAUUSD M15`
2. ใช้ข้อมูลที่มี H1 history ครบ
3. ตรวจ log ว่า setup ถูก block/reject ด้วยเหตุผลอะไรบ้าง
4. เริ่มจากช่วงเวลาแคบก่อน เช่น 3-6 เดือน
5. แยกผลตาม session:
   - London only
   - NY only
   - London + NY

---

## ข้อควรระวัง

- โค้ดนี้เป็น baseline implementation ไม่ใช่ระบบที่ยืนยันผลแล้ว
- ยังไม่มี external news filter
- ยังไม่ได้ทำ partial close / trailing stop
- ยังไม่ได้ยืนยัน compile/runtime quirks กับ cTrader version เฉพาะเครื่อง
- ห้ามตีความว่า strategy นี้ “ทำกำไรได้แน่นอน”


## ATR filter (รอบ 4)

รอบ 4 ปรับ ATR filter สำหรับ XAUUSD โดยตรง เพื่อไม่ให้เกิดอาการบอทถูกบล็อกทั้งระบบเพราะใช้ค่า ATR raw กับ threshold ที่แคบเกินจริง

### พารามิเตอร์ใหม่
- `Enable ATR Filter`
  - `false` = ปิด ATR gate ทั้งหมด
  - ใช้ค่านี้ตอนต้องการดูว่า pipeline ถัดจาก ATR ทำงานหรือไม่
- `ATR Filter Mode`
  - `RawPrice` = ใช้ ATR เป็นหน่วยราคาโดยตรง
  - `Pips` = แปลง ATR เป็น pip ตาม `Symbol.PipSize`
  - `PercentOfPrice` = แปลง ATR เป็น % ของราคาปิดแท่ง
- `Bypass ATR In Diagnostic`
  - ถ้า `true` และเปิด `Diagnostic Mode` อยู่ ATR gate จะไม่บล็อก order flow แต่จะยัง log ค่า ATR ให้เห็น
- `Bypass ATR In Probe`
  - ถ้า `true` และเปิด `Probe Mode` อยู่ ATR gate จะไม่บล็อก order flow เช่นกัน

### ค่าเริ่มต้นที่แนะนำ
#### โหมดตรวจ flow downstream
- `Enable ATR Filter = false`
- `Diagnostic Mode = true`
- `Probe Mode = false` หรือ `true` ตามที่ต้องการ

#### โหมดเปิด ATR แบบ realistic สำหรับ XAUUSD
- `Enable ATR Filter = true`
- `ATR Filter Mode = RawPrice`
- `Min ATR M15 = 0.0`
- `Max ATR M15 = 60.0`
- ถ้าต้องการให้ ATR มีผลจริงในรอบทดสอบนี้ ให้ตั้ง
  - `Bypass ATR In Diagnostic = false`
  - `Bypass ATR In Probe = false`

### วิธีอ่าน log
จะมี log ลักษณะนี้:
- `ATR FILTER | enabled=... mode=... min=... max=... unit=... compare=...`
- `GATE ATR PASS | ... raw=... compare=... unit=... mode=... threshold=[min,max] ...`
- ถ้า bypass จะเห็น `filter=bypassed context=...`
- ถ้าปิด filter จะเห็น `filter=disabled`

ข้อสำคัญ: รอบนี้เป้าหมายคือให้บอทผ่าน ATR gate เพื่อเปิดทางไปดู breakout / retest / confirm / sizing ต่อ ไม่ใช่สรุปผลกำไร
