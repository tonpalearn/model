# RISK_CALIBRATION_NOTES.md

เอกสารนี้อธิบาย risk layer ของ Round 7 แบบ operator-focused

## ปัญหาที่ runtime จริงชี้ให้เห็น

เคสจริงก่อนรอบ 7:
- `riskAmt=0.25`
- `rawVol=1.17`
- `normVol=1.00`
- trade เปิดได้จริง
- ปิดจริง `net=-21.97`

ถ้า stop distance ประมาณ 21.36 price units และ volume 1.00 แล้วขาดทุนจริง ~21.97
นั่นแปลว่า economics ของ symbol นี้อยู่ในระดับประมาณ “1 volume unit เสียเงินจริงเกือบเท่าระยะราคา”
ไม่ใช่ระดับ 0.25 ตามที่ bot เดิมเข้าใจ

## สิ่งที่ Round 7 ใช้เป็นหลัก

Round 7 ประเมิน expected loss ที่ stop ด้วยกรอบนี้:

- `stopPips = stopDistancePrice / Symbol.PipSize`
- `estimatedLoss = stopPips * Symbol.PipValue * volumeInUnits`

จุดสำคัญ:
- ใช้ `Symbol.PipValue` ตรง
- ไม่หาร `LotSize` ซ้ำอีกชั้น

## ตัวเลขที่ควรอ่านจาก log

### 1) targetRisk
เงินที่ bot ตั้งใจเสี่ยงจาก `%Risk`

### 2) rawVol
volume ตามทฤษฎี ก่อน normalize กับ broker rules

### 3) normVol
volume หลัง `NormalizeVolumeInUnits(..., RoundingMode.Down)`

### 4) rawLoss
expected stop loss ถ้าใช้ raw volume แบบต่อเนื่องได้จริง

### 5) normLoss
expected stop loss หลัง normalize แล้ว

### 6) minVolLoss
expected stop loss ถ้าจำใจต้องใช้ broker minimum volume

### 7) minRiskMult
`minVolLoss / targetRisk`

ถ้าค่านี้สูงมาก เช่น 10x, 50x, 80x
แปลว่า account/symbol configuration นี้ **ไม่ compatible** กับ risk target ที่ตั้งไว้

## เหตุผลที่ bot ต้อง skip เมื่อ rawVol < minVol

ถ้า `rawVol < minVol`
ก็แปลโดยคณิตศาสตร์ว่า:
- volume ที่เสี่ยงตาม target จริงมีค่าน้อยกว่าขั้นต่ำที่ broker ยอมให้ส่ง
- ดังนั้นถ้าฝืนส่งขั้นต่ำ broker จะ oversize risk ทันที

Round 7 จึง reject ด้วย:
- `Broker min volume exceeds target risk`

นี่เป็น safety feature ไม่ใช่ regression

## วิธีตีความผลทดสอบรอบถัดไป

### กรณี A: เจอ `Broker min volume exceeds target risk`
แปลว่า:
- risk model เริ่มสะท้อน broker economics ได้จริงขึ้นแล้ว
- ตอนนี้ปัญหาอยู่ที่ account size / broker min volume / symbol contract spec
- ไม่ใช่ปัญหาว่า bot “ไม่กล้าเทรด” แบบไร้เหตุผล

### กรณี B: ได้ `ORDER REQUEST` แล้ว `expectedStopLoss` ใกล้ `targetRisk`
แปลว่า:
- sizing layer เริ่ม align กับ target แล้ว
- ขั้นต่อไปค่อยดู realized vs expected ตอนปิด position

### กรณี C: `REALIZED VS PLAN` เบี่ยงแรงจาก `expectedStopLoss`
ให้ดูต่อว่า:
- spread/slippage แรงผิดปกติหรือไม่
- ปิดด้วย time exit / break-even / manual close หรือไม่
- stop ที่ broker ตั้งจริงตรงกับแผนหรือไม่

## Operator guidance

สำหรับเคส account เล็ก + XAUUSD + broker min volume สูง:
- อย่าฝืนเพิ่ม `%Risk` เพื่อให้ bot ยอมเทรด ถ้ายังไม่เข้าใจ economics จริง
- ควรตรวจว่ามี symbol variant เช่น micro / cent / lower contract size หรือไม่
- หรือใช้ account equity ที่มากพอให้ minimum tradable size ไม่ oversize risk

## Runtime strings สำคัญ

- `RISK CALIBRATION | ...`
- `SIZING | ... rawLoss=... normLoss=... minVolLoss=...`
- `RISK WARNING | ...`
- `GATE SIZING REJECT | reason=Broker min volume exceeds target risk | ...`
- `ORDER REQUEST #... | ... expectedStopLoss=...`
- `REALIZED VS PLAN #... | ...`
