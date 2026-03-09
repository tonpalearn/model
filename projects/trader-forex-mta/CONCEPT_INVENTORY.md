# CONCEPT_INVENTORY.md

คลังแนวคิดสำหรับใช้คัดเลือกไปทำ strategy spec ของ bot cTrader/cAlgo

> หมายเหตุสำคัญ
> - เอกสารนี้สรุปจากคอนเทนต์สาธารณะที่เข้าถึงได้จำกัด + องค์ความรู้ price action สาธารณะทั่วไปที่ใกล้เคียง
> - แต่ละ concept เป็น “ผู้สมัครสำหรับการ formalize” ไม่ใช่ข้อยืนยันว่าเพจ MTA ใช้ concept นี้ในรูปแบบเดียวกันทุกประการ
> - ไม่มี concept ใดในเอกสารนี้ที่รับประกันผลกำไร

---

## 1) Market Structure

**Definition**
- การนิยามสภาพตลาดจากลำดับ swing high / swing low เช่น HH/HL = แนวโน้มขึ้น, LH/LL = แนวโน้มลง, ไม่ชัด = sideway/range

**Programmable?**
- ได้ค่อนข้างดี

**Data needed**
- OHLC
- นิยาม pivot/swing เช่น fractal 3-5 แท่ง หรือ lookback window
- อาจต้องใช้ timeframe หลัก + timeframe ย่อย ถ้าจะทำ multi-timeframe bias

**Caveats**
- คำว่า swing ใหญ่/เล็กมีผลต่อผลลัพธ์มาก
- ถ้าตั้ง pivot ไวเกินไปจะเกิด noise เยอะ
- ถ้าตั้งช้าเกินไปจะเข้าเทรดช้า

---

## 2) Trend Continuation

**Definition**
- เข้าเทรดตามแนวโน้มหลัก หลังเกิด pullback หรือ consolidation สั้น แล้วมีสัญญาณไปต่อ

**Programmable?**
- ได้

**Data needed**
- Market structure state
- ระดับ pullback zone เช่น prior breakout level, moving swing, or percentage retracement
- แท่งยืนยัน continuation

**Caveats**
- ถ้า trend filter หยาบเกินไปจะโดนเข้า late
- ถ้า pullback เล็กเกินไปจะโดน false continuation บ่อย
- ถ้าผูกกับอินดิเคเตอร์มากไปอาจหลุดจากโจทย์ price action ล้วน

---

## 3) Breakout

**Definition**
- ราคาทะลุระดับสำคัญ เช่น range high/low, swing high/low, session high/low หรือ consolidation box

**Programmable?**
- ได้มาก

**Data needed**
- นิยาม level ที่จะถูก break
- เงื่อนไขว่าทะลุด้วย wick หรือ close
- buffer ขั้นต่ำ เช่น pips หรือ ATR fraction
- ตัวกรอง spread/time/session

**Caveats**
- breakout มี false signal สูงโดยธรรมชาติ
- ถ้าไม่ใช้ buffer จะเกิดการ break หลอกบ่อย
- ต้องตัดสินใจว่าตรวจ intrabar หรือ bar close

---

## 4) False Breakout

**Definition**
- ราคาทะลุระดับสำคัญชั่วคราว แต่ไม่ยืนเหนือ/ใต้ระดับนั้น และกลับเข้ากรอบเดิมภายในช่วงเวลาสั้น

**Programmable?**
- ได้ แต่ต้องนิยามเข้ม

**Data needed**
- level reference
- เงื่อนไข break แล้ว return
- จำนวนแท่งสูงสุดที่ยอมให้กลับเข้ากรอบ
- เกณฑ์ close back inside / rejection wick / body ratio

**Caveats**
- ถ้านิยามหลวมจะจับ pattern มั่ว
- ถ้านิยามเข้มเกินไป จำนวนสัญญาณจะน้อยมาก
- ต้องระวัง look-ahead bias ถ้ารอแท่งยืนยันหลายแท่งโดยไม่กำหนดล่วงหน้า

---

## 5) Liquidity Sweep

**Definition**
- ราคาวิ่งไปกวาด stop เหนือ swing high หรือใต้ swing low ก่อนกลับตัวหรือกลับเข้าช่วงเดิม

**Programmable?**
- ได้ระดับกลางถึงสูง

**Data needed**
- swing high/low ล่าสุด
- sweep threshold เช่น ทะลุอย่างน้อย X pips
- reclaim condition เช่น close กลับใน range หรือปิดกลับใต้/เหนือ level
- session/time filter

**Caveats**
- “liquidity” เป็นคำอธิบายเชิงพฤติกรรมตลาด ไม่ได้สังเกตตรงจาก OHLC โดยตรง
- สิ่งที่โค้ดจับจริง ๆ คือ sweep/reclaim pattern ไม่ใช่ liquidity แท้
- ช่วงข่าวแรงอาจเกิด sweep รัวจนระบบพังได้

---

## 6) Retest

**Definition**
- หลัง breakout ราคารีบาวด์กลับมาทดสอบระดับที่เพิ่งทะลุ ก่อนวิ่งต่อ

**Programmable?**
- ได้ดี

**Data needed**
- breakout level
- เขต retest width เช่น ±X pips หรือ ATR-based zone
- timeout ภายในกี่แท่งต้องเกิด retest
- rejection/confirmation condition ตอนแตะโซน

**Caveats**
- ระยะโซน retest ถ้ากว้างไปจะเสียความแม่นของ setup
- ถ้าแคบเกินไปจะพลาดหลายจังหวะ
- ตลาดบางช่วง breakout แล้วไม่ retest เลย ต้องยอมพลาดหรือออกแบบ alternate entry

---

## 7) Session Filters

**Definition**
- จำกัดการเปิดออเดอร์เฉพาะช่วงเวลาที่กำหนด เช่น London, New York, overlap และ/หรือหลีกเลี่ยง session เงียบ

**Programmable?**
- ได้ง่ายมาก

**Data needed**
- server timezone / exchange timezone mapping
- ช่วงเวลาเปิดเทรดที่อนุญาต
- อาจรวมวันในสัปดาห์ และ blackout period ก่อนข่าวแรง

**Caveats**
- timezone ผิดเพียง 1 ชั่วโมงก็ทำให้ผล backtest เพี้ยนมาก
- DST เป็นจุดพังยอดนิยม
- session filter ที่ดูดีในอดีตอาจไม่คงเส้นคงวาใน regime ใหม่

---

## 8) Candle Confirmation

**Definition**
- ใช้คุณสมบัติของแท่งราคาเป็นตัวอนุมัติ setup เช่น close เหนือระดับ, close กลับเข้ากรอบ, body ใหญ่กว่า wick, engulfing, rejection bar

**Programmable?**
- ได้

**Data needed**
- OHLC ของแท่งยืนยัน
- ratio body/wick
- ตำแหน่ง close เทียบกับ range ของแท่ง
- relation กับ zone/level ที่เกี่ยวข้อง

**Caveats**
- ชื่อ pattern แบบ discretionary หลายแบบคลุมเครือ
- ควรนิยามเป็นตัวเลข เช่น body >= 60% ของ range
- ถ้าดูเฉพาะแท่งเดียวอาจไวเกินไป

---

## 9) Stop Placement Ideas

**Definition**
- วิธีตั้ง stop loss ตามโครงสร้างราคา เช่น หลัง swing high/low, นอก zone, นอก sweep extreme, หรือมี buffer เพิ่ม

**Programmable?**
- ได้

**Data needed**
- จุด invalidation ของ setup
- buffer pips หรือ ATR multiplier
- min/max stop distance
- spread consideration

**Caveats**
- stop ที่สั้นเกินไปโดน noise กินง่าย
- stop ที่กว้างเกินไปทำให้ RR แย่
- ต้องผูกกับ position sizing เสมอ ไม่ใช่แค่ระยะ stop อย่างเดียว

---

## 10) Take-Profit Ideas

**Definition**
- วิธีปิดกำไร เช่น fixed R multiple, opposite structure, previous liquidity pool, session close, trailing stop, partial exit

**Programmable?**
- ได้

**Data needed**
- entry price, stop distance
- target structure/level
- trailing rules
- partial exit rules หากมี

**Caveats**
- TP แบบ adaptive ทำให้ผลลัพธ์ไวต่อ parameter มาก
- partial exit เพิ่มความซับซ้อนในการทดสอบ
- trailing stop บางแบบดูดีเฉพาะช่วง trend ชัด

---

## 11) Risk Controls

**Definition**
- กฎควบคุมความเสี่ยงระดับระบบ เช่น risk per trade, max daily loss, max consecutive losses, max open positions, spread filter, slippage guard

**Programmable?**
- ได้มาก และควรมี

**Data needed**
- account equity/balance
- stop distance สำหรับ sizing
- spread real-time/backtest assumptions
- daily PnL / streak state

**Caveats**
- backtest มักจำลอง slippage ไม่สมจริง
- risk control ที่ดีช่วยลดการพัง แต่ไม่ได้ทำให้ strategy มี edge อัตโนมัติ
- ถ้า rule เยอะเกินไปอาจกลายเป็น overfit management

---

## 12) Range / Consolidation Detection

**Definition**
- ตรวจช่วงที่ตลาดบีบตัวอยู่ในกรอบแคบ ก่อนเลือกเล่น breakout หรือ fade กลับเข้ากรอบ

**Programmable?**
- ได้

**Data needed**
- lookback high/low range
- ATR/compression threshold
- minimum bars in range

**Caveats**
- range detection ไวต่อ parameter มาก
- ใช้คู่กับ breakout logic ได้ดี แต่ถ้ากำหนด box ผิด ระบบจะสับสนบ่อย

---

## 13) Structure Break / BOS / CHoCH แบบ simplified

**Definition**
- ใช้การทำลาย swing structure เพื่อบอก continuation หรือ reversal เช่น break of structure (BOS) และ change of character (CHoCH) ในความหมายเชิงง่าย

**Programmable?**
- ได้ระดับกลาง

**Data needed**
- swing map
- กฎว่าจุดไหนถือเป็น external/internal structure
- close-based or wick-based break

**Caveats**
- concept นี้เป็นจุดที่คนมักนิยามไม่ตรงกัน
- ถ้าจะเขียน bot ควรลดศัพท์หรูและกลับไปนิยามเป็น swing break ธรรมดา

---

## 14) Multi-Timeframe Bias

**Definition**
- ใช้ timeframe ใหญ่กำหนด bias แล้วใช้ timeframe เล็กหาจังหวะเข้า

**Programmable?**
- ได้

**Data needed**
- OHLC หลาย timeframe
- mapping bias TF กับ entry TF
- synchronization rule เวลาปิดแท่ง

**Caveats**
- complexity เพิ่มขึ้นมาก
- ถ้าซ้อนหลาย TF เกินไปจะ debug ยาก
- ระวังปัญหา index/time alignment

---

## 15) News / Volatility Avoidance

**Definition**
- หลีกเลี่ยงการเข้าออเดอร์ก่อน/หลังข่าวแรง หรือเมื่อ spread/volatility ผิดปกติ

**Programmable?**
- ได้ ถ้ามี feed ข่าวหรืออย่างน้อยมี proxy เช่น spread filter และ blackout window

**Data needed**
- economic calendar feed หรือ manual schedule
- spread / volatility threshold
- blackout duration ก่อนและหลัง event

**Caveats**
- ถ้าไม่มีข้อมูลข่าวโดยตรง ต้องใช้ proxy ซึ่งไม่สมบูรณ์
- ผล backtest จะต่างจาก live มากถ้าไม่จำลองผลข่าว

---

# Shortlist ที่เหมาะกับการทำ bot ก่อน

## A. Breakout + Retest + Candle Confirmation
**ทำไมควรเริ่มจากชุดนี้**
- rule ชัด
- ใช้ OHLC เป็นหลัก
- debug ง่าย
- ต่อยอดเป็น cBot ได้เร็ว

**องค์ประกอบขั้นต่ำ**
- level detection
- breakout definition
- retest zone
- confirmation candle
- SL หลัง zone/swing
- TP แบบ fixed R หรือ structure target
- session + spread filter

## B. Liquidity Sweep + Reclaim Reversal
**ทำไมควรลองเป็นชุดสอง**
- สอดคล้องกับ price action สมัยใหม่
- มีเอกลักษณ์มากกว่าระบบ breakout ธรรมดา

**องค์ประกอบขั้นต่ำ**
- prior swing reference
- sweep threshold
- reclaim close
- entry trigger
- invalidation at sweep extreme
- conservative session filter

## C. Trend Pullback Continuation
**ทำไมควรเก็บไว้เป็นชุดสาม**
- มักทน noise ได้ดีกว่า reversal setup
- เหมาะกับคู่เงินที่วิ่งเป็น phase ชัด

**องค์ประกอบขั้นต่ำ**
- trend state
- pullback zone
- continuation candle
- stop below/above pullback swing
- TP at next structure / fixed R

---

# สิ่งที่ทีมถัดไปต้องตัดสินใจ

1. จะใช้ concept ไหนเป็นแกนหลักของ strategy รุ่นแรก
2. จะนิยาม swing/level ด้วยวิธีไหน
3. จะใช้ close-based หรือ wick-based confirmation
4. จะเปิดออเดอร์ on close, on next bar, หรือ stop order
5. จะกำหนด stop/TP แบบ fixed หรือ adaptive
6. จะรองรับ multi-timeframe ตั้งแต่รุ่นแรกหรือไม่
7. จะมี news filter จริง หรือเริ่มจาก session/spread filter ไปก่อน
