# AGENT HANDOFFS — Trader Forex MTA

เอกสารนี้กำหนดรูปแบบการส่งต่องานระหว่าง agent เพื่อให้ workflow เดินต่อได้โดยไม่เกิดการเดา logic ซ้ำ

---

## 1) Research Agent → Analysis Agent

### เป้าหมายของ handoff
ส่งมอบ “วัตถุดิบเชิงแนวคิด” ที่มีแหล่งอ้างอิงชัด และพร้อมให้ทีมวิเคราะห์แปลงเป็นกฎ

### สิ่งที่ต้องส่งมอบ
1. รายการ concept ทั้งหมดที่พบ
2. การจัดกลุ่ม concept ตามหัวข้อ เช่น
   - trend / market structure
   - breakout / pullback / retest
   - liquidity / stop hunt
   - entry confirmation
   - risk management
   - session / timing
3. source notes ของแต่ละ concept
4. ระดับความมั่นใจของแต่ละ concept
5. shortlist แนวคิดที่ “เหมาะสำหรับระบบ” มากที่สุด 1-3 ชุด

### รูปแบบไฟล์แนะนำ
- `RESEARCH_NOTES.md`
- `CONCEPT_INVENTORY.md`
- `SOURCE_MAP.md`
- `RESEARCH_SUMMARY.md`

### Template ข้อมูลขั้นต่ำ
สำหรับแต่ละ concept ให้มี:
- ชื่อ concept
- คำอธิบายสั้น
- หลักฐาน/แหล่งที่มา
- พบซ้ำบ่อยแค่ไหน
- เป็นข้อความตรง หรือเป็นการสรุปจากหลายโพสต์
- พร้อมแปลงเป็น rule หรือยัง
- ข้อจำกัด/ความกำกวม

### Acceptance Criteria ก่อนส่งต่อ
- มี evidence พอสมควร ไม่ใช่เดาจากโพสต์เดียวแบบลอยๆ
- แยกชัดว่าอะไรคือ “observed concept” และอะไรคือ “researcher interpretation”
- มี shortlist สำหรับ Analysis Agent อย่างน้อย 1 setup candidate

### คำถามที่ Analysis Agent ต้องตอบได้หลังรับงาน
- concept ไหนควรเอาไป formalize ก่อน
- concept ไหนยังคลุมเครือเกินไป
- concept ไหนเป็นเพียง philosophy ไม่เหมาะกับการเขียน bot

---

## 2) Analysis Agent → Dev Agent

### เป้าหมายของ handoff
เปลี่ยนชุดแนวคิดให้เป็น strategy spec ที่ programmer ใช้ได้ทันทีโดยไม่ต้องตีความแกนหลักใหม่

### สิ่งที่ต้องส่งมอบ
1. ชื่อ strategy/setup ชัดเจน
2. market/timeframe ที่ใช้
3. bias/filter rules
4. setup detection rules
5. entry rules
6. stop loss rules
7. take profit / exit rules
8. invalidation rules
9. risk model
10. parameter list พร้อม default และช่วงค่าที่พิจารณา
11. pseudocode หรือ state machine
12. ambiguity list

### รูปแบบไฟล์แนะนำ
- `STRATEGY_SPEC.md`
- `RULE_DEFINITIONS.md`
- `PSEUDOCODE.md`
- `PARAMETER_MATRIX.md`
- `ASSUMPTIONS.md`

### ตัวอย่างหัวข้อบังคับใน Strategy Spec
- Instrument(s)
- Timeframe(s)
- Session filter
- Market structure definition
- Breakout definition
- Retest definition
- Confirmation candle definition
- Entry trigger
- Position sizing
- Max concurrent positions
- Daily loss stop / trading halt (ถ้ามี)
- Exit hierarchy (SL, TP, time exit, manual close conditions)

### Acceptance Criteria ก่อนส่งต่อ
- Dev Agent สามารถ map กฎแต่ละข้อเป็นฟังก์ชัน/เงื่อนไขในโค้ดได้
- ไม่มีคำอธิบายคลุมเครือโดยไม่มีนิยามเชิงข้อมูลรองรับ
- มีตัวอย่างอย่างน้อย 3 กรณี:
  - valid trade
  - invalid setup
  - canceled setup

### คำถามที่ Dev Agent ต้องตอบได้หลังรับงาน
- ใช้ข้อมูลจากแท่งราคาใดบ้าง
- ต้องตรวจเงื่อนไขตอนเปิดแท่งหรือปิดแท่ง
- มี parameter อะไรที่ต้อง expose ให้ผู้ใช้ปรับได้
- เงื่อนไขใดมี priority สูงสุดเมื่อชนกัน

---

## 3) Dev Agent → Test Agent

### เป้าหมายของ handoff
ส่งมอบ implementation ที่ตรวจสอบได้ว่าตรง spec และพร้อมนำไป validation

### สิ่งที่ต้องส่งมอบ
1. source code ของ cBot
2. คู่มือ build/run/backtest เบื้องต้น
3. implementation map ว่ากฎข้อไหนอยู่ส่วนใดของโค้ด
4. logging/debug instructions
5. known deviations จาก spec ถ้ามี
6. รายการ parameter ที่ใช้งานจริงในโค้ด

### รูปแบบไฟล์แนะนำ
- `SETUP.md`
- `IMPLEMENTATION_MAP.md`
- `DEV_NOTES.md`
- `KNOWN_LIMITATIONS.md`

### สิ่งที่ควรมีใน Implementation Map
- Rule ID / ชื่อกฎ
- คำอธิบายกฎ
- โมดูล/คลาส/เมธอดที่เกี่ยวข้อง
- หมายเหตุเรื่อง edge case

### Acceptance Criteria ก่อนส่งต่อ
- โค้ด compile/run ได้
- parameter names และความหมายตรงกับ spec
- มี log พอให้ Test Agent trace decision path ได้
- ระบุชัดหากมีส่วนใดที่ implement แบบ approximation

### คำถามที่ Test Agent ต้องตอบได้หลังรับงาน
- จะยืนยันอย่างไรว่าโค้ดตรงกับ spec
- จะ reproduce trade decisions ได้อย่างไร
- มีจุดไหนที่เสี่ยงต่อ look-ahead bias หรือ execution artifact

---

## 4) Test Agent → PM Agent

### เป้าหมายของ handoff
ส่งผลการประเมินที่ใช้ตัดสินใจได้จริง ว่าควรเดินหน้าต่อ ปรับ หรือยุติ setup นี้

### สิ่งที่ต้องส่งมอบ
1. Test Plan ที่ใช้จริง
2. ช่วงข้อมูลที่ทดสอบ
3. สมมติฐานเรื่อง spread/slippage/session/timezone
4. สรุปผล metrics
5. sensitivity analysis
6. failure modes
7. go / conditional go / no-go recommendation

### รูปแบบไฟล์แนะนำ
- `TEST_PLAN.md`
- `BACKTEST_RESULTS.md`
- `VALIDATION_SUMMARY.md`
- `FAILURE_ANALYSIS.md`
- `RECOMMENDATION.md`

### Metrics ขั้นต่ำ
- total trades
- win rate
- average R / expectancy
- profit factor
- max drawdown
- longest losing streak
- result by period
- result by session
- result by parameter neighborhood

### Acceptance Criteria ก่อนส่งต่อ
- มีทั้งผลที่ดูดีและผลที่ไม่ดี ไม่เลือกเล่าเฉพาะด้านบวก
- แยก in-sample / out-of-sample ชัดเจน
- ระบุว่า setup fail เพราะอะไร ถ้า fail
- ถ้าจะ recommend ให้ไปต่อ ต้องมีเหตุผลเชิง robustness ไม่ใช่แค่กำไรสูงช่วงเดียว

### คำถามที่ PM Agent ต้องตอบได้หลังรับงาน
- ระบบนี้ควรไป phase ถัดไปหรือไม่
- ถ้าจะ iterate ต้องแก้ที่ research, analysis, หรือ implementation
- ความเสี่ยงหลักของการใช้งานจริงคืออะไร

---

## 5) PM Agent → ผู้ใช้ / รอบถัดไป

### เป้าหมายของ handoff
สรุปโครงการในภาษาที่ตัดสินใจได้ พร้อมแผนรอบถัดไปที่ไม่หลุด scope

### สิ่งที่ต้องส่งมอบ
1. สถานะล่าสุดของแต่ละเฟส
2. สิ่งที่เสร็จแล้ว
3. สิ่งที่ยังไม่ชัด
4. ความเสี่ยงสำคัญ
5. ข้อเสนอรอบถัดไป
6. เอกสารที่ผู้ใช้ต้องอ่านเพื่อรันเอง

### รูปแบบสรุปที่แนะนำ
- Executive summary 1 หน้า
- progress by phase
- go/no-go decision
- next actions 3-5 ข้อ

---

# กติกากลางสำหรับทุก agent

## A. ห้ามข้ามระดับ abstraction
- Research ห้ามสรุปเป็นกฎละเอียดโดยไม่มีหลักฐาน
- Analysis ห้ามแต่ง logic เพิ่มโดยไม่ระบุ assumption
- Dev ห้ามเปลี่ยนกลยุทธ์เองเพื่อให้โค้ดง่ายขึ้นโดยไม่บันทึก
- Test ห้าม optimize ซ้ำจนกลายเป็น redesign strategy โดยไม่ส่งกลับ Analysis

## B. ทุกไฟล์ควรระบุ 3 ส่วนนี้
1. Facts / observed
2. Interpretation / assumption
3. Open questions

## C. ใช้ naming ที่สื่อความหมายและ version ชัด
ตัวอย่าง:
- `MTA_PA_BreakoutRetest_v0.1`
- `MTA_PA_StructureBreakConfirm_v0.1`

## D. ทุก handoff ต้องตอบ 4 คำถามนี้
1. สิ่งที่เรารู้แน่คืออะไร
2. สิ่งที่เราตีความคืออะไร
3. สิ่งที่ยังไม่รู้คืออะไร
4. ทีมถัดไปต้องตัดสินใจอะไร

---

# Definition of Ready ต่อ agent

## Research Agent พร้อมเริ่มเมื่อ
- รู้แหล่งข้อมูลเป้าหมาย
- รู้ว่าต้องเก็บเฉพาะข้อมูลสาธารณะ
- รู้รูปแบบ output ที่ต้องส่ง

## Analysis Agent พร้อมเริ่มเมื่อ
- มี concept shortlist จาก research
- มี source notes ที่พออ้างอิงได้
- มีขอบเขตตลาด/TF เบื้องต้น

## Dev Agent พร้อมเริ่มเมื่อ
- ได้ strategy spec ที่ชัด
- ได้ pseudocode
- ได้ parameter list

## Test Agent พร้อมเริ่มเมื่อ
- ได้ code ที่รันได้
- ได้ implementation map
- ได้ช่วง parameter ที่ต้องทดสอบ

## PM Agent พร้อมสรุปเมื่อ
- มีผลจากแต่ละ agent ครบในระดับที่ตัดสินใจได้

---

# Definition of Done ต่อ handoff
handoff ที่ถือว่า “ใช้ได้จริง” ต้องมีคุณสมบัติครบ:
- อ่านแล้วทีมถัดไปเริ่มงานได้ทันที
- ไม่มี dependency สำคัญที่ซ่อนอยู่ในหัวคนส่งงาน
- มีข้อจำกัดและ assumption ระบุชัด
- สามารถ trace กลับไปยังเอกสารก่อนหน้าได้

หากขาดข้อใดข้อหนึ่ง ให้ถือว่า handoff ยังไม่สมบูรณ์
