# FIX_LIST.md

อัปเดตล่าสุด: 2026-03-09
สถานะ: Prioritized Fixes Before Trusting Backtest

## P0 — ต้องจัดการก่อนเชื่อผล backtest

### 1) แก้ H1 bias logic ให้ตรงสเปกมากขึ้น
ปัญหา:
- `CalculateH1Bias()` ยังตรวจ “swing intact” แบบอ่อนเกินไป
- เสี่ยงตีความโครงสร้างผิด

ควรทำ:
- ตรวจว่า swing low / swing high ถูกทำลายหรือไม่แบบ sequence-aware
- หรือออกแบบ structure model HH/HL/LH/LL ที่ deterministic กว่าเดิม
- เพิ่ม debug log ของ:
  - midpoint
  - last swing high/low
  - เหตุผลที่ bias กลายเป็น bullish/bearish/neutral

เหตุผลที่เป็น P0:
- ถ้า bias ผิด ทุกสถิติ downstream จะไม่น่าเชื่อถือ

---

### 2) ยืนยัน/แก้ volume sizing สำหรับ XAUUSD broker จริง
ปัญหา:
- `CalculateVolumeInUnits()` อาจผิดหน่วยได้ง่าย

ควรทำ:
- ทดสอบกับ broker target จริง
- log symbol spec และตัวเลข sizing ทุกดีล
- ถ้า API มีวิธีคำนวณ risk-based volume ที่ปลอดภัยกว่า ให้ใช้
- ตรวจ volume normalization หลัง round down ว่ายังอยู่ใน risk tolerance

เหตุผลที่เป็น P0:
- ถ้า sizing ผิด backtest ทั้งหมดแทบใช้ไม่ได้

---

### 3) เปลี่ยน daily loss cap ให้ล็อกตาม equity ต้นวัน
ปัญหา:
- ตอนนี้ใช้ equity ปัจจุบันขณะตรวจ

ควรทำ:
- เพิ่มตัวแปร `startOfDayEquity`
- reset ตอนเปลี่ยน trading day
- ใช้ threshold คงที่ทั้งวัน

เหตุผลที่เป็น P0:
- risk guard รายวันต้อง deterministic

---

### 4) Compile และ run sanity test ใน cTrader จริง
ปัญหา:
- ตอนนี้ยังไม่ยืนยัน compile/runtime จริง

ควรทำ:
- build ใน cTrader Automate
- ตรวจ event handlers, MarketData.GetBars, ExecuteMarketOrder, ModifyPosition
- บันทึก compile/runtime issues แยกจาก logic issues

เหตุผลที่เป็น P0:
- ถ้ายัง compile ไม่ผ่าน เรื่องอื่นยังไปต่อไม่ได้

---

## P1 — สำคัญมาก ควรทำก่อน optimization รอบจริง

### 5) เพิ่ม detailed logging / export สำหรับ audit
ควร log:
- bias result
- range high/low
- breakout level/buffer
- retest zone
- stop distance / pips
- risk amount
- raw/normalized volume
- spread ตอน entry
- BE modify success/failure
- reject/invalidate reason

เหตุผล:
- ถ้าไม่มี log ที่ละเอียด จะ debug จาก equity curve อย่างเดียวไม่ได้

---

### 6) ตัดสินใจเรื่อง confirmation bar ให้ชัด
ปัญหา:
- ตอนนี้แท่งเดียวกันสามารถทั้งแตะ retest zone และ confirm ได้

ควรทำ:
- ตัดสินใจว่าอนุญาตหรือไม่
- ถ้าไม่อนุญาต ให้แยก state เป็น:
  - retest seen
  - wait next closed candle for confirmation

เหตุผล:
- มีผลต่อ character ของระบบพอสมควร

---

### 7) Enforce กฎห้าม re-enter range เดิมเกิน 1 ครั้ง
ปัญหา:
- ปัจจุบันยังไม่มี persistent range memory จริง

ควรทำ:
- เก็บ identifier ของ range เดิม
- block re-entry หลัง stop/close/invalidate ตามกฎที่ต้องการ
- เพิ่ม cooldown logic ถ้าจำเป็น

เหตุผล:
- ลด overtrading ในตลาด chop

---

### 8) กำหนด timezone / trading-day convention ให้ตายตัว
ควรทำ:
- ระบุชัดว่าใช้ UTC ทั้งระบบหรือใช้ broker/server mapping
- ผูก daily reset, session filter, Friday cutoff กับ convention เดียวกัน
- บันทึก convention ในรายงานผลทุกครั้ง

เหตุผล:
- ไม่อย่างนั้นผล backtest เปรียบเทียบกันไม่ได้

---

## P2 — ควรทำเพื่อเพิ่มความน่าเชื่อถือและลดความคลุมเครือ

### 9) ทบทวน `MinRewardRisk` ว่าจะลบหรือ implement จริง
ปัญหา:
- ตอนนี้ไม่ได้วัด reward/risk ของ setup จริง

ควรทำอย่างใดอย่างหนึ่ง:
- ลบออกชั่วคราว
- หรือ implement target proxy/structure target จริง

---

### 10) เพิ่ม logging เมื่อ BE modify ไม่สำเร็จ
ปัญหา:
- ตอนนี้เงียบเกินไปถ้า `ModifyPosition()` fail

ควรทำ:
- log error/result code
- บันทึก stop-level constraint ถ้าดึงได้

---

### 11) ตรวจหน่วยของ stop-related parameters ทั้งชุด
รายการที่ต้อง verify:
- `MinStopDistance`
- `MaxStopDistance`
- `FixedMinBreakBuffer`
- `FixedMinSlBuffer`
- `BreakEvenOffset`
- `MaxSpread`

เหตุผล:
- ทั้งหมด sensitive มากกับ quote format ของทอง

---

### 12) ทำ scenario checklist สำหรับ visual validation
ควรมีชุดภาพ/ตัวอย่างอย่างน้อย:
- valid breakout
- no retest timeout
- bias flip invalidate
- spread reject
- stop too wide reject
- BE moved
- time exit

เหตุผล:
- ช่วยแยก “โค้ดทำตามกฎไหม” ออกจาก “กำไรไหม”

---

## P3 — ปรับปรุงคุณภาพระบบระยะถัดไป

### 13) แยก helper classes เพื่อ maintainability
เช่น:
- BiasCalculator
- SetupDetector
- RiskManager
- SessionFilter
- TradeManager

---

### 14) เพิ่ม structured trade journal export
เช่น CSV/JSON:
- setup id
- timestamps
- state transitions
- entry/exit prices
- R multiple
- reject reason

---

### 15) พิจารณา external news blackout ในเวอร์ชันถัดไป
ยังไม่จำเป็นสำหรับ baseline แต่มีประโยชน์มากกับ XAUUSD หากระบบแสดงความเปราะต่อข่าวแรง

---

## บทสรุปสั้น

ถ้าต้องเลือกแก้ไม่กี่อย่างก่อน backtest รอบจริง ให้เริ่มจาก:
1. Bias H1
2. Volume sizing
3. Daily loss cap
4. Compile/runtime verification
5. Detailed logging

ห้าเรื่องนี้สำคัญสุด เพราะถ้ายังไม่ชัด ต่อให้ได้ผล backtest สวยก็ยังเชื่อไม่ได้มากนัก
