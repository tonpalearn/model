# TEST_REVIEW.md

อัปเดตล่าสุด: 2026-03-09
สถานะ: Conceptual / Static Review เท่านั้น

เอกสารนี้เป็นการรีวิวเชิงทดสอบก่อนนำระบบไป backtest จริงใน cTrader สำหรับโปรเจกต์ `MtaGoldBreakoutRetestBot`

## ขอบเขตที่ตรวจ

อ่านและเทียบกันระหว่าง:
- `STRATEGY_SPEC.md`
- `DEV_NOTES.md`
- `IMPLEMENTATION_MAP.md`
- `SETUP.md`
- `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`

### สิ่งที่ยัง **ไม่ได้** ทำ
- ยังไม่ได้ compile ใน cTrader Automate จริง
- ยังไม่ได้ run backtest จริงกับ historical data
- ยังไม่ได้ validate กับ broker-specific symbol spec ของ XAUUSD จริง

ดังนั้นรีวิวนี้เป็น **adversarial pre-backtest review**: ตั้งใจหาจุดที่อาจพัง, เพี้ยน, หรือทำให้ backtest หลอกตา

---

## สรุปภาพรวม

โค้ดเวอร์ชันนี้ถือว่าเป็น baseline ที่มีโครงสร้างค่อนข้างดี:
- แยก state หลักได้พอสมควร
- มี session filter, ATR filter, spread filter, risk guard, break-even, time exit
- พยายามหลีกเลี่ยง intrabar look-ahead ในหลายจุด

แต่ก่อนจะเชื่อผล backtest ควรถือว่ายังมีความเสี่ยงสำคัญหลายเรื่อง:
1. **Bias H1 อาจตีความผิดจากสเปก** และอาจให้สัญญาณ neutral/bullish/bearish เพี้ยน
2. **Position sizing อาจไม่ตรงความเสี่ยงจริง** ขึ้นกับ `PipValue/LotSize` ของ broker
3. **Daily loss cap ใช้ฐาน equity ปัจจุบัน** ไม่ใช่ equity ต้นวัน ทำให้ guard ไม่ deterministic ตามสเปก
4. **Session และ timezone ยังเสี่ยงเพี้ยน** ถ้า map ไม่ตรงกับ broker/server time
5. **กฎบางข้อในสเปกยัง implement ไม่ครบ** เช่น range memory / re-entry control แบบจริงจัง
6. **MinRewardRisk ปัจจุบันแทบไม่มีผลจริง** เพราะเอาไปเทียบกับ `TakeProfitR` ตรง ๆ ไม่ได้วัด reward ไปยังโครงสร้างจริง

สรุปแบบตรงไปตรงมา: **ยังไม่ควรเชื่อผลกำไรของ backtest รอบแรก** จนกว่าจะผ่านการตรวจแก้จุดเหล่านี้

---

## สิ่งที่ทำได้ดี

### 1) โครง state machine ชัดพอสำหรับ debug
มี flow ประมาณนี้:
- หา bias
- หา breakout
- arm setup
- รอ retest
- รอ confirmation
- เข้า order
- manage trade

จุดนี้ดีเพราะทำให้ visual validation บนกราฟทำได้ง่ายกว่าระบบที่เขียนรวมทุกอย่างแบบ monolithic

### 2) พยายามใช้ข้อมูลแท่งปิดเป็นหลัก
- M15 ใช้ `lastClosedIndex = Bars.Count - 2`
- H1 bias ก็อ้างอิง last closed bar

จุดนี้ช่วยลด look-ahead bias ได้ในระดับหนึ่ง

### 3) มี risk guard พื้นฐานที่จำเป็น
- max trades/day
- max losses/day
- daily loss cap
- max one concurrent position

แม้ implementation ยังมีข้อกังวล แต่โครงหลักมีครบ

### 4) มีการ block สภาพตลาดที่ไม่เหมาะสม
- ATR ต่ำ/สูงเกินไป
- spread สูง
- นอก session
- Friday cutoff

จุดนี้เหมาะกับระบบทองคำที่มักโดน noise และ spike

---

## ประเด็นน่ากังวลหลัก

## A. Logic Bug / Design Gap ที่ควรแก้ก่อนเชื่อ backtest

### A1) H1 bias มีโอกาสไม่ตรงกับเจตนาของสเปก
โค้ด:
- `CalculateH1Bias()`
- `FindLastSwingHigh()`
- `FindLastSwingLow()`

ประเด็น:
- สเปกระบุว่า bullish ต้องครบทั้ง
  1. close เหนือ midpoint
  2. ปิดเหนือ swing high ล่าสุด
  3. swing low ล่าสุดยังไม่ถูกทำลาย
- แต่ implementation ใช้:
  - `brokeSwingHigh = close > lastSwingHigh.Price`
  - `brokeSwingLow = close < lastSwingLow.Price`
  - `swingLowStillIntact = lowest > lastSwingLow.Price || close > lastSwingLow.Price`

ปัญหา:
- เงื่อนไข `swingLowStillIntact` และ `swingHighStillIntact` ไม่ได้ตรวจว่า **หลังจากเกิด swing แล้วเคยถูกทำลายหรือยัง**
- มันตรวจเพียง low/high ใน lookback รวม และ current close เท่านั้น
- โดยเฉพาะ `close > lastSwingLow.Price` มักเป็นจริงง่ายมาก ทำให้เงื่อนไข “ยังไม่ถูกทำลาย” อ่อนเกินไป

ผลกระทบ:
- bias อาจกลายเป็น bullish/bearish ทั้งที่โครงสร้างจริงเสียไปแล้ว
- หรือ bias อาจ neutral มากเกินหรือน้อยเกิน ขึ้นกับ sequence ของ swing
- backtest อาจดูดี/แย่เพราะนิยาม bias ผิด ไม่ใช่เพราะ strategy จริงดี/แย่

ข้อเสนอ:
- เปลี่ยนเป็นตรวจแบบ sequence-aware:
  - หา `lastConfirmedSwingHigh` และ `lastConfirmedSwingLow`
  - ตรวจว่า **ตั้งแต่ swing low ล่าสุดเกิดมา** ไม่มี close ต่ำกว่าจุดนั้น หากจะเรียกว่า low ยัง intact
  - หรือเก็บ explicit structure state เช่น HH/HL/LH/LL แบบง่าย

ระดับความรุนแรง: **สูง**

---

### A2) Daily loss cap ไม่ deterministic ตามสเปก
โค้ด:
- `CanOpenNewTrade()`

ปัจจุบัน:
```csharp
var maxDailyLoss = Account.Equity * (DailyLossCapPercent / 100.0);
if (_closedNetToday <= -maxDailyLoss)
    return false;
```

ปัญหา:
- ใช้ `Account.Equity` ปัจจุบัน ณ เวลาตรวจ ไม่ใช่ equity ตอนเริ่มวัน
- ถ้าวันนั้นกำไรขึ้น threshold จะขยับขึ้น
- ถ้าขาดทุนไปบางส่วน threshold จะหดลง
- ไม่ตรงกับความหมายปกติของ “daily loss cap” ที่ควร lock จากทุนต้นวัน

ผลกระทบ:
- guard รายวันเปรียบเทียบผลข้ามวัน/ข้าม backtest ยาก
- optimization อาจ exploit guard ที่ลื่นไหลตาม equity

ข้อเสนอ:
- เก็บ `StartOfDayEquity`
- คำนวณ `DailyLossCapAmount = StartOfDayEquity * cap%`
- รีเซ็ตตอนเปลี่ยน trading day

ระดับความรุนแรง: **สูง**

---

### A3) Position sizing มีความเสี่ยงผิดหน่วยสูง
โค้ด:
- `CalculateVolumeInUnits()`

ปัจจุบัน:
```csharp
var pipValuePerUnit = Symbol.PipValue / Symbol.LotSize;
var volume = riskAmount / (stopLossPips * pipValuePerUnit);
```

ปัญหา:
- แนวคิดนี้อาจใช้ได้ในบาง broker แต่ XAUUSD broker-to-broker ต่างกันมาก
- `PipValue`, `LotSize`, `PipSize`, `VolumeInUnitsMin`, `Volume step` อาจไม่สัมพันธ์กันแบบที่คาด
- โดยเฉพาะทอง บางที่ 1 lot = 100 oz, บางที่ contract model ต่างกัน

ผลกระทบ:
- เสี่ยงมากที่สุดจุดหนึ่ง เพราะต่อให้ entry logic ถูก ถ้า size เพี้ยน backtest จะโกหกทันที
- อาจเสี่ยงจริงมากกว่าที่คิดหลายเท่า หรือเล็กเกินจนผลไม่ meaningful

ข้อเสนอ:
- ทำ test matrix ต่อ broker/symbol spec จริง
- log รายการต่อไปนี้ทุกครั้งก่อนส่ง order:
  - Equity
  - riskAmount
  - stopDistance
  - stopPips
  - PipSize
  - PipValue
  - LotSize
  - raw volume
  - normalized volume
- ถ้า cTrader API มี helper ที่ปลอดภัยกว่าในการคำนวณ volume ตาม risk ให้พิจารณาใช้

ระดับความรุนแรง: **สูงมาก**

---

### A4) MinRewardRisk ปัจจุบันแทบไม่ทำหน้าที่จริง
โค้ด:
```csharp
var expectedRewardRisk = TakeProfitR;
if (expectedRewardRisk < MinRewardRisk)
    InvalidateSetup(...)
```

ปัญหา:
- นี่ไม่ใช่การคำนวณ reward/risk ของ setup จริง
- เป็นแค่การเช็กว่า parameter `TakeProfitR` น้อยกว่า `MinRewardRisk` หรือไม่
- ถ้าตั้ง TP 2R และ min 1.5R ก็ผ่านทุก setup เสมอ

ผลกระทบ:
- ขัดกับสเปกที่พูดถึงการกรอง setup ที่ระยะไปยัง target โครงสร้างถัดไปไม่คุ้ม
- backtest อาจรับ trade ในบริเวณที่ชน structure ใกล้มาก

ข้อเสนอ:
- ถ้ายังไม่มี structure-target model ชัดเจน ให้ลบ parameter นี้ออกชั่วคราวเพื่อไม่ให้หลอกว่ามี guard แล้ว
- หรือ implement target proxy เช่น nearest swing / range extension / session high-low

ระดับความรุนแรง: **กลางถึงสูง**

---

### A5) Daily counters reset ตาม `Server.Time.Date` อาจไม่ตรง session logic จริง
โค้ด:
- `GetTradingDay(Server.Time)`
- `ResetDailyCountersIfNeeded()`

ปัญหา:
- วันเทรดรีเซ็ตตาม UTC date เพราะ bot ตั้ง `[Robot(TimeZone = TimeZones.UTC)]`
- แต่ผู้ใช้/โบรกเกอร์อาจตีความ “วันเทรด” ตาม server time, New York close, หรือ local trading session

ผลกระทบ:
- daily loss cap, max trades/day, max losses/day อาจรีเซ็ตคนละจังหวะกับที่ตั้งใจ
- โดยเฉพาะ XAUUSD ช่วง rollover/late US มีผลเยอะ

ข้อเสนอ:
- ระบุ trading-day convention ให้ชัดเจนในโค้ดและเอกสาร
- ถ้าใช้ UTC ก็ต้องทดสอบทั้งระบบบน UTC consistently

ระดับความรุนแรง: **กลางถึงสูง**

---

### A6) กฎ “ห้าม re-enter range เดิมเกิน 1 ครั้ง” ยังไม่ถูก enforce จริง
เอกสารระบุเองแล้วใน `DEV_NOTES.md` และ `IMPLEMENTATION_MAP.md`

ผลกระทบ:
- backtest อาจ overtrade range เดิมโดยไม่ได้ตั้งใจ
- ทำให้ผลดูดีเกินจริงในช่วงตลาด chop ที่มีหลาย fake retest

ข้อเสนอ:
- เก็บ persistent range/setup memory เช่น hash จาก rangeHigh/rangeLow + breakout direction + session bucket
- กำหนด cooldown หลัง invalidate หรือหลัง close trade

ระดับความรุนแรง: **กลาง**

---

## B. Ambiguity / Spec Drift

### B1) Entry timing ไม่ตรงสเปกแบบเป๊ะ
สเปกบอก:
- เข้า market order ที่เปิดของแท่งถัดไปหลัง confirmation candle ปิด

implementation:
- ยิง market order ใน event `OnBar()` ของแท่งใหม่ โดยใช้ engine fill ตามสภาพ backtest/live

นี่เป็น approximation ที่ practical แต่ต้องยอมรับว่า:
- visual expectation กับ fill จริงอาจไม่ตรง
- ถ้าช่วงข่าวหรือ gap/slippage มาก ผลจะต่าง

ข้อเสนอ:
- ตอน backtest ให้เทียบอย่างน้อย 20-30 ดีลแบบ visual/manual เพื่อดูว่า fill ต่างจาก “next bar open” มากแค่ไหน

---

### B2) Confirmation bar ยอมให้ “แท่งเดียวกัน” ทั้งแตะ zone และ confirm
โค้ดอนุญาตให้แท่งเดียวกัน:
- แตะ retest zone
- มี body ratio ผ่าน
- ปิดกลับเหนือ/ต่ำกว่า breakout level

นี่ไม่ผิดเสมอไป แต่เป็นจุดที่ควรตัดสินใจให้ชัดว่า:
- ต้องการ “first touch rejection bar” แบบนี้หรือไม่
- หรืออยากให้มี state แยกว่า bar หนึ่งใช้ทำ retest แล้วรอ bar ถัดไปยืนยัน

ผลกระทบ:
- จำนวนสัญญาณและ character ของระบบอาจต่างพอสมควร

---

### B3) Spread filter ใช้ค่า absolute เดียว
สเปกเปิดทางทั้ง rolling spread factor และ absolute max spread ชั่วคราว

implementation ใช้:
- `Symbol.Spread > MaxSpread`

ปัญหา:
- ง่ายดี แต่ broker-specific มาก
- spread ของ XAUUSD ในตลาดจริงแกว่งตาม session/news ชัด

ข้อเสนอ:
- อย่างน้อยควรทดสอบ sensitivity ต่อ `MaxSpread`
- ถ้าเป็นไปได้เพิ่ม logging spread ตอน entry และ reject

---

## C. Edge Cases ที่ต้องระวัง

### C1) หาก H1 data โหลดไม่ครบใน backtest บาง environment bias จะเพี้ยนหรือไม่ทำงาน
โค้ดเช็ก `_h1Bars.Count` ก่อนใช้ แต่ยังต้องยืนยันว่า cTrader backtest environment โหลด H1 history พอจริง

สิ่งที่ควรทดสอบ:
- เปิด backtest ช่วงต้น dataset
- ดูว่าช่วงแรก bot เงียบเพราะ history ไม่พอ หรือมี behavior แปลก

---

### C2) `ModifyPosition()` สำหรับ break-even อาจชน stop-level constraint ของ broker
กรณี broker มี min stop distance หรือ freeze level:
- ระบบอาจย้าย SL ไม่สำเร็จ
- ตอนนี้ถ้าไม่สำเร็จ แค่ไม่ set `BreakEvenMoved`

ข้อเสนอ:
- log เหตุผลของ `modifyResult` เมื่อไม่สำเร็จ
- ทดสอบโดยเฉพาะช่วง spread กว้าง/price ขยับเร็ว

---

### C3) Time exit ใช้จำนวน bars ไม่ใช่ elapsed market time จริง
โดยหลักใช้ได้ แต่ต้องเข้าใจว่า:
- ถ้าช่วง illiquid หรือ data gap เกิดขึ้น behavior อาจไม่เหมือน “ถือเกิน X ชั่วโมง” แบบนาฬิกาจริง

---

### C4) Session filter ใช้แค่ `barTime.Hour`
นั่นแปลว่า filter ละเอียดแค่ระดับชั่วโมง ไม่ดูนาที

ผลกระทบ:
- ถ้าต้องการ 2-4 ชั่วโมงแรกแบบแม่นขึ้น อาจยังหยาบไป
- สำหรับ M15 ยังพอรับได้ แต่ควรยอมรับความหยาบนี้ในผลทดสอบ

---

### C5) Invalidation ฝั่งตรงข้ามของ range ใช้ close ของแท่งล่าสุดเท่านั้น
อาจยังปล่อยกรณี wick หลุดแรงแล้วกลับเข้ามา ซึ่งอาจดีหรือไม่ดีก็ได้ ขึ้นกับเจตนาของ strategy

นี่ไม่ใช่ bug ชัด ๆ แต่ควรกำหนดให้แน่:
- invalid เมื่อ wick ทะลุ หรือเฉพาะ close ทะลุ

---

## D. Broker-Specific Risks สำหรับ XAUUSD

### D1) Quote format ต่างกัน
ตัวอย่างที่พบบ่อย:
- 0.01 เป็น tick หลัก
- บาง broker ใช้ digit ต่างกัน
- spread และ stop distance requirements ต่างกัน

ผลกระทบ:
- `MinStopDistance = 2.0` และ `MaxStopDistance = 15.0` อาจแคบ/กว้างคนละเรื่องระหว่าง broker

---

### D2) Contract size / lot model ต่างกัน
ต่อให้ชื่อ symbol เป็น `XAUUSD` เหมือนกัน ก็อาจไม่เท่ากันด้าน:
- 1 lot เท่ากับกี่ oz
- pip value ต่อ lot
- min volume / step

นี่ผูกโดยตรงกับความถูกต้องของ position sizing

---

### D3) Backtest spread model อาจไม่สะท้อน live
ถ้าใช้ fixed spread หรือ tick model ที่หยาบ:
- ระบบ breakout-retest บนทองอาจดูสวยเกินจริง
- โดยเฉพาะตอน London open / NY open / ข่าวแรง

---

### D4) Slippage ใน live สำคัญกว่าที่โค้ดสื่อ
entry แบบ market หลัง confirmation มี sensitivity ต่อ slippage พอสมควร โดยเฉพาะ XAUUSD ช่วง volatility สูง

ข้อสรุป:
- ถ้าผล backtest ดีแต่พึ่ง market order timing หนัก ต้องทำ sensitivity test เรื่อง slippage ก่อนเชื่อ

---

## สิ่งที่ควรแก้ก่อน “trusting a backtest”

### ระดับเร่งด่วนที่สุด
1. แก้/ทดสอบ bias H1 ให้ตรงนิยามมากขึ้น
2. ยืนยัน position sizing กับ broker spec จริง
3. เปลี่ยน daily loss cap ให้ล็อกตาม equity ต้นวัน
4. เพิ่ม logging ที่ละเอียดเรื่อง entry sizing, BE modify, reject reason

### ระดับสำคัญมาก
5. ตัดสินใจให้ชัดเรื่อง confirmation bar แบบ same-bar retest+confirm
6. enforce re-entry/range memory
7. ทำ timezone/session convention ให้ตายตัวและบันทึกไว้ในผลทดสอบ

### ระดับเสริมแต่ควรมี
8. เพิ่ม exported trade log / setup log เพื่อ audit
9. ทำ visual validation แบบ manual อย่างน้อยหนึ่งชุดตัวอย่างต่อ session
10. ทดสอบหลาย spread/slippage assumptions

---

## รายการทดสอบเชิง scenario ที่ควรทำกับกราฟจริง

1. **Bullish breakout แล้ว retest ภายใน 1-2 แท่ง**
   - ต้องเข้าได้ถ้า confirmation ผ่าน
2. **Breakout แล้วไม่ retest ภายใน 4 แท่ง**
   - ต้อง invalidate
3. **Breakout แล้ว bias H1 เปลี่ยนก่อน entry**
   - ต้อง invalidate
4. **Breakout แล้วแท่ง retest หลุดอีกฝั่งของ range แบบ close หลุด**
   - ต้อง invalidate
5. **แท่ง breakout ใหญ่กว่า 2 ATR**
   - ต้อง reject ไม่ arm setup
6. **spread สูงเกิน threshold ตอน signal มาสวยมาก**
   - ต้องไม่เข้า
7. **stop distance แคบเกิน min / กว้างเกิน max**
   - ต้อง reject
8. **กำไรถึง 1R แล้ว broker ไม่ยอม move SL**
   - ต้อง log เหตุผล
9. **โดน 2 loss ในวันเดียว**
   - ต้องหยุดเปิด trade ใหม่
10. **ข้ามวันตาม timezone ที่ใช้**
   - daily counters ต้อง reset ตรงเวลาที่นิยามไว้

---

## คำตัดสินแบบตรงไปตรงมา

เวอร์ชันนี้ **พร้อมสำหรับ “pre-backtest hardening”** แต่ยังไม่พร้อมสำหรับการเชื่อผลลัพธ์เชิง performance

ถ้าจะเอาไป backtest ตอนนี้ก็ทำได้ แต่ต้องถือว่าเป็น:
- รอบหา bug
- รอบตรวจ unit/price conventions
- รอบ validate session/bias/entry behavior

ไม่ใช่รอบสรุปว่า strategy ดีหรือไม่ดี

ถ้าจะใช้คำเดียวสั้น ๆ: **ยังเร็วเกินไปที่จะเชื่อ equity curve**
