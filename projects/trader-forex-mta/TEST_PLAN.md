# TEST_PLAN.md

อัปเดตล่าสุด: 2026-03-09
สถานะ: Backtest Preparation Plan

เอกสารนี้เป็นแผนทดสอบสำหรับ `MtaGoldBreakoutRetestBot` บน `XAUUSD` โดยตั้งใจให้ใช้เพื่อ:
- ตรวจ logic ว่าตรงสเปกหรือไม่
- ตรวจ robustness ก่อนเชื่อผล backtest
- ลดความเสี่ยงจาก curve fit และ broker-specific illusion

> ข้อสำคัญ: แผนนี้ไม่ตั้งต้นจากสมมติฐานว่า bot จะกำไร
> จุดประสงค์คือ “หาวิธีทำให้การทดสอบไม่หลอกเรา” มากกว่า “หาค่าที่สวยที่สุด”

---

## 1) เป้าหมายของการทดสอบ

### เป้าหมายหลัก
1. ยืนยันว่า logic ในโค้ดทำงานตามที่สเปกระบุ
2. ยืนยันว่าหน่วยราคา, stop distance, spread, และ volume sizing ถูกต้องกับ broker ที่ใช้
3. วัดความคงเส้นคงวาของระบบข้าม:
   - ช่วงเวลา
   - session
   - volatility regime
   - spread/slippage assumptions
4. คัดแยกว่า edge ที่เห็นมาจาก strategy จริง หรือมาจาก implementation artifact

### เป้าหมายรอง
1. หา parameter zone ที่เสถียรกว่าค่าเดียว
2. เตรียมชุดทดสอบ walk-forward
3. สร้าง go / no-go criteria ก่อนเข้าสู่การทดสอบเงินจริงหรือ forward test

---

## 2) สิ่งที่ต้องทำก่อนเริ่ม backtest

## 2.1 Compile และ sanity check ใน cTrader จริง
ต้องทำรายการนี้ก่อนทุกอย่าง:
- import โค้ดเข้า cTrader Automate
- build/compile ให้ผ่าน
- ยืนยันว่าไม่มี API mismatch ตามเวอร์ชัน cTrader ที่ใช้งาน
- attach กับกราฟ `XAUUSD M15`
- เปิดดู parameters ทั้งหมดว่าปรากฏครบ

### ผลลัพธ์ที่ต้องการ
- bot compile ผ่าน 100%
- เริ่มทำงานได้โดยไม่ throw runtime error ทันที
- H1 bars โหลดได้จริง

---

## 2.2 ยืนยัน symbol specification ของ broker
ต้องจดค่าจริงจาก broker/test environment:
- `Symbol.Name`
- `Symbol.Digits`
- `Symbol.PipSize`
- `Symbol.TickSize` (ถ้ามี)
- `Symbol.PipValue`
- `Symbol.LotSize`
- `VolumeInUnitsMin`
- `VolumeInUnitsStep`
- min stop distance / stop level constraints
- spread ปกติแยกตาม session

### เป้าหมาย
- ยืนยันว่าค่า `MinStopDistance`, `MaxStopDistance`, `BreakEvenOffset`, `MaxSpread` อยู่ในหน่วยที่สมเหตุสมผล
- ยืนยันว่าการคำนวณ volume ไม่เสี่ยงเพี้ยน

---

## 2.3 เพิ่ม logging เพื่อการ audit ก่อน run รอบใหญ่
ควรเพิ่มหรือเปิด log ให้เห็นอย่างน้อย:
- bias H1 ของแต่ละ signal
- range high/low
- breakout level
- break buffer
- retest zone
- stop distance
- stop pips
- TP pips
- risk amount
- raw/normalized volume
- spread ตอน entry
- เหตุผล reject/invalidate
- เหตุผล modify BE สำเร็จหรือไม่สำเร็จ

ถ้า export CSV/JSON ได้ จะช่วยมาก

---

## 3) ขั้นตอนการทดสอบ

## เฟส 1: Logic Validation บนกราฟ

### วัตถุประสงค์
ตรวจว่า bot “ตีความกราฟ” ถูกก่อนสนใจ PnL

### วิธีทำ
- เลือกช่วงข้อมูล 2-4 สัปดาห์
- ใช้ visual backtest / step-through
- ตรวจอย่างน้อย 30 setup candidates

### Checklist
1. bias H1 ตรงกับสิ่งที่เห็นบนกราฟหรือไม่
2. breakout valid ถูก arm ถูกจังหวะหรือไม่
3. retest timeout ทำงานถูกหรือไม่
4. confirmation candle ผ่าน/ไม่ผ่านสมเหตุสมผลหรือไม่
5. stop loss อยู่หลัง invalidation point จริงหรือไม่
6. break-even ขยับเมื่อถึง 1R จริงหรือไม่
7. time exit ปิดตาม bar count จริงหรือไม่
8. daily guards รีเซ็ตถูกวันหรือไม่

### เกณฑ์ผ่านเฟส 1
- ไม่มี bug เชิง logic ชัดเจน
- ไม่มีกรณี “เข้าเทรดทั้งที่สเปกบอกไม่ควรเข้า” แบบซ้ำ ๆ
- ไม่มีปัญหาหน่วยราคาหรือ volume ผิดชัดเจน

ถ้าไม่ผ่าน: หยุดก่อน ยังไม่ควรไป optimization

---

## เฟส 2: Baseline Backtest

### วัตถุประสงค์
สร้าง baseline ที่ trace ได้ง่าย ด้วย parameter ค่า default ก่อน

### ค่าเริ่มต้น
- Symbol: `XAUUSD`
- Execution TF: `M15`
- Bias TF: `H1`
- Risk: `0.25%`
- TP: `2.0R`
- BE: `1.0R`
- Max trades/day: `2`
- Max losses/day: `2`
- Daily loss cap: `1.0%`

### ช่วงข้อมูล baseline ที่แนะนำ
เพื่อให้เห็นหลาย regime ควรครอบคลุมอย่างน้อย 3 ปี ถ้ามีข้อมูลคุณภาพพอ

#### ชุดแนะนำ
- 2022-01-01 ถึง 2022-12-31
- 2023-01-01 ถึง 2023-12-31
- 2024-01-01 ถึง 2024-12-31
- 2025-01-01 ถึง 2025-12-31

ถ้าข้อมูลไม่พอ ให้เริ่มขั้นต่ำ:
- 2023-01-01 ถึง 2025-12-31

### เหตุผล
- ทองคำมี regime shift ชัด
- ต้องการเห็นช่วง trend, chop, high volatility, event-driven expansion

---

## เฟส 3: Session Decomposition

### วัตถุประสงค์
ดูว่าระบบอยู่ได้จริงใน session ไหน ไม่ใช่รวมทุกอย่างแล้วหลอกตา

### ต้องรันอย่างน้อย 3 แบบ
1. London only
2. New York only
3. London + New York

### สิ่งที่ต้องเปรียบเทียบ
- จำนวน trade
- win rate
- average R / trade
- expectancy
- max drawdown
- profit factor
- average holding bars
- spread sensitivity

### สิ่งที่ต้องจับตา
- ถ้า edge อยู่แค่ session เดียว ถือว่าระบบเฉพาะทาง ไม่ใช่ universal
- ถ้ารวมสอง session แล้วแย่ลง อาจมีปัญหาเรื่อง parameter ใช้กับคนละ microstructure

---

## เฟส 4: Parameter Sensitivity Test

### หลักการ
ห้าม optimize แบบไล่หาค่าที่สวยที่สุดอย่างเดียว
ให้ดูว่า “โซนรอบ ๆ ค่า default ยังพออยู่ได้หรือไม่”

## 4.1 Core Sweep ที่ควรทดสอบก่อน

### RangeLookbackM15
- 6
- 8
- 10
- 12

### BreakBufferAtr
- 0.10
- 0.15
- 0.20
- 0.25

### RetestZoneAtr
- 0.10
- 0.20
- 0.30

### RetestTimeoutBars
- 2
- 4
- 6

### ConfirmationBodyMin
- 0.40
- 0.50
- 0.60

### MaxBreakoutCandleAtr
- 1.5
- 2.0
- 2.5

### SlBufferAtr
- 0.05
- 0.10
- 0.15

### TakeProfitR
- 1.5
- 2.0
- 2.5

### BreakEvenAtR
- 0.8
- 1.0
- 1.2
- ปิด BE ไปเลย 1 ชุดเพื่อเทียบ

---

## 4.2 Session Parameter Sweep
- London start/end ขยับทีละ 1 ชั่วโมง
- NY start/end ขยับทีละ 1 ชั่วโมง
- Friday cutoff: 15 / 16 / 17 / 18 UTC

เหตุผล:
- breakout/retest systems บนทองไวกับเวลาอย่างมาก

---

## 4.3 ATR / Spread Filter Sweep
### MinAtrM15
- 1.0
- 1.5
- 2.0

### MaxAtrM15
- 8.0
- 10.0
- 12.0
- 15.0

### MaxSpread
- 0.8
- 1.2
- 1.5
- 2.0
- 2.5

---

## 4.4 Stop Constraint Sweep
### MinStopDistance
- 1.0
- 2.0
- 3.0

### MaxStopDistance
- 10.0
- 15.0
- 20.0

**หมายเหตุ:** ต้องยืนยันหน่วยกับ broker ก่อน ไม่เช่นนั้น sweep นี้ไม่มีความหมาย

---

## เฟส 5: Robustness Test

### 5.1 Spread stress test
อย่างน้อยทดสอบ 3 สภาพ:
1. spread base / optimistic
2. spread realistic
3. spread stressed

ถ้า cTrader backtest ปรับ spread model ได้ ให้ใช้
ถ้าปรับไม่ได้ ให้ทดสอบผ่าน `MaxSpread` และ conservative broker selection

### 5.2 Slippage stress test
ถ้า engine รองรับ ให้ทดสอบ slippage หลายระดับ
ถ้าไม่รองรับ ให้ตีความผลแบบ conservative ว่า live น่าจะแย่กว่า backtest สำหรับ market order breakout system

### 5.3 Broker cross-check
อย่างน้อย 2 สภาพแวดล้อมถ้าเป็นไปได้:
- broker/test data A
- broker/test data B

จุดประสงค์คือดูว่า edge มาจากกลยุทธ์หรือมาจาก symbol specification/data model ของแหล่งเดียว

---

## 6) ชุดข้อมูลแนะนำสำหรับ Walk-Forward

## 6.1 โครง walk-forward แบบง่าย
ใช้ rolling windows:
- Train 6 เดือน / Test 3 เดือน
หรือ
- Train 12 เดือน / Test 3 เดือน

### ตัวอย่าง
- Train: Jan-Jun 2023 → Test: Jul-Sep 2023
- Train: Apr-Sep 2023 → Test: Oct-Dec 2023
- Train: Jul-Dec 2023 → Test: Jan-Mar 2024
- ทำต่อเนื่องไปจนถึงข้อมูลล่าสุด

### เป้าหมาย
- ดูว่าค่าที่เลือกไว้ยังใช้ได้ใน out-of-sample หรือไม่
- หลีกเลี่ยงการตัดสินจากผลรวมทั้งก้อนที่มี look-back bias

---

## 6.2 กฎเวลาเลือก parameter ใน walk-forward
- ห้ามเลือกค่าที่ดีที่สุดเฉพาะ metric เดียว
- ให้เลือกค่าที่อยู่ในโซนเสถียร เช่น top quartile ของหลาย metric พร้อมกัน
- ถ้าต้องเลือกระหว่าง “ผลตอบแทนสูงแต่แกว่งมาก” กับ “ผลตอบแทนกลางแต่เสถียรกว่า” ให้เลือกอย่างหลัง

---

## 7) Metrics ที่ต้องเก็บ

## 7.1 ระดับระบบ
- Net profit
- Return %
- Profit factor
- Expectancy ต่อ trade
- Avg R / trade
- Max drawdown
- Recovery factor
- Sharpe/Sortino (ถ้า platform ให้)
- Number of trades
- Win rate
- Loss rate
- Break-even rate
- Avg trade duration
- Median trade duration

## 7.2 ระดับพฤติกรรมของ setup
- จำนวน breakout armed
- จำนวน invalidated ก่อนเข้า
- จำนวน retest ที่เกิดจริง
- จำนวน confirmation ที่ผ่าน
- conversion rate จาก breakout → entry
- อัตราโดน stop distance reject
- อัตราโดน spread reject
- อัตราโดน daily guard block

## 7.3 ระดับ robustness
- ผลแยกตามปี
- ผลแยกตาม quarter
- ผลแยกตาม session
- ผลแยกตาม ATR regime
- ผลแยกตาม weekday
- ผลแยกตาม stop distance bucket

---

## 8) Go / No-Go Criteria

## 8.1 Go ขั้นต่ำสำหรับไปต่อสู่ forward test แบบระวังตัว
ต้องผ่านครบทุกข้อหลัก:
1. compile/run ได้เสถียร ไม่มี runtime bug สำคัญ
2. volume sizing ยืนยันแล้วว่าถูกต้องกับ broker ที่ใช้
3. logic validation บนกราฟผ่าน
4. ผล out-of-sample ไม่พังยับเทียบกับ in-sample
5. ไม่มีปีหรือ quarter ใดที่แย่ผิดปกติจนบ่งชี้ว่า edge เปราะมากเกิน
6. จำนวน trade เพียงพอให้สรุปได้ในระดับหนึ่ง
7. spread/slippage stress แล้วยังไม่ collapse ทันที

### เกณฑ์เชิงตัวเลขที่แนะนำแบบ conservative
- Profit factor > 1.10 ใน out-of-sample หลายช่วง ไม่ใช่แค่รวมทั้งหมด
- Max drawdown อยู่ในระดับที่รับได้เมื่อเทียบกับ expectancy
- Avg R / trade เป็นบวกหลังรวมต้นทุน
- ไม่มี dependency กับ session เดียวแบบเปราะเกินไป เว้นแต่ตั้งใจออกแบบมาเฉพาะ session นั้น

> ถ้าผ่านเพียงเล็กน้อย ให้ตีความว่า “พอไป forward test แบบทดลอง” ไม่ใช่ “พร้อมใช้เงินจริงหนัก ๆ”

## 8.2 No-Go ทันที
ถ้าเจอข้อใดข้อหนึ่ง ให้หยุดก่อน:
1. sizing ผิดหน่วยหรือไม่แน่ใจ
2. bias H1 ตีความเพี้ยนจากกราฟบ่อย
3. ผลดีเฉพาะช่วงเดียวหรือ session เดียวแบบรุนแรง
4. เปลี่ยน spread/slippage นิดเดียวแล้ว equity curve พัง
5. ต้อง optimize แคบมากถึงจะดูดี
6. ระบบ trade น้อยเกินไปจนสถิติไม่น่าเชื่อถือ

---

## 9) ลำดับการทำงานที่แนะนำ

### Step 1
แก้ประเด็น critical จาก `TEST_REVIEW.md` ก่อน โดยเฉพาะ:
- bias H1
- daily loss cap
- volume sizing verification
- logging

### Step 2
compile และ visual validate 30 setup candidates

### Step 3
run baseline backtest ด้วย default params

### Step 4
แยกผล London / NY / Combined

### Step 5
ทำ sensitivity sweep แบบจำกัดชุด ไม่ brute-force กว้างเกิน

### Step 6
ทำ walk-forward

### Step 7
ทำ spread/slippage stress

### Step 8
ตัดสิน go/no-go สำหรับ forward test แบบ demo หรือ paper trade

---

## 10) ชุดบันทึกผลที่ควรมีทุกครั้ง

ทุก run ควรบันทึกอย่างน้อย:
- broker / data source
- timezone convention
- symbol specification
- date range
- session config
- parameter set
- spread assumption
- slippage assumption
- จำนวน trades
- PF
- expectancy
- max DD
- notes เรื่อง anomaly

แนะนำให้ทำตารางผลลัพธ์แยกเป็น:
- baseline
- session split
- parameter sweep
- walk-forward
- stress test

---

## 11) บทสรุปเชิงปฏิบัติ

แผนทดสอบนี้ตั้งใจบังคับให้ระบบผ่าน 3 ด่านก่อน:
1. **ถูกต้อง** — logic/units/timezone ถูก
2. **เสถียร** — ไม่พังเมื่อขยับ parameter หรือเปลี่ยนช่วงเวลา
3. **ทนโลกจริงขึ้นนิดหนึ่ง** — spread/slippage/broker change แล้วไม่ยุบทันที

ถ้าระบบยังไม่ผ่านสามด่านนี้ การ optimize ต่อมีโอกาสได้แค่เส้น equity ที่สวยบนกระดาษ
