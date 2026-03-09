# CHANGELOG_ROUND6.md

สถานะ: Round 6 Stop Distance / Sizing / Execution Validation Build

## สรุปสั้น

รอบนี้แก้คอขวดที่ log ล่าสุดชี้ชัดแล้วว่า:

- confirmation ผ่าน
- order precheck ผ่าน
- แต่ตายที่ sizing เพราะ `Stop distance invalid`

ตัวอย่างอาการจริง:

- `GATE SIZING REJECT | reason=Stop distance invalid | stopDistance=21.36 min=1.60 max=18.00`

ดังนั้นรอบนี้ไม่ได้ไปแตะ logic signal ต้นน้ำเป็นหลัก แต่เน้นทำให้ stop-distance layer:

- explicit
- observable
- แยกหน่วยราคา / pips / ticks ชัด
- มี debug path สำหรับ probe/diagnostic
- ยัง conservative ใน normal mode

---

## ไฟล์ที่แก้

### โค้ด
- `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`

### เอกสาร
- `CHANGELOG_ROUND6.md`
- `STOP_DISTANCE_REWORK.md`
- `PARAMETER_PRESETS.md`
- `RUNBOOK_CTRADER.md`
- `SETUP.md`

---

## รายการเปลี่ยนหลัก

### 1) แยก stop-distance evaluation เป็นก้อนชัดเจน

เพิ่ม object ภายในสำหรับเก็บข้อมูล stop-distance โดยตรง เช่น:

- entry price
- stop price
- ระยะ stop แบบราคา
- ระยะ stop แบบ pips
- ระยะ stop แบบ ticks
- base band
- effective band
- context ว่ากำลัง strict หรือ relaxed

ผลลัพธ์:
- เวลา reject จะไม่เห็นแค่เลข stopDistance ลอยๆ
- เห็นครบว่าระยะถูกตีความยังไงบน symbol นั้นจริง

---

### 2) เพิ่ม log เฉพาะสำหรับ stop-distance และ sizing math

มี log ใหม่ที่ควรมองหา:

- `STOP DISTANCE LIMITS | ...`
- `STOP CHECK #... | ...`
- `GATE SIZING PASS | ...`
- `GATE SIZING REJECT | reason=Stop distance invalid | ...`
- `SIZING | ... stopDist=... stopPips=... stopTicks=... ...`
- `ORDER REQUEST #... | ...`

ผลลัพธ์:
- เห็นชัดว่า reject เพราะ band แคบจริง หรือเพราะ conversion/volume math
- แยกได้ว่า stop-distance ผ่านแล้วไปติด order submit จริงหรือไม่

---

### 3) เพิ่ม execution-side validation ก่อนส่ง order

ก่อน `ExecuteMarketOrder(...)` จะมีการตรวจเพิ่มว่า:

- `Symbol.PipSize` ใช้ได้ไหม
- `Symbol.TickSize` ใช้ได้ไหม
- `stopPips` / `tpPips` เป็นค่าบวกไหม
- `pipValuePerUnit` เป็นค่าที่ใช้ sizing ได้ไหม
- `rawVolumeInUnits` เป็นค่าที่ไม่ NaN / Infinity / <= 0

ถ้าไม่ผ่าน จะเห็น:

- `GATE SIZING REJECT | reason=Sizing math invalid | ...`

ผลลัพธ์:
- ลด dead path ที่ดูเหมือน sizing ผ่านแต่จริงๆ unit conversion พัง

---

### 4) ทำ stop-limit relaxation ให้ชัดและจำกัด scope

เพิ่ม parameter ใหม่:

- `Allow Stop Relax In Probe`
- `Allow Stop Relax In Diagnostic`
- `Relaxed Stop Max Multiplier`
- `Relaxed Stop Min Multiplier`

หลักการ:
- normal mode = strict ตาม `Min Stop Distance` / `Max Stop Distance`
- probe mode = สามารถผ่อน band ได้ ถ้าเปิด `Allow Stop Relax In Probe`
- diagnostic mode = ผ่อนได้เช่นกัน แต่ default ปิดไว้ เพื่อไม่ให้ผ่อนโดยไม่ตั้งใจ

สำคัญ:
- default ของบอทยัง conservative ในโหมดปกติ
- การผ่อนถูกจำกัดไว้ที่ probe/diagnostic path เท่านั้น

---

### 5) เพิ่ม context label ให้อ่านออกทันทีว่ากำลัง strict หรือ relaxed

ใน log จะเห็นค่าเช่น:

- `context=Strict`
- `context=ProbeModeRelaxed`
- `context=DiagnosticModeRelaxed`

ผลลัพธ์:
- operator ไม่ต้องเดาแล้วว่า limit ที่ใช้เป็นค่า base หรือค่าผ่อน

---

## แนวทางรันรอบถัดไป

### ถ้าต้องการคงความ conservative
ใช้แนวนี้ก่อน:
- `Diagnostic Mode = true`
- `Probe Mode = false`
- `Allow Stop Relax In Diagnostic = false`
- `Min Stop Distance = 1.5`
- `Max Stop Distance = 20.0` หรือมากกว่านั้นถ้า broker/symbol วิ่งกว้างจริง

### ถ้าต้องการทะลุไปดู downstream execution
ใช้แนวนี้:
- `Diagnostic Mode = true`
- `Probe Mode = true`
- `Allow Stop Relax In Probe = true`
- `Relaxed Stop Max Multiplier = 1.60`
- `Relaxed Stop Min Multiplier = 0.80`

กรณี log เก่าแบบ `stopDistance=21.36 max=18.00`
ค่า multiplier 1.60 จะช่วยเปิดทาง debug ถ้า base max เดิมคือ 15 หรือ 18 แล้วแคบเกินจริง

---

## สิ่งที่ควรยืนยันจากรอบทดสอบใหม่

1. มี `STOP CHECK` ออกก่อน sizing ทุกครั้งที่ถึงจุด entry
2. ถ้า reject ต้องเห็นทั้ง price / pips / ticks / context
3. ถ้า sizing ผ่าน ต้องเห็น `ORDER REQUEST`
4. ถ้า order broker reject จริง ต้องไปเห็น `GATE ORDER_SUBMIT REJECT`
5. ใน `STOP SUMMARY` ควรแยกได้ชัดว่า reject อยู่ที่ `gate.SIZING.*` หรือ `order.submit.*`

---

## สรุป

Round 6 เน้นให้ stop-distance / sizing layer “ตรวจสอบได้จริง” มากกว่าพยายามเดาแล้วปรับ threshold แบบมืดๆ

เป้าหมายของรอบนี้คือ:
- รู้ให้ชัดว่า stop ถูกคำนวณยังไง
- รู้ว่า reject เพราะ band หรือเพราะ unit conversion
- ถ้าต้องการ probe downstream ก็มีทางผ่อนแบบจำกัดขอบเขต
- ไม่อ้าง performance หรือความสามารถทำกำไร
