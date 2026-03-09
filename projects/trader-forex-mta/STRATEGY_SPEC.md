# STRATEGY_SPEC.md

ชื่อสเปก: `MTA_GOLD_BreakoutRetestConfirm_v0.1`
อัปเดตล่าสุด: 2026-03-09
สถานะ: Analysis Draft for Dev Handoff

> เอกสารนี้เป็น **สเปกกลยุทธ์ที่ทีมวิเคราะห์สร้างขึ้น** โดยอิงจากหลักฐานสาธารณะที่พบว่าเพจ MTA วางตัวเองเป็น
> - สอนเทรด Forex
> - วิเคราะห์กราฟทองทุกวัน
> - มี framing แบบห้อง Signal
>
> เอกสารนี้ **ไม่ได้อ้างว่าเป็นกฎจริงของ MTA** และ **ไม่ได้อ้างว่าถอดวิธีของ MTA ได้ตรงทั้งหมด**
> แต่เป็นการ formalize กลยุทธ์ที่ “เข้ากับกรอบการสื่อสารที่สังเกตได้” และเหมาะสำหรับนำไปเขียน bot แบบ conservative

---

## 1) Facts / Interpretation / Open Questions

### Facts ที่รองรับจากงานวิจัย
- เพจระบุชัดว่าเน้น **Forex**
- เพจระบุชัดว่า **วิเคราะห์กราฟทองทุกวัน**
- เพจระบุชัดว่ามี **ห้อง Signal**
- ภาษาที่สรุปได้จากงานวิจัยชี้ไปทาง **daily analysis + actionable setup + confirmation before entry**

### Interpretation ที่ทีมวิเคราะห์ใช้ในการสร้างสเปกนี้
- เลือก **XAUUSD** เป็นตลาดหลักของระบบเวอร์ชันแรก
- เลือก setup แบบ **Breakout + Retest + Confirmation** เพราะสอดคล้องกับคำว่า “วิเคราะห์กราฟ” และ “สัญญาณที่ actionable” มากกว่าการใช้ indicator-heavy system
- ใช้แนวคิด **market structure + key level + bar-close confirmation** เพื่อให้เขียนโค้ดได้จริงและลดความคลุมเครือ

### Open Questions
- MTA ใช้ timeframe ใดจริงเป็นหลัก ยังไม่ทราบ
- MTA ใช้ logic breakout, reversal, liquidity sweep หรือ trend continuation เป็นแกนจริงหรือไม่ ยังไม่ทราบ
- MTA ใช้ news filter จริงหรือไม่ ยังไม่ทราบ

---

# 2) Executive Summary

กลยุทธ์หลักของเอกสารนี้คือ:

**Primary Setup:** รอให้ราคา XAUUSD ทะลุกรอบสำคัญระยะสั้นในทิศทางเดียวกับ bias ที่ได้จากโครงสร้าง H1 จากนั้นรอราคาย้อนกลับมาทดสอบโซน breakout บน M15 และเข้าเทรดก็ต่อเมื่อมีแท่งยืนยันการ rejection/continuation ชัดเจน

แนวคิดหลัก:
- ไม่ไล่ราคาในแท่ง breakout แรก
- ไม่สวนโครงสร้างใหญ่
- ไม่เข้าเมื่อ breakout ยังไม่มี retest หรือยังไม่มี confirmation
- จำกัดจำนวนเทรดต่อวันและจำกัดความเสี่ยงรายวัน

**Secondary Setup (optional):** false-break / sweep-and-reclaim แบบ conservative บริเวณกรอบ London opening range แต่ให้ถือเป็น setup สำรองเท่านั้น ไม่ใช่เวอร์ชัน implement แรก

---

# 3) Market Scope

## 3.1 Instrument
- หลัก: `XAUUSD`
- ไม่รองรับหลายสินทรัพย์ในเวอร์ชันแรก

## 3.2 Trading Style
- Intraday / session-based
- ไม่ออกแบบให้ถือข้ามวันโดยตั้งใจ

## 3.3 Timeframes
- Bias timeframe: `H1`
- Setup / execution timeframe: `M15`
- Optional micro confirmation timeframe: **ไม่ใช้ใน v0.1** เพื่อไม่เพิ่มความซับซ้อน

## 3.4 Timezone Reference
- ให้ Dev/Test เลือก timezone เดียวตลอดทั้งระบบ
- แนะนำใช้ broker/server time หรือ UTC แบบ fix แล้ว map session ให้ชัด
- ห้ามปล่อยให้ session logic อิง timezone แบบไม่ระบุชัด เพราะจะทำให้ผล backtest เพี้ยน

---

# 4) Primary Setup — Breakout + Retest + Confirmation

## 4.1 เป้าหมายของ setup
จับจังหวะที่ทอง breakout จากกรอบระยะสั้น แล้วกลับมาทดสอบระดับ breakout ก่อนวิ่งต่อ โดยเล่นเฉพาะฝั่งที่สอดคล้องกับ bias ของโครงสร้าง H1

## 4.2 เหตุผลที่เลือก setup นี้
- เขียนโค้ดได้ชัดกว่าคำเชิง discretionary
- สอดคล้องกับ framing แบบ daily analysis + actionable setup
- อธิบายบนกราฟได้ง่ายและเหมาะกับ content style ที่ดูเป็น chart walkthrough
- ใช้ OHLC เป็นหลัก ไม่ต้องพึ่งข้อมูลภายนอกจำนวนมาก

---

# 5) Bias และ Market Structure Logic

## 5.1 นิยาม bias บน H1
ใช้ swing structure แบบ simplified

### Bullish bias
ให้ถือว่า H1 เป็น bullish เมื่อครบทุกข้อ:
1. ราคาปัจจุบันปิดเหนือค่า midpoint ของ H1 range lookback ล่าสุด 20 แท่ง
2. swing high ล่าสุดถูกทำลายด้วยการ **ปิดเหนือ** ไม่ใช่แค่ wick เหนือ
3. swing low ล่าสุดยังไม่ถูกทำลายลง

### Bearish bias
ให้ถือว่า H1 เป็น bearish เมื่อครบทุกข้อ:
1. ราคาปัจจุบันปิดต่ำกว่าค่า midpoint ของ H1 range lookback ล่าสุด 20 แท่ง
2. swing low ล่าสุดถูกทำลายด้วยการ **ปิดต่ำกว่า**
3. swing high ล่าสุดยังไม่ถูกทำลายขึ้น

### Neutral / no-trade bias
ถ้าไม่เข้าเงื่อนไข bullish หรือ bearish ชัดเจน ให้ถือว่าเป็น neutral และ **ไม่เปิด setup ใหม่**

## 5.2 นิยาม swing สำหรับ v0.1
ใช้ fractal/pivot แบบง่าย:
- swing high = high ของแท่งกลาง สูงกว่า high ของ 2 แท่งก่อนหน้าและ 2 แท่งถัดไป
- swing low = low ของแท่งกลาง ต่ำกว่า low ของ 2 แท่งก่อนหน้าและ 2 แท่งถัดไป

> หมายเหตุ: หากทีม dev เห็นว่าการใช้ fractal 2-2 ทำให้สัญญาณช้าเกินไป สามารถทำเป็น parameter `SwingStrength` ได้ แต่ค่า default ให้เริ่มที่ 2

---

# 6) Session Filters

## 6.1 Allowed Sessions
อนุญาตให้หา entry เฉพาะช่วง liquidity สูง

### Window A: London session
- ช่วง 2-4 ชั่วโมงแรกของ London

### Window B: New York early session
- ช่วง 2-3 ชั่วโมงแรกของ New York

## 6.2 สิ่งที่ไม่ทำใน v0.1
- ไม่เปิด entry ในช่วงตลาดเอเชียที่แคบ
- ไม่เปิด entry ใกล้วันปิดตลาด
- ไม่เปิด entry นอก session window ที่กำหนด

## 6.3 Session implementation note
Dev ต้อง implement session windows เป็น input ที่แก้ได้ เช่น
- `LondonStartHour`
- `LondonEndHour`
- `NewYorkStartHour`
- `NewYorkEndHour`

เพื่อให้ test agent ปรับตาม broker timezone ได้

---

# 7) Setup Detection Logic

## 7.1 นิยามกรอบ breakout บน M15
ให้สร้าง `Setup Range` จากหนึ่งในสองวิธีนี้ และให้ใช้วิธีเดียวคงที่ใน v0.1

### วิธีแนะนำ (default)
- ใช้ high/low ของ M15 ย้อนหลัง `N = 8` แท่งล่าสุด
- ไม่รวมแท่งปัจจุบันที่กำลังก่อตัว
- ได้เป็น `RangeHigh` และ `RangeLow`

เหตุผล:
- เรียบง่าย
- เขียนโค้ดง่าย
- ใช้ได้กับ intraday gold

## 7.2 เงื่อนไข breakout valid

### Buy breakout valid
1. Bias บน H1 ต้องเป็น bullish
2. มีแท่ง M15 ปิดเหนือ `RangeHigh`
3. ระยะที่ปิดเหนือ breakout level ต้องมากกว่า buffer ขั้นต่ำ เช่น
   - `BreakBuffer = max(0.15 * ATR(14, M15), FixedMinBuffer)`
4. ขนาดแท่ง breakout ต้องไม่ใหญ่ผิดปกติ เช่นไม่เกิน `2.0 x ATR(14, M15)` เพื่อหลีกเลี่ยงไล่ราคาในแท่งข่าวแรง

### Sell breakout valid
1. Bias บน H1 ต้องเป็น bearish
2. มีแท่ง M15 ปิดต่ำกว่า `RangeLow`
3. ระยะที่ปิดต่ำกว่า breakout level ต้องมากกว่า buffer ขั้นต่ำ
4. ขนาดแท่ง breakout ต้องไม่ใหญ่ผิดปกติ

## 7.3 Retest condition
หลัง breakout valid แล้ว:
- รอให้ราคา retrace กลับเข้าหา breakout level ภายใน `RetestTimeoutBars = 4` แท่ง M15
- โซน retest คือช่วง
  - Buy: `RangeHigh ± RetestZoneWidth`
  - Sell: `RangeLow ± RetestZoneWidth`
- `RetestZoneWidth` แนะนำเริ่มที่ `0.20 * ATR(14, M15)`

ถ้าไม่เกิด retest ภายในเวลาที่กำหนด ให้ยกเลิก setup

## 7.4 Confirmation candle
เมื่อราคาเข้าโซน retest แล้ว ต้องรอแท่ง M15 ปิดยืนยัน

### Buy confirmation
แท่งยืนยันต้องครบทุกข้อ:
1. low ของแท่งแตะหรือทิ่มเข้า retest zone
2. close ของแท่งปิดกลับเหนือ breakout level
3. body ของแท่ง >= 50% ของ range แท่งนั้น
4. close อยู่ใน 35% บนสุดของ range แท่ง

### Sell confirmation
แท่งยืนยันต้องครบทุกข้อ:
1. high ของแท่งแตะหรือทิ่มเข้า retest zone
2. close ของแท่งปิดกลับต่ำกว่า breakout level
3. body ของแท่ง >= 50% ของ range แท่งนั้น
4. close อยู่ใน 35% ล่างสุดของ range แท่ง

---

# 8) Entry Rules

## 8.1 Buy Entry
เข้า Buy เมื่อครบทุกข้อ:
1. H1 bias = bullish
2. มี valid breakout เหนือ M15 range
3. เกิด retest ภายใน timeout
4. มี buy confirmation candle ปิดแล้ว
5. spread ปัจจุบันไม่เกิน threshold
6. ยังไม่ชน risk guard รายวัน
7. ยังไม่เกินจำนวนเทรดต่อวัน

### วิธีเข้าออเดอร์
- เข้าแบบ market order ที่เปิดของแท่งถัดไปหลัง confirmation candle ปิด
- ไม่ใช้ stop order ใน v0.1

## 8.2 Sell Entry
เข้า Sell เมื่อครบเงื่อนไข mirror ฝั่ง Buy

---

# 9) Invalidation Rules

setup ถือว่า invalid และห้ามเข้า หากเกิดข้อใดข้อหนึ่งก่อน entry:

## 9.1 Invalid ก่อน entry
- Bias H1 เปลี่ยนเป็น neutral หรือกลับฝั่ง
- ราคาไม่ retest ภายใน timeout
- ราคาปิดทะลุฝั่งตรงข้ามของ setup range ก่อนเกิด confirmation
- breakout candle ใหญ่มากผิดปกติและเกิน volatility filter
- spread เกิน threshold ที่ตั้งไว้
- อยู่ใน blackout window ที่กำหนด

## 9.2 Invalid หลัง entry
- ถ้าถูก stop loss ถือว่า setup จบ
- ห้าม re-enter setup เดิมซ้ำมากกว่า 1 ครั้งจาก range เดิม

---

# 10) Stop Loss Rules

## 10.1 Buy stop loss
ตั้ง stop ที่จุดต่ำสุดระหว่าง:
- low ของ confirmation candle
- low ต่ำสุดของ retest sequence

แล้วลบ buffer เพิ่มอีก:
- `SLBuffer = max(0.10 * ATR(14, M15), FixedMinSLBuffer)`

## 10.2 Sell stop loss
ใช้ mirror logic ฝั่ง Buy

## 10.3 Stop distance constraints
- ถ้า stop แคบเกิน `MinStopDistance` ให้ยกเลิก trade
- ถ้า stop กว้างเกิน `MaxStopDistance` ให้ยกเลิก trade

เหตุผล:
- stop แคบมากมักโดน noise
- stop กว้างมากทำให้ RR เสียและ position sizing ผิดธรรมชาติ

---

# 11) Take Profit / Trade Management

## 11.1 Initial take profit
ใช้แบบ conservative ง่ายก่อน
- `TP1 = 1.5R`
- `TP2 = 2.0R` เป็น target หลักของระบบ

สำหรับ v0.1 แนะนำเลือก **หนึ่งวิธีเดียว** เพื่อให้ผลทดสอบอ่านง่าย

### ทางเลือกแนะนำเป็น default
- ปิดเต็มจำนวนที่ `2.0R`
- ไม่ใช้ partial exit

## 11.2 Break-even rule
เมื่อกำไรถึง `+1.0R`
- ขยับ SL ไปที่ entry + offset เล็กน้อยเพื่อครอบคลุม spread/commission
- ถ้าระบบหรือแพลตฟอร์มทำยาก ให้เริ่มจาก BE = exact entry ก่อน แล้วค่อยปรับใน v0.2

## 11.3 Trailing rule
- ไม่ใช้ trailing stop แบบ dynamic ใน v0.1
- เหตุผล: ลดความซับซ้อนและลดโอกาส overfit

## 11.4 Time exit
- หากเปิดเทรดแล้วไม่ถึง TP/SL ภายใน `MaxBarsInTrade = 8` แท่ง M15
- ให้ปิดที่ตลาดเมื่อครบเวลา

เหตุผล:
- strategy นี้ออกแบบเป็น intraday momentum-follow-through
- ถ้าราคาไม่ไปต่อภายในเวลาสมเหตุสมผล edge มักลดลง

---

# 12) Risk Constraints

## 12.1 Risk per trade
- แนะนำ `0.25% - 0.50%` ของ equity ต่อเทรด
- default สำหรับการทดสอบจริงจัง: `0.25%`

## 12.2 Max trades per day
- `2` เทรดต่อวันสูงสุด
- ถ้าโดน stop loss 2 ครั้งในวันเดียว ให้หยุดเทรดวันนั้น

## 12.3 Daily loss cap
- ถ้าขาดทุนสะสมถึง `-1.0%` ของ equity ในวันเดียว ให้หยุดเปิด setup ใหม่

## 12.4 Max concurrent positions
- `1` position เท่านั้น
- ห้ามถือ Buy และ Sell พร้อมกัน

## 12.5 Spread filter
- ไม่เปิดเทรดถ้า spread > ค่าเฉลี่ย rolling spread * factor ที่กำหนด
- ถ้าไม่มีข้อมูล spread history ให้ใช้ absolute max spread เป็น input ชั่วคราว

---

# 13) No-Trade Conditions

ห้ามเปิดออเดอร์ใหม่เมื่อเกิดข้อใดข้อหนึ่ง:

1. H1 bias เป็น neutral
2. ATR(M15) ต่ำเกิน threshold จนตลาดแคบผิดปกติ
3. ATR(M15) สูงเกิน threshold จนตลาดผันผวนผิดปกติ
4. breakout candle มีขนาด > 2.0 ATR
5. spread สูงเกิน threshold
6. อยู่นอก allowed session
7. ชน daily loss cap หรือ max trades/day
8. วันศุกร์ช่วงท้าย session
9. หลังเกิด trade แรกแล้วราคากลับมาป้วนเปี้ยนใน range เดิมเกินจำนวนแท่งที่กำหนด
10. มีแท่งยืนยัน แต่ RR ถึง target ขั้นต่ำไม่คุ้ม เช่น น้อยกว่า `1.5R` ไปยัง target โครงสร้างถัดไป

---

# 14) Secondary Setup (Optional) — Sweep & Reclaim Reversal

> setup นี้ใส่ไว้เป็นผู้สมัครลำดับสอง ยังไม่แนะนำให้ implement ก่อน primary setup

## 14.1 แนวคิด
เมื่อราคาแทงเกิน high/low ของ opening range หรือ swing ล่าสุดใน session แล้วปิดกลับเข้ากรอบอย่างชัดเจน ให้เล่นกลับเข้ากรอบแบบ conservative

## 14.2 เงื่อนไขคร่าว ๆ
- ใช้เฉพาะช่วง London/NY ที่มี liquidity
- ต้องมี sweep เกินระดับอ้างอิงขั้นต่ำ
- ต้องมี reclaim close กลับเข้ากรอบ
- stop อยู่หลังจุด extreme ของ sweep
- TP ที่ midpoint หรืออีกฝั่งของ range

## 14.3 เหตุผลที่ยังไม่เลือกเป็น primary
- นิยามให้คงเส้นคงวายากกว่า
- ไวต่อข่าวและ spike มากกว่า
- เสี่ยงเกิด false positives มากกว่าใน gold

---

# 15) Parameter List (สำหรับ Dev/Test)

## Core Parameters
- `BiasLookbackH1 = 20`
- `SwingStrength = 2`
- `RangeLookbackM15 = 8`
- `ATRPeriodM15 = 14`
- `BreakBufferATR = 0.15`
- `RetestZoneATR = 0.20`
- `RetestTimeoutBars = 4`
- `ConfirmationBodyMin = 0.50`
- `ConfirmationClosePercent = 0.35`
- `MaxBreakoutCandleATR = 2.0`
- `SLBufferATR = 0.10`
- `MinStopDistance`
- `MaxStopDistance`
- `RiskPercent = 0.25`
- `MaxTradesPerDay = 2`
- `DailyLossCapPercent = 1.0`
- `BreakEvenAtR = 1.0`
- `TakeProfitR = 2.0`
- `MaxBarsInTrade = 8`
- Session inputs ตาม timezone ที่ใช้จริง
- Spread / volatility thresholds

---

# 16) Example State Machine

## State 0: Idle
- รอ bias ชัดบน H1
- ถ้า neutral อยู่ Idle ต่อ

## State 1: Range Tracking
- คำนวณ M15 setup range จาก lookback
- รอ breakout valid

## State 2: Breakout Armed
- breakout ผ่านแล้ว
- เริ่มนับ timeout เพื่อรอ retest

## State 3: Retest Observed
- ราคาแตะ retest zone
- รอ confirmation candle ปิด

## State 4: Entry Ready
- เมื่อ confirmation ผ่าน ให้ส่งคำสั่งที่แท่งถัดไป

## State 5: Trade Active
- จัดการ SL / BE / TP / time exit

## State 6: Done / Cooldown
- บันทึกผล
- ไม่ re-enter range เดิมเกิน 1 ครั้ง
- กลับไป Idle หรือ Range Tracking ตามเงื่อนไข

---

# 17) Assumptions and Uncertainty

## Assumptions สำคัญ
1. ทองคำควรเป็นตลาดแรก เพราะหลักฐานสาธารณะพูดถึง “กราฟทองทุกวัน” ชัดที่สุด
2. Confirmation-based entry เป็นแกนที่เหมาะที่สุดสำหรับ bot เวอร์ชันแรก
3. การใช้ H1 + M15 เป็น compromise ระหว่างความชัดของ bias และความถี่ของสัญญาณ
4. กลยุทธ์ควรหลีกเลี่ยงการเข้าในแท่ง breakout แรกเพื่อไม่ไล่ราคา
5. v0.1 ควรเน้นความเสถียรและการ debug ง่าย มากกว่าการ maximize จำนวนสัญญาณ

## Uncertainty ที่ต้องยอมรับ
1. ไม่มีหลักฐานตรงว่า MTA ใช้กฎ breakout-retest แบบนี้จริง
2. ไม่มีหลักฐานตรงว่า MTA ใช้ H1/M15 จริง
3. ไม่มีหลักฐานตรงว่า MTA ใช้ fixed 2R หรือ break-even ที่ 1R
4. พฤติกรรมของ XAUUSD อาจเปลี่ยนตาม regime ทำให้ parameter ต้องทดสอบอย่างระมัดระวัง

---

# 18) Programmability Notes

ส่วนนี้มีไว้ map จาก “ภาษากลยุทธ์” ไปเป็น “ตรรกะที่เขียนโค้ดได้”

## Rule Mapping Table

### A. Bias / Structure
- **กฎเชิงแนวคิด:** เทรดเฉพาะฝั่งเดียวกับโครงสร้างใหญ่
- **ตรรกะที่เขียนโค้ดได้:**
  - สร้าง swing map จาก fractal H1
  - หา last confirmed swing high/low
  - ตรวจว่ามี close เหนือ swing high ล่าสุดหรือ close ต่ำกว่า swing low ล่าสุด
  - ถ้าเงื่อนไขไม่ครบให้ bias = neutral

### B. Setup Range
- **กฎเชิงแนวคิด:** ใช้กรอบราคาล่าสุดเป็นจุดตัดสิน breakout
- **ตรรกะที่เขียนโค้ดได้:**
  - คำนวณ highest high / lowest low จาก M15 ย้อนหลัง 8 แท่ง โดยไม่รวมแท่งปัจจุบัน

### C. Valid Breakout
- **กฎเชิงแนวคิด:** breakout ต้องปิดเลยระดับอย่างมีนัย ไม่ใช่แค่ไส้เทียน
- **ตรรกะที่เขียนโค้ดได้:**
  - Buy: close[1] > RangeHigh + BreakBuffer
  - Sell: close[1] < RangeLow - BreakBuffer
  - และ abs(close-open) / ATR <= MaxBreakoutCandleATR

### D. Retest
- **กฎเชิงแนวคิด:** หลัง breakout ต้องกลับมาทดสอบก่อน
- **ตรรกะที่เขียนโค้ดได้:**
  - หลัง breakout เก็บ breakout level และเริ่มตัวนับ bar
  - ถ้า low/high ของแท่งถัด ๆ มาแตะโซน ± RetestZoneWidth ภายใน timeout ถือว่า retest observed

### E. Confirmation Candle
- **กฎเชิงแนวคิด:** เข้าเมื่อเห็น rejection/continuation ชัด
- **ตรรกะที่เขียนโค้ดได้:**
  - body = abs(close-open)
  - range = high-low
  - body/range >= ConfirmationBodyMin
  - buy: close > breakout level และ (high-close)/range <= ConfirmationClosePercent
  - sell: close < breakout level และ (close-low)/range <= ConfirmationClosePercent

### F. Entry Timing
- **กฎเชิงแนวคิด:** ไม่เข้า intrabar
- **ตรรกะที่เขียนโค้ดได้:**
  - ประเมิน setup เมื่อแท่ง M15 ปิดเท่านั้น
  - ส่ง order ที่ open ของแท่งถัดไปหรือทันทีหลังปิดแท่งถัดจาก event callback ที่ยืนยันแล้ว

### G. Stop Loss
- **กฎเชิงแนวคิด:** stop ต้องอยู่หลังจุดที่ setup ใช้ไม่ได้
- **ตรรกะที่เขียนโค้ดได้:**
  - Buy: min(confirm low, retest swing low) - SLBuffer
  - Sell: max(confirm high, retest swing high) + SLBuffer
  - ถ้า stop distance นอกช่วง min/max ให้ reject trade

### H. Take Profit / Break-even
- **กฎเชิงแนวคิด:** เป้าหมายเรียบง่ายก่อน
- **ตรรกะที่เขียนโค้ดได้:**
  - initial TP = entry + 2R หรือ entry - 2R
  - หาก floating profit >= 1R ให้ move SL to BE
  - หาก bars in trade > MaxBarsInTrade ให้ close market

### I. Risk Controls
- **กฎเชิงแนวคิด:** จำกัดความเสียหายต่อวันและต่อ setup
- **ตรรกะที่เขียนโค้ดได้:**
  - lot size = equity * RiskPercent / stop distance value
  - reset daily counters เมื่อขึ้นวันใหม่ตาม timezone ที่กำหนด
  - ถ้า tradesToday >= MaxTradesPerDay หรือ dailyPnL <= -DailyLossCapPercent ให้ block entries

### J. No-trade Filters
- **กฎเชิงแนวคิด:** หลีกเลี่ยงตลาดที่เงียบเกินหรือแรงผิดปกติ
- **ตรรกะที่เขียนโค้ดได้:**
  - เช็ก ATR อยู่ในช่วงที่อนุญาต
  - เช็ก spread <= max spread
  - เช็ก current time อยู่ใน allowed sessions
  - เช็กไม่ใช่ Friday cutoff

---

# 19) สิ่งที่ Dev Agent ควรระวัง

1. ระวัง look-ahead bias ตอนหา swing ด้วย fractal เพราะ swing จะ confirm หลังจากมีแท่งถัดไปครบ
2. session logic ต้อง deterministic และ test ได้
3. แยก state ของ breakout แต่ละ range ให้ชัด เพื่อไม่ให้ retest ไป match กับ range เก่า
4. ห้ามอ่านค่าแท่งที่ยังไม่ปิดเพื่อตัดสิน confirmation ใน backtest ถ้าต้องการความสอดคล้อง
5. บันทึก log ทุกครั้งที่ setup ถูก reject พร้อมเหตุผล เช่น neutral bias, no retest, spread high, stop too wide

---

# 20) สิ่งที่ Test Agent ควรทดสอบต่อ

1. sensitivity ของ `RangeLookbackM15`
2. sensitivity ของ `RetestTimeoutBars`
3. sensitivity ของ `BreakBufferATR` และ `RetestZoneATR`
4. ผลต่างระหว่าง TP 1.5R กับ 2.0R
5. ผลของ BE ที่ 1R เทียบกับไม่ใช้ BE
6. ความแตกต่างระหว่าง London only, NY only, และ London+NY
7. robustness ต่อ spread/slippage ที่สูงขึ้น

---

# 21) สรุปสำหรับ handoff

## สิ่งที่เรารู้แน่
- ควรเริ่มจากทองคำ
- ควรใช้กรอบ daily analysis + actionable entry + confirmation
- ควรใช้กฎที่อธิบายบนกราฟได้ง่ายและเขียนโค้ดได้จริง

## สิ่งที่เราตีความ
- เลือก breakout-retest-confirmation เป็น setup หลัก
- เลือก H1/M15 เป็น TF แรก
- เลือก risk model แบบ conservative และ time exit แบบ intraday

## สิ่งที่ยังไม่รู้
- กฎจริงของ MTA
- parameter ที่เหมาะที่สุดของทองในแต่ละ regime
- ว่า BE/2R จะ robust แค่ไหนในข้อมูลจริง

## สิ่งที่ทีมถัดไปต้องตัดสินใจ
- จะใช้ fractal แบบใดเป็น swing detector
- จะ encode session/timezone อย่างไร
- จะเริ่ม backtest ด้วย TP 2R + BE 1R หรือไม่ใช้ BE ก่อน
- จะเปิด secondary setup ในโค้ดรอบแรกหรือเก็บไว้ก่อน
