# แผนปฏิบัติการโครงการ Trader Forex MTA

## วัตถุประสงค์
สร้างระบบเทรดอัตโนมัติในรูปแบบ **cTrader/cAlgo cBot** ที่อ้างอิงแรงบันดาลใจจากแนวคิด Price Action ที่เผยแพร่แบบสาธารณะของเพจ MTA และแหล่งสาธารณะใกล้เคียง โดยต้องแปลงแนวคิดให้เป็น **กฎที่ชัดเจน ทดสอบได้ ทำซ้ำได้** และมีกรอบการประเมินผลที่เข้มงวด

> หมายเหตุสำคัญ: โครงการนี้มีเป้าหมายเพื่อสร้าง **ระบบที่ตรวจสอบและทดสอบได้** ไม่ใช่การรับประกันผลตอบแทน ไม่ควรตีความผล backtest ว่าจะเกิดซ้ำในตลาดจริงเสมอ

---

## หลักการกำกับโครงการ
1. **ใช้เฉพาะข้อมูลสาธารณะที่เข้าถึงได้**
2. **แยก “แนวคิด” ออกจาก “กฎ” ให้ชัด**
3. **เริ่มจาก setup แคบและชัดก่อน** ไม่ทำระบบครอบจักรวาลตั้งแต่รอบแรก
4. **ทดสอบแบบหักล้างได้**: ต้องมี failure criteria ชัดเจน
5. **ห้ามสรุปเกินข้อมูล**: ถ้าบางส่วนเป็นการตีความ ต้องระบุว่าเป็นการตีความของทีมวิเคราะห์
6. **เอกสารต้องพอให้ผู้ใช้รันเองได้**

---

## ภาพรวม workflow แบบ multi-agent

### 1) Research Agent
หน้าที่:
- สำรวจว่าเพจ/แหล่งสาธารณะของ MTA มีข้อมูลเข้าถึงได้มากน้อยเพียงใด
- เก็บ recurring concepts เช่น market structure, trend, breakout, liquidity, pullback, session, confirmation, risk management
- แยกสิ่งที่เป็นคำอธิบายเชิงแนวคิด ออกจากสิ่งที่พอจะตีความเป็นกฎได้
- ทำ evidence log ว่าแต่ละแนวคิดมาจากแหล่งใด

ผลลัพธ์ที่ต้องส่ง:
- รายการ concept ที่พบพร้อมความถี่/ความสำคัญ
- source notes พร้อมลิงก์หรือคำอธิบายแหล่งที่มา
- ตารางแยก: “แนวคิดเชิงปรัชญา” / “แนวคิดที่พร้อมแปลงเป็นกฎ” / “แนวคิดที่ยังคลุมเครือ”

### 2) Analysis Agent
หน้าที่:
- เลือก setup ที่ตีความได้ชัดที่สุด 1-2 แบบ
- แปลง concept เป็นกฎเชิงระบบ: entry, exit, stop loss, take profit, position sizing, filters, invalidation
- กำหนดนิยามเชิงข้อมูล เช่น swing high/low, breakout, retest, candle confirmation
- ระบุ assumption ที่ใช้ในการตีความ

ผลลัพธ์ที่ต้องส่ง:
- Strategy Spec เวอร์ชัน machine-readable/pseudocode
- state machine หรือ decision tree ของระบบ
- parameter list พร้อมช่วงค่าที่จะนำไป optimize
- edge cases และ ambiguity list

### 3) Dev Agent
หน้าที่:
- สร้างโครงสร้างโปรเจกต์ cTrader/cAlgo cBot
- implement logic ตาม Strategy Spec เท่านั้น
- ทำให้ parameter configurable
- เพิ่ม logging/debug hooks เพื่อช่วยตรวจสอบ logic
- เขียนเอกสารการ build/run เบื้องต้น

ผลลัพธ์ที่ต้องส่ง:
- source code ของ cBot
- README/SETUP สำหรับ build และใช้งาน
- mapping ระหว่าง strategy rules กับโค้ด
- known technical limitations

### 4) Test Agent
หน้าที่:
- ตรวจความสอดคล้องระหว่าง logic ในเอกสารกับ logic ในโค้ด
- นิยามแผน backtest, walk-forward, sensitivity analysis, stress scenarios
- สร้างเกณฑ์ตัดสินว่าระบบ “ยังไม่ผ่าน” เมื่อใด
- สรุปผลเชิงวิทยาศาสตร์ ไม่ cherry-pick

ผลลัพธ์ที่ต้องส่ง:
- Test Plan
- Backtest matrix (symbol / timeframe / date ranges / sessions / spread assumptions)
- Metrics dashboard definition
- failure report template และ go/no-go recommendation

### 5) PM Agent
หน้าที่:
- กำหนดลำดับงาน, เกณฑ์ผ่านแต่ละเฟส, และเงื่อนไขวนรอบ
- ประสาน handoff ระหว่าง agent
- คุม scope ไม่ให้ strategy drift
- สรุปความเสี่ยง ข้อจำกัด และ next steps

ผลลัพธ์ที่ต้องส่ง:
- แผนปฏิบัติการโครงการ
- handoff contract ระหว่าง agent
- milestone, acceptance criteria, risk register
- สรุปสถานะพร้อมคำแนะนำรอบถัดไป

---

## ขอบเขตงานรอบแรก (แนะนำ)
เพื่อให้ทดสอบได้จริง ควรจำกัดรอบแรกดังนี้:
- เลือก **1 ตลาดหลักก่อน**: เช่น EURUSD
- เลือก **1-2 timeframe หลัก**: เช่น M15/H1 หรือ M5/M15
- เลือก **1 setup หลัก** และ **1 setup สำรอง** เท่านั้น
- ใช้ risk model แบบเรียบง่ายก่อน: fixed fractional risk หรือ fixed lot ใน test mode
- กำหนด session filter ชัดเจน เช่น London / New York overlap

เหตุผล:
- ลดความคลุมเครือ
- ทำให้ backtest และ debugging เร็วขึ้น
- ง่ายต่อการพิสูจน์ว่า edge มาจาก setup จริง ไม่ใช่จาก parameter fitting มากเกินไป

---

## แผนดำเนินงานตามเฟส

## Phase 1 — Research Discovery
### เป้าหมาย
สกัดแนวคิดการเทรดเชิงสาธารณะให้มากพอสำหรับสร้าง strategy hypothesis

### งานย่อย
- ตรวจการเข้าถึงแหล่งข้อมูลสาธารณะของ MTA
- เก็บโพสต์/คำอธิบาย/สื่อที่พบซ้ำบ่อย
- จัดหมวด concept
- ประเมินความชัดเจนของแต่ละ concept

### Deliverables
- `RESEARCH_NOTES.md`
- `CONCEPT_INVENTORY.md`
- `SOURCE_MAP.md`

### Acceptance Criteria
- มี concept อย่างน้อย 5-10 รายการที่จัดหมวดแล้ว
- มี shortlist 2-3 concept cluster ที่เหมาะกับการทำระบบ
- ระบุชัดว่าข้อไหนเป็น inference ของทีม ไม่ใช่ข้อความตรงจากแหล่ง

### Exit Condition
- PM อนุมัติให้เดินหน้าสู่ analysis เมื่อมี concept ที่สามารถนิยามเชิงกฎได้อย่างน้อย 1 setup

---

## Phase 2 — Strategy Analysis & Formalization
### เป้าหมาย
แปลงแนวคิดให้เป็น strategy specification ที่ programmer ใช้ได้ทันที

### งานย่อย
- เลือก setup หลัก 1-2 แบบ
- นิยาม trigger เงื่อนไข entry/exit
- นิยาม SL/TP/trailing/breakeven หากใช้
- กำหนด risk model และ session filter
- เขียน pseudocode
- ระบุ ambiguity และจุดเสี่ยงต่อ overfitting

### Deliverables
- `STRATEGY_SPEC.md`
- `RULE_DEFINITIONS.md`
- `PSEUDOCODE.md`
- `PARAMETER_MATRIX.md`

### Acceptance Criteria
- ทุกกฎมีนิยามเชิงข้อมูล ไม่ใช้คำกำกวมล้วนๆ เช่น “แท่งสวย”, “แรง”, “ชัด” โดยไม่มีตัววัด
- สามารถตอบได้ว่า “เข้าเมื่อไหร่ / ไม่เข้าเมื่อไหร่ / ออกเมื่อไหร่ / ยกเลิก setup เมื่อไหร่”
- มี parameter list พร้อม default และช่วง optimize

### Exit Condition
- Dev Agent อ่านแล้ว implement ได้โดยไม่ต้องเดา logic หลัก

---

## Phase 3 — Development
### เป้าหมาย
สร้าง cBot ที่ทำงานตาม spec ได้ครบและตรวจสอบย้อนหลังได้

### งานย่อย
- ตั้งโครงสร้างโปรเจกต์
- implement market structure helpers / setup detectors / order execution / risk management
- เพิ่ม parameters
- เพิ่ม logs สำหรับ debug
- เขียนเอกสาร build/run

### Deliverables
- source code cBot
- `SETUP.md`
- `IMPLEMENTATION_MAP.md`
- `CHANGELOG.md`

### Acceptance Criteria
- โค้ด compile ได้
- parameter หลักปรับได้จาก UI/inputs
- มี log ที่ช่วย trace decision ได้
- ตรรกะหลักตรงกับ spec

### Exit Condition
- พร้อมส่งต่อให้ Test Agent ตรวจและทำ backtest

---

## Phase 4 — Validation & Backtest
### เป้าหมาย
ประเมินว่าระบบมีคุณสมบัติพอสำหรับทดลองต่อหรือควรยุติ/ย้อนกลับ

### งานย่อย
- ตรวจ logic consistency ระหว่าง spec กับ code
- backtest หลายช่วงเวลา (in-sample / out-of-sample)
- ทดสอบหลาย market regimes: trend / range / volatility expansion
- sensitivity analysis ของ parameter หลัก
- stress test ต่อ spread/slippage assumptions
- สรุป failure modes

### Deliverables
- `TEST_PLAN.md`
- `BACKTEST_RESULTS.md`
- `VALIDATION_SUMMARY.md`
- `FAILURE_ANALYSIS.md`

### Metrics ขั้นต่ำที่ควรรายงาน
- จำนวนเทรด
- win rate
- average win / average loss
- profit factor
- expectancy ต่อเทรด
- max drawdown
- recovery factor
- Sharpe/Sortino (ถ้าสภาพข้อมูลรองรับ)
- exposure time
- consecutive losses สูงสุด
- ผลแยกตาม session / timeframe / regime

### Failure Criteria ตัวอย่าง
ถ้าเกิดข้อใดข้อหนึ่ง ต้องถือว่า “ยังไม่ผ่าน”
- จำนวนเทรดน้อยเกินจนสรุปไม่ได้
- ผลลัพธ์ดีเฉพาะช่วง in-sample แต่พังใน out-of-sample
- กำไรพึ่งพา parameter แคบมากผิดปกติ
- max drawdown ไม่สัมพันธ์กับผลตอบแทน
- spread/slippage เล็กน้อยแล้วผลลัพธ์เปลี่ยนจากบวกเป็นลบอย่างรุนแรง
- logic mismatch ระหว่างเอกสารกับโค้ด

### Exit Condition
- PM ตัดสินใจว่าจะ
  1. เดินหน้าทำ live-sim/paper trading
  2. ปรับกฎแล้ววนกลับ Phase 2
  3. ปิดโครงการ setup นี้

---

## Phase 5 — Delivery & Operational Readiness
### เป้าหมาย
ส่งมอบสิ่งที่ผู้ใช้สามารถอ่าน เข้าใจ และนำไปทดลองรันเองได้

### Deliverables
- source code ล่าสุด
- คู่มือติดตั้งและใช้งาน
- strategy documentation
- test/validation summary
- known limitations
- next iteration proposal

### Acceptance Criteria
- ผู้ใช้เข้าใจขั้นตอน build/run/backtest
- ทราบข้อจำกัดและความเสี่ยง
- มีเอกสารพอสำหรับให้ agent รอบถัดไปทำงานต่อได้

---

## ลำดับเวลาแนะนำ

### Sprint 0 — Project Framing
- ยืนยันตลาด/TF/scope รอบแรก
- ตั้งชื่อ setup เวอร์ชันแรก
- กำหนดรูปแบบเอกสารกลาง

### Sprint 1 — Research
- เก็บข้อมูลสาธารณะ
- สกัด concept
- shortlist setup

### Sprint 2 — Analysis
- formalize rules
- สร้าง pseudocode และ parameter matrix

### Sprint 3 — Build
- implement cBot V0
- debug logic flow

### Sprint 4 — Validate
- backtest + analyze failure modes
- สรุป go/no-go

### Sprint 5 — Iterate or Deliver
- ถ้าผลน่าสนใจ: ปรับปรุง V1
- ถ้าผลไม่ผ่าน: บันทึกข้อค้นพบและหยุด setup นี้อย่างมีหลักฐาน

---

## Decision Gates ของ PM

### Gate A — หลัง Research
ถามว่า:
- มีข้อมูลสาธารณะพอหรือไม่
- มี setup ที่นิยามเป็นกฎได้หรือไม่
- มี risk ของการตีความเกินจริงหรือไม่

### Gate B — หลัง Analysis
ถามว่า:
- กฎชัดพอให้เขียนโค้ดหรือยัง
- ยังมี ambiguity จุดไหนบ้าง
- setup นี้ซับซ้อนเกินจำเป็นไหม

### Gate C — หลัง Development
ถามว่า:
- โค้ดตรง spec หรือไม่
- trace/debug decision ได้หรือไม่
- พร้อมทดสอบอย่างมีวินัยหรือยัง

### Gate D — หลัง Validation
ถามว่า:
- ระบบ robust หรือ overfit
- สมควร iterate ต่อหรือควร kill idea
- ถ้าเดินหน้าต่อ ต้องแก้ตรงไหนก่อน

---

## ความเสี่ยงหลักของโครงการ
1. **ข้อมูลต้นทางไม่ชัดพอ** → ทำให้การตีความ strategy เพี้ยน
2. **แนวคิดสอนเทรดไม่เท่ากับระบบเทรดอัตโนมัติ** → ต้อง formalize เพิ่มเอง
3. **overfitting** → โดยเฉพาะถ้าใช้ parameter มากเกินไป
4. **data quality risk** → historical data, spread, timezone, session alignment
5. **execution gap** → ผล backtest ไม่สะท้อน live execution
6. **scope creep** → พยายามยัดหลาย setup เร็วเกินไป

แนวทางลดความเสี่ยง:
- จำกัด setup รอบแรก
- แยก in-sample / out-of-sample ชัดเจน
- บันทึก assumption ทุกข้อ
- ทำ sensitivity analysis
- ใช้เอกสาร handoff แบบเข้มงวด

---

## Definition of Done ระดับโครงการรอบแรก
โครงการรอบแรกถือว่าเสร็จเมื่อมีครบ:
- concept research ที่อ้างอิงแหล่งสาธารณะ
- strategy spec ที่ชัดและ implement ได้
- cBot ที่ compile/run ได้
- validation report ที่มี metrics และ failure criteria
- เอกสารสำหรับผู้ใช้รันเองได้

ไม่จำเป็นต้องพิสูจน์ว่า “ทำกำไรแน่นอน” จึงจะถือว่าโครงการสำเร็จในเชิงวิศวกรรม

---

## ข้อเสนอเชิงปฏิบัติสำหรับรอบแรก
แนะนำให้ทีมเริ่มด้วยกรอบนี้:
- ตลาด: EURUSD
- Timeframe: M15 เป็น execution timeframe, H1 เป็น bias filter
- setup: breakout + retest หรือ structure break + confirmation เพียง 1 แบบก่อน
- risk: เสี่ยงคงที่ต่อเทรด
- session: London/NY ช่วงที่ liquidity สูง

เหตุผล: เป็นกรอบที่ตรวจสอบง่ายและเหมาะกับการ formalize เป็น cBot มากกว่าแนว discretionary กว้างๆ

---

## ผลลัพธ์ที่ PM ต้องการจาก agent ทุกตัว
ทุก agent ต้องส่งมอบงานในรูปแบบที่:
- trace กลับไปยังข้อกำหนดต้นทางได้
- มี assumption list
- ระบุสิ่งที่ “รู้” กับ “ตีความ” แยกกัน
- ส่งต่อให้งานเฟสถัดไปได้โดยไม่ต้องเดาใหม่

นี่คือแกนกลางของโครงการ: **จากแนวคิดสาธารณะ → กฎที่ชัด → โค้ด → การทดสอบที่หักล้างได้**
