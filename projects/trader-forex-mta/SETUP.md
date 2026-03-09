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
- Break Buffer ATR: `0.15`
- Retest Zone ATR: `0.20`
- Retest Timeout Bars: `4`
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
- pip value / lot size ต่างกัน

ดังนั้น Test Agent ควรตรวจ:
- `Symbol.PipSize`
- `Symbol.PipValue`
- `Symbol.LotSize`
- volume normalization

### 3) Stop distance practicality
ค่าตั้งต้น `MinStopDistance` และ `MaxStopDistance` เป็นค่าเชิงตรรกะเริ่มต้น ไม่ใช่ค่าที่ validate แล้วว่าดีที่สุดกับทุก broker

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
