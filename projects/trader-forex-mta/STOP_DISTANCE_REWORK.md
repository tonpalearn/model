# STOP_DISTANCE_REWORK.md

เอกสารนี้อธิบายการตีความ stop-distance ในรอบ 6 สำหรับ `XAUUSD` บน cTrader/cAlgo

## ปัญหาที่รอบนี้แก้

ก่อนหน้า flow ไปได้ถึง:
- confirmation pass
- order precheck pass

แต่ reject ที่ sizing ด้วยอาการประมาณนี้:

- `GATE SIZING REJECT | reason=Stop distance invalid | stopDistance=21.36 min=1.60 max=18.00`

ปัญหาหลักไม่ใช่แค่ว่า "stop กว้างเกิน" แต่คือ operator ยังเห็นรายละเอียดไม่พอว่า:
- stop ถูกวัดเป็นหน่วยอะไร
- ค่านี้เทียบกับ pip/tick บน broker นั้นเท่าไร
- band ที่ใช้เป็น strict หรือ relaxed
- sizing math หลังจากแปลงหน่วยแล้วสมเหตุผลหรือไม่

---

## สิ่งที่เปลี่ยนในโค้ด

ตอนนี้ bot จะ log stop-distance เป็น 3 มุมพร้อมกัน:

- `distPrice` = ระยะ stop ในหน่วยราคา
- `distPips` = ระยะ stop หลังหารด้วย `Symbol.PipSize`
- `distTicks` = ระยะ stop หลังหารด้วย `Symbol.TickSize`

และจะ log ทั้ง:
- `bandBasePrice`
- `bandEffPrice`
- `bandBasePips`
- `bandEffPips`

จึงทำให้เห็นทันทีว่า:
- ค่าตั้งต้นของ user คืออะไร
- ค่าที่ใช้จริงตอนนั้นถูกผ่อนหรือไม่
- ถ้ากว้างเกิน มันกว้างเกินในเชิง price เท่าไร และตีเป็น pips/ticks เท่าไร

---

## Log ที่ต้องมองหา

### 1) ตอน bot start
- `STOP DISTANCE LIMITS | ...`

ตัวนี้บอก:
- base min/max
- effective min/max
- relax context
- flag ว่า probe/diagnostic relaxation เปิดหรือไม่

### 2) ตอน entry กำลังจะถูกคำนวณ
- `STOP CHECK #... | ...`

ตัวนี้คือ log สำคัญสุดของรอบนี้

### 3) ตอน sizing
- `SIZING | ...`

ตัวนี้บอกต่อว่า stop-distance ที่ผ่านแล้ว ถูกแปลงไปเป็น:
- stop pips
- stop ticks
- tp pips
- pipValuePerUnit
- rawVol
- normVol

### 4) ก่อนส่งคำสั่งจริง
- `ORDER REQUEST #... | ...`

ถ้าเห็นบรรทัดนี้ แปลว่า stop-distance และ sizing math ผ่านแล้ว

---

## พารามิเตอร์ใหม่

### `Allow Stop Relax In Probe`
- default: `true`
- มีผลเมื่อ `Probe Mode = true`
- ใช้เพื่อเปิดทาง debug downstream execution โดยไม่ต้องไปเปลี่ยน base limit ทุกครั้ง

### `Allow Stop Relax In Diagnostic`
- default: `false`
- มีผลเมื่อ `Diagnostic Mode = true`
- ตั้งใจให้ operator เปิดเองเมื่ออยาก probe behavior แต่ยังไม่อยากเปิด `Probe Mode`

### `Relaxed Stop Max Multiplier`
- default: `1.60`
- ใช้ขยาย `Max Stop Distance` ใน relaxed context

### `Relaxed Stop Min Multiplier`
- default: `0.80`
- ใช้ลด `Min Stop Distance` ใน relaxed context

---

## วิธีคิดเรื่อง conservative vs debug

### โหมดปกติ / conservative
ใช้:
- `Probe Mode = false`
- `Allow Stop Relax In Diagnostic = false`

ผลคือ bot ใช้ค่า `Min Stop Distance` / `Max Stop Distance` ตรงๆ

### โหมด probe downstream
ใช้:
- `Probe Mode = true`
- `Allow Stop Relax In Probe = true`

ผลคือ bot ยัง log ทุกอย่างเหมือนเดิม แต่ band stop จะกว้างขึ้นตาม multiplier

สำคัญ:
- การผ่อนนี้มีไว้เพื่อดู flow downstream
- ไม่ควรเอาไปรวมกับข้อสรุป performance แบบ production

---

## Suggested operator workflow

### กรณีต้องการยืนยันว่า stop-distance ยังเป็นคอขวดไหม
1. รันด้วยค่าที่ใช้ล่าสุดก่อน
2. ดู `STOP CHECK`
3. ถ้า `context=Strict` และ reject เพราะเกิน max เล็กน้อย ให้ตัดสินใจว่าจะ:
   - เพิ่ม `Max Stop Distance` แบบ explicit
   - หรือเปิด probe relaxation เพื่อ debug downstream ก่อน

### กรณีต้องการทะลุไปดู broker execution
1. เปิด `Probe Mode = true`
2. ใช้ `Allow Stop Relax In Probe = true`
3. ดูว่า flow เปลี่ยนจาก
   - `GATE SIZING REJECT`
   ไปเป็น
   - `ORDER REQUEST`
   หรือ
   - `GATE ORDER_SUBMIT REJECT`

ตรงนี้จะช่วยตอบได้ว่า bottleneck ถัดไปอยู่ที่ไหน

---

## ตัวอย่างการตีความ

ถ้าเห็น:
- `distPrice=21.36`
- `bandBasePrice=[1.50,15.00]`
- `bandEffPrice=[1.20,24.00]`
- `context=ProbeModeRelaxed`
- `valid=True`

แปลว่า:
- ใน strict mode จะไม่ผ่าน
- แต่ใน probe-relaxed mode ผ่านแล้ว
- จึงสามารถไปทดสอบ order submit ต่อได้

ถ้าเห็น:
- `SIZING | ... pipValuePerUnit=0 ...`

แปลว่า bottleneck ไม่ใช่ stop-distance แล้ว แต่เป็น symbol spec / pip value conversion

---

## สิ่งที่ไม่ควรสรุปเกินจริง

- การผ่าน stop-distance gate ไม่ได้แปลว่า setup ดี
- การผ่อน limit เพื่อ debug ไม่ได้แปลว่าเหมาะสำหรับ live usage
- การเข้า order ได้ ไม่ได้แปลว่า strategy นี้ clone พฤติกรรม MTA ได้ตรง

เป้าหมายของรอบนี้คือ “เห็นความจริงของ execution path” ให้ชัดที่สุด
