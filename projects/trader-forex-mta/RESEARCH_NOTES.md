# RESEARCH_NOTES.md

เอกสารนี้เป็นแพ็กวิจัยเบื้องต้นสำหรับใช้ส่งต่อให้ Analysis Agent ในโปรเจกต์ Trader Forex MTA

> สำคัญ: การเข้าถึงคอนเทนต์จาก Facebook page/reels ผ่าน CLI มีข้อจำกัดสูงมาก จึงไม่สามารถยืนยันได้ว่าหลักคิดทั้งหมดของเพจ MTA คืออะไรจากโพสต์สาธารณะเพียงไม่กี่ชิ้นที่มองเห็นได้โดยตรง ข้อมูลด้านล่างจึงแยกชัดระหว่าง **ข้อเท็จจริงที่สังเกตได้**, **การตีความว่ามีแนวคิดซ้ำบ่อย**, และ **คำถามที่ยังเปิดอยู่**
>
> เอกสารนี้ไม่ได้อ้างว่ากลยุทธ์ใดทำกำไรได้แน่นอน และไม่ควรถูกมองว่าเป็นคำแนะนำการลงทุน

---

## 1) Facts observed from public content

### 1.1 สิ่งที่สังเกตได้โดยตรงจากคอนเทนต์สาธารณะที่เข้าถึงได้
1. พบ reel สาธารณะบน Facebook ที่มีข้อความลักษณะว่า
   - “THE MTA WEBSITE is already ready for the launch”
   - มี hashtag ที่เกี่ยวข้องกับ `#forex`, `#trading`, `#pexstrategy`
2. จาก metadata ของ reel ที่ดึงได้ เห็นชื่อผู้เผยแพร่เป็นลักษณะ `Pex`
3. ชื่อ hashtag `pexstrategy` บ่งชี้ว่าคอนเทนต์น่าจะเกี่ยวกับ “strategy” มากกว่าคอนเทนต์แนว motivational ล้วน ๆ แต่จุดนี้ยังเป็นเพียงการตีความระดับต่ำ ไม่ใช่ข้อพิสูจน์ว่ามีกฎเทรดครบระบบ
4. ในไฟล์โปรเจกต์เอง มีการกำหนดทิศทางงานชัดว่าเป้าหมายคือการ “แกะแนวคิด price action จากแหล่งสาธารณะ แล้วแปลงเป็น logic ที่ทดสอบได้”

### 1.2 สิ่งที่สังเกตได้จากแหล่ง public educational content ที่สอดคล้องกับสไตล์ price action ทั่วไป
เมื่อแหล่งจาก Facebook จำกัด จึงอ้างอิงจากองค์ความรู้สาธารณะซึ่งพบซ้ำบ่อยในคอนเทนต์สอน price action / forex ได้แก่
1. การอ่านโครงสร้างตลาด (market structure) ผ่าน higher high / higher low / lower high / lower low
2. การใช้แนวรับแนวต้าน, swing high/low และ breakout เป็นจุดตัดสินใจ
3. การไม่ไล่ราคาแบบสุ่ม แต่รอ retest หรือ candle confirmation หลัง breakout
4. การแยก breakout จริงกับ false breakout / liquidity grab / stop hunt
5. การใช้ session เป็นตัวกรอง เช่น London, New York หรือช่วง overlap
6. การวาง stop loss หลัง swing point, zone หรือ invalidation level แทนการตั้งแบบลอย ๆ
7. การกำหนด take profit ผ่าน fixed R multiple, opposite structure, หรือ liquidity target
8. การมี risk control เช่น จำกัดความเสี่ยงต่อไม้, จำกัดจำนวนไม้ต่อวัน, หยุดเทรดเมื่อเสียต่อเนื่อง

---

## 2) Likely recurring concepts

ส่วนนี้คือ “ข้อสรุปเชิงวิจัย” ว่า ถ้าจะสร้าง bot ที่ได้รับแรงบันดาลใจจากคอนเทนต์สาธารณะแนว MTA/price action ชุด concept ที่น่าจะเกิดซ้ำและเหมาะแก่การ formalize มีดังนี้

### 2.1 Market structure
- นิยามตลาดเป็นขึ้น/ลง/sideway จาก swing ล่าสุด
- เหมาะกับการเขียนระบบ เพราะนิยามเป็นกฎ OHLC ได้
- เป็นรากฐานของ concept อื่นเกือบทั้งหมด

### 2.2 Breakout + Retest
- เงื่อนไขแบบ “ทะลุระดับสำคัญก่อน แล้วรอราคากลับมาทดสอบ” เป็นรูปแบบที่พบซ้ำบ่อยมากในวงการ price action
- เขียนระบบได้ค่อนข้างตรง โดยต้องนิยาม level, ระยะ retest และ timeout ให้ชัด

### 2.3 False breakout / Liquidity sweep
- แนวคิดว่าราคาทะลุ high/low เดิมเพื่อกวาด stop แล้วกลับเข้า range เป็น concept ที่สอดคล้องกับ price action สมัยใหม่มาก
- มีศักยภาพสำหรับ bot แต่เสี่ยงเรื่องการตีความเกินข้อมูล ถ้าไม่กำหนด threshold ชัด

### 2.4 Trend continuation หลัง pullback
- อีก setup ที่เหมาะกับระบบ: ยืนยัน trend แล้วรอพักตัว/ย่อ/รีเทสต์ ก่อนเข้าตามแนวโน้ม
- ข้อดีคือ logic ค่อนข้างเสถียรกว่า “เดาท็อปเดาก้น”

### 2.5 Candle confirmation
- ใช้แท่งยืนยัน เช่น close เหนือ/ใต้ level, rejection wick, engulfing แบบที่แปลงเป็นตัวเลขได้
- ต้องระวังไม่ใช้ภาษากำกวม เช่น “แท่งสวย”, “แรงซื้อแรงขายชัด” ถ้าไม่มีตัวชี้วัดเชิงข้อมูลรองรับ

### 2.6 Session filters
- London open, New York open, overlap, หลีกเลี่ยงเอเชียบางช่วง
- เหมาะกับ bot มาก เพราะเป็น time filter ตรงไปตรงมา

### 2.7 Risk-first logic
- หาก MTA เน้น strategy จริง ระบบที่ต่อยอดได้ควรมี risk cap เป็นองค์ประกอบหลัก ไม่ใช่แค่สัญญาณเข้าออก
- เช่น risk per trade, max daily loss, cooldown หลัง loss streak, spread filter

---

## 3) Researcher interpretation vs evidence

### สิ่งที่เรารู้ค่อนข้างแน่
- มีการใช้คำ/แท็กที่สื่อถึง forex, trading, strategy
- โปรเจกต์นี้ต้องการ bot price action ไม่ใช่ bot อินดิเคเตอร์หนัก
- ชุด concept อย่าง market structure, breakout/retest, false breakout, session filter, stop placement เป็นแกนที่เหมาะกับการ formalize

### สิ่งที่เป็นการตีความ
- เพจ MTA น่าจะสอน price action แบบมีโครงสร้าง setup ซ้ำ ๆ มากกว่าการสอนเชิงแรงบันดาลใจอย่างเดียว
- แนวคิดเรื่อง liquidity sweep, retest, candle confirmation น่าจะเข้ากับสไตล์ที่ต้องการ
- ถ้าต้อง shortlist setup สำหรับ bot ควรเริ่มจาก breakout-retest หรือ liquidity sweep + confirmation มากกว่า setup เชิง discretionary สูง

### สิ่งที่ยังยืนยันไม่ได้
- MTA ใช้ timeframe ใดเป็นหลัก
- เน้นคู่เงินอะไร
- ใช้ bias แบบ multi-timeframe หรือไม่
- มีเงื่อนไข entry/exit ที่เฉพาะตัวกว่าความรู้ price action ทั่วไปหรือไม่
- ใช้ fixed RR, trailing, partial TP, หรือ session close exit แบบใด

---

## 4) Open questions

1. เพจ MTA เน้นคู่เงินหลักอะไร เช่น EURUSD, GBPUSD, XAUUSD หรือหลายสินทรัพย์รวมกัน?
2. Timeframe หลักคือ M5/M15/H1/H4?
3. คำว่า `pexstrategy` หมายถึง framework เฉพาะหรือเป็นเพียง hashtag ทางการตลาด?
4. มีการใช้ confluence อื่นร่วมด้วยหรือไม่ เช่น EMA, VWAP, HTF zone, news avoidance?
5. นิยาม breakout ของ MTA ต้องการ “close outside level” หรือเพียง wick ทะลุ?
6. นิยาม false breakout ต้องกลับเข้ากรอบภายในกี่แท่ง?
7. ใช้ session filter แบบตายตัว หรือดูเฉพาะช่วง volatility สูง?
8. stop placement ใช้หลัง swing, หลัง wick, หลัง zone buffer หรือ ATR?
9. take profit ใช้ fixed RR, liquidity target, structure target หรือ hybrid?
10. มีข้อห้ามเรื่อง spread, slippage, ข่าวแรง, วันศุกร์ปลายตลาด หรือไม่?

---

## 5) Shortlist แนวคิดที่เหมาะสำหรับ Analysis Agent มากที่สุด

### Candidate A: MTA_PA_BreakoutRetest_v0.1
**เหตุผล**
- ตรงกับแนว price action ที่แปลงเป็น rule ได้ง่าย
- ใช้ข้อมูล OHLC + เวลา เป็นหลัก
- เหมาะสำหรับทำ state machine ชัดเจน

**โครงสร้างแนวคิด**
- หา key level จาก swing/range
- รอ breakout ที่ปิดเหนือ/ใต้ระดับ
- รอ retest ภายใน N แท่ง
- ต้องมี confirmation candle
- วาง stop หลัง zone/swing
- TP ที่ 1R/2R หรือ target ที่ opposite structure

### Candidate B: MTA_PA_LiquiditySweepReversal_v0.1
**เหตุผล**
- สอดคล้องกับภาษาของ price action สมัยใหม่และเหมาะกับการทดสอบเป็นระบบ
- ใช้ได้ดีถ้า define sweep + reclaim ชัด

**โครงสร้างแนวคิด**
- ราคากวาด high/low ก่อนหน้า
- ปิดกลับเข้าช่วงเดิม
- มี rejection / body-close confirmation
- เข้าเมื่อ break confirmation candle หรือเข้า on close
- stop ไว้นอก sweep extreme
- TP ที่ mid-range / opposite side / fixed R

### Candidate C: MTA_PA_TrendPullbackContinuation_v0.1
**เหตุผล**
- มัก robust กว่า reversal ล้วน
- ใช้ market structure + pullback + continuation trigger

**โครงสร้างแนวคิด**
- trend ตาม HH/HL หรือ LH/LL
- รอ pullback เข้าหา previous breakout zone หรือ swing area
- มีแท่งกลับตัวตามเทรนด์
- stop หลัง swing pullback
- TP ที่ measured move หรือ next structure target

---

## 6) ข้อจำกัดของงานวิจัยรอบนี้

1. แหล่งอ้างอิงจาก Facebook ที่เข้าถึงได้โดยตรงมีจำกัดมาก จึงยังไม่สามารถสรุปว่าแนวคิดทั้งหมด “มาจาก MTA โดยตรง”
2. งานรอบนี้เป็น research package เพื่อคัด concept ที่เหมาะต่อการ formalize ไม่ใช่ strategy spec สำเร็จรูป
3. หลายคำในโลก price action มีความกำกวมสูง เช่น breakout, sweep, confirmation, strong candle ถ้าไม่กำหนดเชิงตัวเลขจะเกิด overfitting หรือ implement ไม่ตรงกัน
4. ความคิดที่ดูสมเหตุผลเชิง narrative อาจไม่ผ่าน backtest เมื่อเจอ spread, slippage, timezone, data quality และ regime change
5. ห้ามนำเอกสารนี้ไปตีความว่าเป็นการรับรองผลตอบแทนหรือความแม่นยำของระบบในอนาคต

---

## 7) Recommendation to next agent

Analysis Agent ควรเริ่มจาก 2 เส้นทางนี้ก่อน
1. **Breakout + Retest + Candle Confirmation + Session Filter**
2. **Liquidity Sweep + Reclaim + Structure-based Stop/TP**

และควรตอบให้ได้ตั้งแต่ต้นว่า
- level จะนิยามอย่างไร
- breakout ต้องอาศัย close หรือ wick
- retest มี timeout กี่แท่ง
- confirmation candle ต้องมีคุณสมบัติอะไร
- stop/TP จะอิง structure แบบไหน
- setup ใดควรถูกยกเลิกเพราะ spread, session, หรือ news risk
