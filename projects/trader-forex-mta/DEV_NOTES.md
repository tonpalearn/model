# DEV_NOTES.md

## สิ่งที่ทำสำเร็จ
- สร้าง baseline cBot สำหรับ cTrader/cAlgo
- วางโครง logic ตาม setup หลัก Breakout + Retest + Confirmation
- เพิ่ม parameters หลักให้ปรับได้จาก UI
- ใส่ risk guards รายวัน, session filter, ATR filter, spread filter, break-even, time exit
- เพิ่ม log สำหรับ state สำคัญ เช่น arm / reject / invalidate / entry / break-even / close

---

## Assumptions ที่ใช้ใน implementation

### 1) ใช้ XAUUSD เป็นตลาดเดียว
ตรงตาม strategy spec รอบแรก และยังไม่พยายาม generalize multi-symbol

### 2) ใช้ M15 chart เป็น execution bars
โค้ดคาดหวังว่า cBot ถูก attach กับกราฟ M15

### 3) H1 bias อ่านจาก bars ของ `BiasTimeFrame`
ค่า default คือ `Hour` และ logic ใช้ last closed H1 bar เท่านั้น

### 4) ราคาเข้า order ใช้ market execution หลัง confirmation bar ปิด
ในเชิง backtest/automation นี่เป็น approximation ที่ practical กว่าการพยายามบังคับ “open ของแท่งถัดไป” แบบเป๊ะทุก engine

### 5) ค่าหน่วยราคา/stop distance ใช้ตาม price units ของ symbol
ไม่ได้ hardcode point model ของทอง เพราะ broker อาจต่างกัน

---

## Simplifications ที่ตั้งใจทำ

### A. Bias logic ถูก simplify ให้ deterministic
แม้ spec จะพูดถึง structure แบบมี nuance แต่ implementation รุ่นแรกใช้:
- midpoint of lookback range
- last valid swing high/low
- close break logic

เพื่อให้ trace ง่ายก่อน

### B. ไม่มี partial take profit
ใช้ fixed full exit ที่ TP ตาม `TakeProfitR`

### C. ไม่มี trailing stop
ตรงตาม scope v0.1 ที่ต้องการลดความซับซ้อน

### D. ไม่มี external news filter
ยังไม่เชื่อม economic calendar หรือ blackout event ภายนอก

### E. ยังไม่เก็บประวัติ “range เดิม” แบบเต็มระบบ
จึงยังไม่ได้ enforce เรื่อง re-enter range เดิมเกินหนึ่งครั้งอย่างสมบูรณ์

---

## สิ่งที่ Test Agent ควร challenge ก่อน

### 1) Bias logic บน H1 เข้ม/หลวมเกินไปหรือไม่
โดยเฉพาะเงื่อนไข swing intact และการใช้ midpoint + last swing break

### 2) Volume sizing ถูกต้องกับ XAUUSD broker ที่จะใช้จริงหรือไม่
ให้ตรวจละเอียดเรื่อง:
- `Symbol.PipSize`
- `Symbol.PipValue`
- `Symbol.LotSize`
- minimum volume
- normalized volume behavior

### 3) Stop distance units เหมาะกับ broker data หรือไม่
ค่าตั้งต้น 2.0 ถึง 15.0 อาจกว้าง/แคบไม่เท่ากันตาม quote format

### 4) Break-even offset ควรเป็น price units, pips, หรือ spread-adjusted model
เวอร์ชันนี้ใช้ price units ตรง ๆ เพื่อให้ง่าย แต่ควรทดสอบว่าตรงความต้องการหรือไม่

### 5) Session timing ต้อง map กับ UTC/server time ให้ชัด
ถ้า cTrader backtest ใช้ timezone ต่างจากที่ตีความไว้ ผลจะเพี้ยนทันที

### 6) Entry timing “close bar แล้วส่ง market order” ให้ผลต่างจาก “open next bar” มากแค่ไหน
ควรเปรียบเทียบด้วย visual backtest

### 7) ATR thresholds ควรใช้ fixed absolute values หรือ normalize แบบอื่น
เพราะ volatility ของทองอาจเปลี่ยน regime มาก

---

## Known Technical Risks

1. compile API surface ของ cTrader แต่ละ version อาจต่างกันเล็กน้อย
2. `Symbol.PipValue` ใน backtest/live อาจให้ behavior ต่างกันตาม broker/model
3. H1 bars ที่ดึงผ่าน `MarketData.GetBars()` ต้องตรวจว่าประวัติครบจริงใน environment ที่จะใช้
4. การใช้ `ModifyPosition()` สำหรับ BE ควรทดสอบว่าไม่มี rounding issue กับ stop level constraints

---

## ข้อเสนอรอบถัดไป

1. เพิ่ม persistent setup memory เพื่อกัน re-entry range เดิม
2. แยกไฟล์ helper/classes หากต้องการ maintainability สูงขึ้น
3. เพิ่ม optional analysis logging เป็น structured CSV/JSON ถ้า cTrader environment รองรับ
4. เพิ่ม test checklist สำหรับ scenario-based visual validation
5. ถ้าผล backtest อ่อน ให้กลับไป challenge assumptions ก่อนเพิ่ม complexity
