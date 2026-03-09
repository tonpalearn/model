# SETUP_CANDIDATES.md

อัปเดตล่าสุด: 2026-03-09
สถานะ: Analysis Shortlist

> เอกสารนี้ใช้เปรียบเทียบ candidate setups สำหรับระบบ XAUUSD ที่ “ได้รับแรงบันดาลใจจากกรอบการสื่อสารของ MTA”
> ไม่ใช่การอ้างว่า MTA ใช้ setup ใด setup หนึ่งตามนี้จริง
>
> หลักคิดในการคัดเลือกคือ:
> 1. ต้องสอดคล้องกับภาพรวมจากงานวิจัย: daily gold analysis + actionable setup + confirmation
> 2. ต้อง formalize เป็นกฎได้
> 3. ต้องเหมาะกับ bot รุ่นแรกที่เน้นความชัดและตรวจสอบได้

---

# 1) เกณฑ์ที่ใช้เปรียบเทียบ

ให้คะแนนเชิงคุณภาพใน 5 มิติ

1. **ความสอดคล้องกับ research framing**
2. **ความชัดในการเขียนโค้ด**
3. **ความเสี่ยงต่อการตีความเกินจริง**
4. **ความง่ายในการ debug / backtest**
5. **ความเหมาะสมเป็น implementation แรก**

ระดับคะแนน:
- สูง
- กลาง
- ต่ำ

---

# 2) Candidate A — Breakout + Retest + Confirmation

## แนวคิด
รอให้ราคา breakout กรอบระยะสั้นในทิศทางเดียวกับ bias ที่ได้จากโครงสร้างใหญ่ จากนั้นรอให้ราคาย้อนมาทดสอบระดับ breakout แล้วค่อยเข้าเมื่อมีแท่งยืนยันว่าราคายังไปต่อ

## ทำไม setup นี้เข้ากับ research
- เข้ากับ framing แบบ “วิเคราะห์กราฟทองทุกวัน” เพราะใช้ key level และ scenario ชัด
- เข้ากับ framing แบบ “signal/actionable setup” เพราะระบุจุดเข้า จุด invalidation และเป้าหมายได้ตรง
- เข้ากับแนวคิด “confirmation-based entry” โดยธรรมชาติ

## จุดแข็ง
- นิยามเป็นกฎได้ชัด
- ใช้ OHLC เป็นหลัก
- อธิบายบนกราฟง่าย
- ไม่ต้องเดาคำศัพท์เชิง discretionary มาก
- เหมาะกับการเริ่ม backtest แบบมีวินัย

## จุดอ่อน
- จะพลาดบางจังหวะที่ breakout แล้วไปต่อเลยโดยไม่ retest
- ถ้า range definition ไม่ดีจะเกิด false breakout บ่อย
- ถ้าตั้ง buffer หลวมเกินไปจะเข้าเยอะและคุณภาพลด

## ความเสี่ยงเชิง implementation
- ปานกลาง
- ความเสี่ยงหลักอยู่ที่การนิยาม range, breakout buffer, และ retest zone

## คะแนนรวม
- สอดคล้องกับ research framing: **สูง**
- เขียนโค้ด: **สูง**
- เสี่ยงตีความเกินจริง: **ต่ำถึงกลาง**
- ง่ายต่อ debug/backtest: **สูง**
- เหมาะเป็น implementation แรก: **สูงมาก**

---

# 3) Candidate B — Liquidity Sweep + Reclaim Reversal

## แนวคิด
เมื่อราคาทะลุ swing high/low หรือ session high/low ไปเล็กน้อย แล้วปิดกลับเข้ากรอบอย่างชัดเจน ให้ตีความว่าเป็นการ sweep แล้วกลับตัว จากนั้นเข้าเทรดสวนจากจุด reclaim

## ทำไม setup นี้น่าสนใจ
- เป็น pattern ที่อธิบายเป็นคลิป/กราฟได้ดี
- ให้ภาพของ “จุดหลอก” และ “จุดยืนยัน” ซึ่งดูเข้ากับคอนเทนต์สไตล์สอนอ่านกราฟ
- มีจุด invalidation ชัดที่ extreme ของ sweep

## จุดแข็ง
- R:R มักดูดีถ้าเข้าใกล้จุด reclaim
- มีเอกลักษณ์มากกว่าระบบ breakout ธรรมดา
- เหมาะกับทองในบางช่วงที่ชอบแทงหลอกก่อนกลับ

## จุดอ่อน
- ไวต่อข่าวและ spike มาก
- คำว่า liquidity sweep เป็นคำอธิบายทางพฤติกรรมตลาด ไม่ได้สังเกตจากข้อมูลตรง ๆ
- ถ้านิยาม reclaim หลวม จะเกิดสัญญาณหลอกเยอะมาก
- การแยก “sweep ที่มีนัย” กับ “noise ปกติ” ทำได้ยากกว่า candidate A

## ความเสี่ยงเชิง implementation
- ค่อนข้างสูง
- เสี่ยง overfit ง่ายถ้าใช้ threshold หลายตัว

## คะแนนรวม
- สอดคล้องกับ research framing: **กลางถึงสูง**
- เขียนโค้ด: **กลาง**
- เสี่ยงตีความเกินจริง: **กลางถึงสูง**
- ง่ายต่อ debug/backtest: **กลาง**
- เหมาะเป็น implementation แรก: **กลาง**

---

# 4) Candidate C — Trend Pullback Continuation

## แนวคิด
เมื่อโครงสร้างใหญ่เป็น trend ชัด ให้รอ pullback เข้าหาโซนที่สนใจ แล้วเข้าเมื่อเกิด continuation signal ในทิศทางของ trend

## ทำไม setup นี้เข้ากับ research
- เข้ากับ daily analysis เพราะใช้ bias และ structure
- เข้ากับ signal framing เพราะทำจุดเข้า/ออกได้
- ใช้ confirmation ได้เช่นกัน

## จุดแข็ง
- ตามแนวโน้ม ไม่สวนแรงหลัก
- มักเสียหายน้อยกว่าการเล่น reversal ถ้า trend ชัดจริง
- ใช้ได้กับสินทรัพย์ที่เกิด trend phase ต่อเนื่อง

## จุดอ่อน
- คำว่า “pullback เข้าโซน” คลุมเครือ ถ้าไม่นิยามโซนให้แน่น
- ถ้าใช้แค่ structure กว้าง ๆ อาจเข้าช้า
- ถ้าเติมเงื่อนไขเพื่อกรองให้ดี จะเริ่มซับซ้อนขึ้นเร็ว

## ความเสี่ยงเชิง implementation
- ปานกลาง
- ความยากอยู่ที่การนิยาม pullback zone และ continuation signal แบบไม่ discretionary เกินไป

## คะแนนรวม
- สอดคล้องกับ research framing: **สูง**
- เขียนโค้ด: **กลางถึงสูง**
- เสี่ยงตีความเกินจริง: **กลาง**
- ง่ายต่อ debug/backtest: **กลาง**
- เหมาะเป็น implementation แรก: **กลางถึงสูง**

---

# 5) Candidate D — Opening Range False Break

## แนวคิด
ใช้กรอบราคาเปิด session (เช่น 30-60 นาทีแรกของ London) เป็น reference range หากราคาทะลุกรอบแล้วกลับเข้ากรอบด้วยแท่งยืนยัน ให้เล่นกลับเข้าสู่ range หรือไปยังอีกฝั่งหนึ่งของกรอบ

## ทำไม setup นี้น่าสนใจ
- มี session logic ชัด
- เหมาะกับ intraday gold ที่มี volatility ช่วงเปิด session
- ทำเป็น content-style chart explanation ได้ง่าย

## จุดแข็ง
- กรอบราคาชัดและ deterministic
- เหมาะกับระบบ intraday ไม่ถือข้ามวัน
- เชื่อมกับพฤติกรรม liquidity ตอน session เปิดได้ดี

## จุดอ่อน
- ใช้ได้เฉพาะบางช่วงเวลา ทำให้จำนวนสัญญาณไม่มาก
- ไวต่อ timezone และ DST มาก
- ถ้า opening range แคบหรือกว้างผิดปกติ ผลลัพธ์จะแกว่งมาก
- เสี่ยงพึ่งพาเงื่อนไขเฉพาะ session จนไม่ robust

## ความเสี่ยงเชิง implementation
- ปานกลาง
- ปัญหาหลักคือ timezone consistency และความเสถียรข้ามช่วงเวลา

## คะแนนรวม
- สอดคล้องกับ research framing: **กลาง**
- เขียนโค้ด: **สูง**
- เสี่ยงตีความเกินจริง: **กลาง**
- ง่ายต่อ debug/backtest: **กลางถึงสูง**
- เหมาะเป็น implementation แรก: **กลาง**

---

# 6) Comparative Summary

## ตารางสรุปเชิงคุณภาพ

- **A. Breakout + Retest + Confirmation**
  - Research fit: สูง
  - Programmability: สูง
  - Over-interpretation risk: ต่ำถึงกลาง
  - Debug/backtest ease: สูง
  - First implementation suitability: สูงมาก

- **B. Liquidity Sweep + Reclaim Reversal**
  - Research fit: กลางถึงสูง
  - Programmability: กลาง
  - Over-interpretation risk: กลางถึงสูง
  - Debug/backtest ease: กลาง
  - First implementation suitability: กลาง

- **C. Trend Pullback Continuation**
  - Research fit: สูง
  - Programmability: กลางถึงสูง
  - Over-interpretation risk: กลาง
  - Debug/backtest ease: กลาง
  - First implementation suitability: กลางถึงสูง

- **D. Opening Range False Break**
  - Research fit: กลาง
  - Programmability: สูง
  - Over-interpretation risk: กลาง
  - Debug/backtest ease: กลางถึงสูง
  - First implementation suitability: กลาง

---

# 7) ทำไมเลือก Candidate A เป็น Primary Setup

## 7.1 เพราะเข้ากับหลักฐานสาธารณะมากที่สุดแบบไม่ฝืนหลักฐาน
จากงานวิจัย เรารู้เพียงค่อนข้างชัดว่า MTA เน้น:
- Forex
- Gold analysis รายวัน
- signal/actionable framing
- confirmation-oriented style ในระดับการตีความ

Candidate A ใช้องค์ประกอบเหล่านี้ครบโดยไม่ต้องสมมติศัพท์เฉพาะหรือ framework ซับซ้อนเกินหลักฐาน

## 7.2 เพราะ formalize ได้ชัดและเหมาะกับ bot รุ่นแรก
ระบบรุ่นแรกไม่ควรเริ่มจาก pattern ที่ตีความยากอย่าง sweep/reclaim หรือ pullback เชิง subjective มากเกินไป

Candidate A ตอบโจทย์นี้ดีที่สุด เพราะสามารถนิยามได้ชัดว่า:
- breakout คืออะไร
- retest คืออะไร
- confirmation candle คืออะไร
- invalidation อยู่ตรงไหน
- stop และ target คิดอย่างไร

## 7.3 เพราะ debug ง่ายกว่า candidate อื่น
ถ้า bot เทรดผิด เราสามารถไล่กลับไปดูได้ชัดว่า fail ที่ step ไหน:
- bias ผิด
- breakout ไม่ valid
- retest ไม่เกิด
- confirmation ไม่ผ่าน
- risk filter block

นี่สำคัญมากสำหรับรอบแรก เพราะช่วยแยกได้ว่า “logic ไม่มี edge” หรือ “implementation ผิด”

## 7.4 เพราะลดความเสี่ยงต่อการอ้างเกินจริง
ถ้าเลือก candidate แบบ liquidity sweep ตั้งแต่แรก จะมีความเสี่ยงสูงกว่าที่ทีมจะเผลอใส่ภาษาหรือสมมติฐานที่ดูเหมือนกำลังอ้าง framework ของผู้สอนต้นทาง ทั้งที่จริงไม่มีหลักฐานตรง

Candidate A เป็น price-action setup ที่เป็นกลางกว่าและปลอดภัยกว่าในเชิง epistemic

## 7.5 เพราะเหมาะกับการต่อยอดภายหลัง
หาก candidate A ทำงานได้ในระดับหนึ่ง สามารถค่อยเพิ่ม sophistication ได้ภายหลัง เช่น
- เพิ่ม opening range filter
- เพิ่ม structure target แบบ adaptive
- เพิ่ม sweep rejection เป็น alternate entry
- เพิ่ม news blackout window

กล่าวคือมันเป็นฐานที่ดีสำหรับ versioning

---

# 8) ทำไม Candidate C เป็นตัวสำรองที่ดีรองจาก A

Trend Pullback Continuation ใกล้เคียงกับ A มากในแง่ mindset และอาจทนตลาดบาง regime ได้ดีกว่า แต่สาเหตุที่ยังไม่เลือกก่อนคือ:
- นิยาม pullback zone ให้คงเส้นคงวายากกว่า retest ของ breakout level
- ถ้าไม่มี breakout reference ที่ชัด อาจเปิดช่องให้เกิด discretion มากขึ้น
- สำหรับทีม dev/test รอบแรก A จะ trace ง่ายกว่า

ดังนั้น C เหมาะเป็น **secondary roadmap candidate** มากกว่าจะเป็นตัวแรก

---

# 9) สิ่งที่ยังไม่แน่ใจและต้องพูดตรง ๆ

1. ไม่มีข้อมูลพอจะยืนยันว่า MTA ใช้ candidate A จริง
2. ไม่มีข้อมูลพอจะบอกว่า MTA หลีกเลี่ยงหรือชอบ sweep/reversal มากกว่ากัน
3. ไม่มีข้อมูลพอจะสรุป timeframe ที่ใช้จริง
4. ไม่มีข้อมูลพอจะยืนยันว่ากลยุทธ์ที่เหมาะกับการสื่อสารบนเพจ จะเหมาะกับการทำ bot โดยตรง
5. XAUUSD เป็นสินทรัพย์ที่ volatility สูง การได้ setup ชัดไม่ได้แปลว่าผล backtest จะดี

ดังนั้นการเลือก candidate A คือการเลือกบนฐานของ
- ความปลอดภัยทางการตีความ
- ความสามารถในการเขียนโค้ด
- ความเหมาะสมต่อการทดสอบแบบหักล้างได้

ไม่ใช่การอ้างว่า “นี่แหละคือวิธีของ MTA”

---

# 10) Recommendation สำหรับทีมถัดไป

## สำหรับ Dev Agent
ให้ implement **Candidate A เพียงตัวเดียวก่อน**
- อย่าใส่ candidate B/C/D เพิ่มในรอบแรก
- expose parameter เฉพาะที่จำเป็น
- ทำ logging ให้เห็นเหตุผลที่ trade/reject ทุกครั้ง

## สำหรับ Test Agent
โฟกัสการหาคำตอบ 3 เรื่อง:
1. setup นี้มีสัญญาณเพียงพอหรือไม่
2. parameter neighborhood กว้างพอหรือแคบจนเสี่ยง overfit
3. gold regime ใดที่ setup นี้พังชัดที่สุด

## สำหรับ PM
ให้ lock scope ว่า
- เวอร์ชันแรก = XAUUSD + H1 bias + M15 execution + breakout-retest-confirmation เท่านั้น
- ถ้าผลไม่ดี อย่ารีบเพิ่มอีก 3 setup พร้อมกัน
- ให้ย้อนกลับมาปรับ assumption ทีละชั้น

---

# 11) สรุปสั้น

Primary setup ที่เหมาะสุดสำหรับ implementation แรกคือ:
**Breakout + Retest + Confirmation ในทิศทางเดียวกับ H1 structure bias บน XAUUSD**

เหตุผลหลัก:
- ตรงกับ research framing มากที่สุดแบบ conservative
- เขียนโค้ดได้ชัดที่สุด
- debug ง่ายที่สุด
- ลดความเสี่ยงต่อการอ้างเกินหลักฐาน
- เหมาะสำหรับใช้เป็น baseline strategy ในรอบแรก
