# HARDENING_NOTES.md

อัปเดตล่าสุด: 2026-03-09
สถานะ: Round 2 Hardening สำหรับ `MtaGoldBreakoutRetestBot`

เอกสารนี้สรุปการแก้เชิง hardening หลังได้รับ feedback จริงจากการนำบอทไป compile/run ใน cTrader แล้วพบว่า
- เวอร์ชันก่อนหน้าต้องแก้ compile เองบางจุด
- เมื่อรันได้แล้วกลับไม่เกิด trade เลย

เป้าหมายของรอบนี้จึงไม่ใช่ “ทำให้กำไร” แต่คือ
1. ทำให้โค้ดเป็นมิตรกับการ compile มากขึ้น
2. ทำให้รู้ว่า bot ติดอยู่ตรงไหนจาก log
3. คลายเงื่อนไขที่เข้มเกินจำเป็นเพื่อให้ backtest มี candidate setup ให้ตรวจได้จริง
4. ลด dead path ใน flow breakout → retest → confirm
5. ทำให้เรื่อง session / trading day / หน่วยราคาโปร่งใสขึ้น

---

## 1) สิ่งที่ปรับในโค้ดรอบนี้

### 1.1 ปรับ bias H1 ให้ sequence-aware มากขึ้น
เวอร์ชันก่อน logic “swing ยังไม่ถูกทำลาย” อ่อนและกำกวม
รอบนี้เปลี่ยนเป็น:
- หา `lastSwingHigh` และ `lastSwingLow` แบบ fractal เหมือนเดิม
- ตรวจการ break ด้วย **close** เหนือ/ต่ำกว่าระดับ swing
- ตรวจว่า swing ฝั่งตรงข้ามยัง intact โดยดูว่า **หลัง swing นั้นเกิดมาแล้ว** เคยมี close ทำลายหรือไม่

ผลคือ bias จะ deterministic กว่าเดิม และ log ออกได้ว่าทำไมกลายเป็น bullish / bearish / neutral

---

### 1.2 ล็อก daily loss cap ตาม equity ต้นวัน
รอบนี้เพิ่ม `_startOfDayEquity`
- รีเซ็ตตอนเปลี่ยน trading day
- ใช้ตัวเลขนี้เป็นฐานคำนวณ daily loss cap ตลอดทั้งวัน

ช่วยให้ risk guard รายวันคงที่และเทียบผล backtest ได้ตรงกว่าเดิม

---

### 1.3 เพิ่ม trading day offset
เพิ่ม parameter:
- `Trading Day Offset Hours`

ใช้เพื่อ map “วันเทรด” ตาม convention ที่ต้องการ เช่น
- `0` = ใช้ UTC day ตรง ๆ
- ถ้าจะเลื่อนให้ใกล้ broker/session บางแบบ ให้ปรับ offset ได้

ข้อสำคัญ:
- ต้องใช้ convention เดียวกันตลอดรอบทดสอบ
- ควรบันทึก offset ทุกครั้งในรายงานผล backtest

---

### 1.4 คลายเงื่อนไขเพื่อให้เกิด candidate trade ง่ายขึ้น แต่ยังมี guard
มีการปรับ default หลายจุดให้ไม่เข้มเกินไป เช่น
- `Break Buffer ATR` จาก 0.15 → 0.10
- `Fixed Min Break Buffer` จาก 0.50 → 0.30
- `Retest Zone ATR` จาก 0.20 → 0.30
- `Retest Timeout Bars` จาก 4 → 6
- `Confirm Body Min` จาก 0.50 → 0.35
- `Confirm Close Percent` จาก 0.35 → 0.45
- `Max Breakout Candle ATR` จาก 2.0 → 2.5
- `Min ATR M15` จาก 1.50 → 1.00
- `Max ATR M15` จาก 12.0 → 20.0
- `Max Spread` จาก 1.50 → 2.50
- `Min Stop Distance` จาก 2.0 → 1.5
- `Max Stop Distance` จาก 15.0 → 20.0
- `New York End Hour` จาก 16 → 17

แนวคิดคือ:
- ไม่เอา loose จนไร้กรอบ
- แต่ลดโอกาสที่ระบบเงียบเพราะ threshold แคบเกินสำหรับ XAUUSD จริง

---

### 1.5 เพิ่ม toggle สำหรับ same-bar retest + confirmation
เพิ่ม parameter:
- `Allow Same-Bar Retest Confirm`

ถ้าเปิด (`true`):
- แท่งเดียวกันสามารถทั้งแตะ retest zone และเป็น confirmation ได้
- ช่วยเพิ่มโอกาสเกิดสัญญาณในตลาดที่ rejection เร็ว

ถ้าปิด (`false`):
- หลังเห็น retest ต้องรอแท่งถัดไปมาปิด confirm
- เหมาะกับการทดสอบ flow แบบ conservative

ค่า default รอบนี้ตั้งเป็น `true` เพื่อแก้ปัญหา “ไม่เกิด trade เลย” ก่อน

---

### 1.6 เพิ่ม range memory แบบ lightweight
เพิ่ม parameter:
- `Max Attempts Per Range`

แนวทางที่ใช้:
- สร้าง `RangeKey` จาก direction + session bucket + trading day + range high/low
- นับจำนวน attempt ต่อ range
- block ถ้าเกินจำนวนที่กำหนด

นี่คือการ implement P1 แบบ pragmatic เพื่อกัน overtrade range เดิม โดยยังไม่ถึงขั้นทำ persistent journal เต็มระบบ

---

### 1.7 เพิ่ม diagnostic logging หนักขึ้น
เพิ่ม log หลัก ๆ ดังนี้

#### Bias log
แสดง:
- bias
- reason
- H1 close
- midpoint
- swing high/low
- breakHigh / breakLow
- lowIntact / highIntact

#### Setup log
แสดง:
- arm setup
- retest seen
- invalidate reason
- reject reason
- confirmation fail แบบ verbose

#### Sizing log
แสดง:
- start-of-day equity
- current equity
- risk amount
- entry / stop / stop distance
- stop pips / tp pips
- spread
- pip size / pip value / lot size
- raw volume / normalized volume

#### Trade management log
แสดง:
- break-even move success
- break-even move fail พร้อม error
- time exit success/fail

รอบนี้จงตั้งใจอ่าน log มากกว่า equity curve อย่างเดียว

---

## 2) เรื่อง compile-friendliness ที่ตั้งใจระวัง

### 2.1 ใช้ volume เป็น `double` ตลอดทางก่อน normalize
สาเหตุหนึ่งที่มักเจอใน cTrader/cAlgo คือ signature ของ volume / normalize / execute order มีความต่างเรื่อง type
รอบนี้หลีกเลี่ยงการยัดเป็น `long` เร็วเกินไป และปล่อยให้ `NormalizeVolumeInUnits()` ทำงานบน `double`

จุดนี้มีโอกาสช่วยลด compile friction ที่ผู้ใช้เจอรอบก่อน

---

### 2.2 ลดส่วนที่ไม่จำเป็นและเสี่ยง version mismatch
เวอร์ชันนี้พยายามใช้ surface API พื้นฐานที่พบบ่อยใน cTrader:
- `MarketData.GetBars()`
- `ExecuteMarketOrder()`
- `ModifyPosition()`
- `ClosePosition()`
- `Positions.Opened / Closed`

แต่ยังต้อง compile test จริงอีกครั้งใน cTrader build ที่ผู้ใช้ใช้อยู่

---

## 3) สิ่งที่ยังเป็นข้อจำกัด

### 3.1 Position sizing ยังต้อง verify กับ symbol spec จริงของ broker
แม้รอบนี้เพิ่ม log ละเอียดขึ้นแล้ว แต่สูตร sizing ยังอิง:
- `PipValue`
- `LotSize`
- `PipSize`

ดังนั้นหลัง compile/rerun ต้องดู log sizing ทุกดีลแรก ๆ ว่า volume ดูสมเหตุสมผลหรือไม่

ถ้า volume ดูหลุดโลก:
- ให้หยุดสรุปผล performance ทันที
- แล้วแก้ sizing ให้ตรงกับ broker target ก่อน

---

### 3.2 Entry ยังเป็น market execution ตามราคาปัจจุบัน
รอบนี้ตั้งใจใช้
- Buy → `Symbol.Ask`
- Sell → `Symbol.Bid`
เป็น expected entry price สำหรับคำนวณ risk/log

แต่ fill จริงยังขึ้นกับ engine ของ cTrader
จึงยังต้อง visual validate คู่กับกราฟจริง

---

### 3.3 ยังไม่ได้ทำ structure-target จริงสำหรับ MinRewardRisk
รอบนี้ตัด parameter `Min Reward/Risk` ออกจาก logic แล้ว
เพราะของเดิมเป็น guard หลอก ๆ มากกว่า guard จริง

สรุปคือ:
- ตอนนี้อย่าเข้าใจผิดว่าบอทมี structure-based RR filter แล้ว
- ถ้าจะใส่กลับ ต้องทำ target proxy จริงในรอบถัดไป

---

## 4) คำแนะนำสำหรับการ backtest รอบถัดไป

### ชุดตั้งต้นที่แนะนำให้ลองก่อน
- ใช้ M15 chart
- Bias TF = Hour
- `Allow Same-Bar Retest Confirm = true`
- `Verbose Logging = true`
- `Trading Day Offset Hours = 0` ก่อน
- ใช้ session ตาม UTC ตามที่โค้ดตั้งไว้ แล้วบันทึกไว้ในรายงาน

### สิ่งที่ต้องสังเกตจาก log
1. มี `BIAS` ออกมาสลับ bullish/bearish/neutral สมเหตุสมผลหรือไม่
2. มี `ARM` เกิดบ่อยขึ้นหรือยัง
3. มี `RETEST` แล้วไปต่อถึง `ENTRY` บ้างหรือยัง
4. ถ้าไม่เข้า เพราะอะไรบ่อยสุด
   - neutral bias
   - spread too high
   - stop distance invalid
   - volume below min
   - outside session
5. ค่า `SIZING` สมเหตุสมผลกับ XAUUSD ของ broker หรือไม่

---

## 5) ถ้ายัง “ไม่มี trade เลย” หลังรอบนี้ ให้ตรวจตามลำดับ

1. ตรวจว่า attach bot บนกราฟ `M15` จริง
2. เปิด `Verbose Logging`
3. ดูว่า bias เป็น neutral เกือบตลอดหรือไม่
4. ดูว่า session window แคบไปสำหรับ timezone ที่ใช้หรือไม่
5. ดูว่า `MaxSpread` ยังแคบเกินสำหรับ broker นี้หรือไม่
6. ดูว่า `Min/Max Stop Distance` ไม่สอดคล้องกับ quote model ของ broker หรือไม่
7. ดูว่า `volume below symbol minimum` เกิดบ่อยหรือไม่
8. ถ้ายัง arm setup น้อยมาก ให้ลด `Break Buffer ATR` หรือเพิ่ม `Retest Timeout Bars` อีกเล็กน้อย

---

## 6) สรุปตรง ๆ

รอบ hardening นี้ไม่ได้พยายามทำให้ระบบ “ดูดี” บน backtest
แต่พยายามทำให้ระบบ:
- คอมไพล์ง่ายขึ้น
- อธิบายตัวเองได้ผ่าน log
- ไม่เงียบเพราะ logic เข้มเกินไป
- มี state flow ที่ตรวจได้จริงบนกราฟ

ดังนั้น success criterion ของรอบนี้คือ:
- bot รันได้
- มี setup/candidate trades ให้ audit
- รู้ชัดว่า reject/invalidate เพราะอะไร

ยังไม่ใช่การยืนยันว่ากลยุทธ์นี้ profitable
