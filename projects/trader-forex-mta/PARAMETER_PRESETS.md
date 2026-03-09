# PARAMETER_PRESETS

ค่าพวกนี้เป็นจุดเริ่มต้นสำหรับการทดสอบ ไม่ใช่ค่าที่พิสูจน์แล้ว

## Preset A — Conservative
เหมาะสำหรับเริ่มต้นแบบคุมเข้ม
- breakout lookback: กลาง
- breakout buffer: กลาง
- retest zone tolerance: แคบ
- retest timeout: กลาง
- allow same-bar retest confirm: ปิด
- ATR filter: เปิด
- spread filter: เข้ม
- max trades/day: 1

## Preset B — Balanced
เหมาะสำหรับดูว่าระบบมี trade candidate มากพอไหม
- breakout lookback: กลาง
- breakout buffer: เล็กลง
- retest zone tolerance: กลาง
- retest timeout: ยาวขึ้น
- allow same-bar retest confirm: เปิด
- ATR filter: เปิด
- spread filter: กลาง
- max trades/day: 1-2

## Preset C — Exploratory
ใช้เพื่อ debug ว่าระบบติดเงื่อนไขเข้มเกินหรือไม่
- breakout lookback: สั้นลง
- breakout buffer: เล็ก
- retest zone tolerance: กว้าง
- retest timeout: ยาว
- allow same-bar retest confirm: เปิด
- ATR filter: ผ่อน
- spread filter: ผ่อน
- max trades/day: 2

## วิธีใช้ preset
1. เริ่มที่ Balanced
2. ถ้าไม่เกิด trade เลย ให้ลอง Exploratory
3. ถ้าเกิด trade มากเกินหรือมั่ว ให้ย้อนกลับ Conservative
4. ห้ามสรุปผลจาก preset เดียว


## หมายเหตุรอบ 4 เรื่อง ATR

สำหรับ XAUUSD ในรอบ debug นี้ แนะนำให้ตีความ preset เรื่อง ATR แบบนี้:
- ถ้าต้องการดู flow downstream ก่อน: `Enable ATR Filter = false`
- ถ้าต้องการเก็บค่า ATR แต่ไม่ให้ block diagnostic: เปิด `Enable ATR Filter = true` แล้วใช้ `Bypass ATR In Diagnostic = true` / `Bypass ATR In Probe = true`
- ถ้าต้องการทดสอบ ATR จริง: `Enable ATR Filter = true`, `ATR Filter Mode = RawPrice`, เริ่มที่ `Min ATR M15 = 0.0`, `Max ATR M15 = 60.0` แล้วค่อยปรับ

## หมายเหตุรอบ 6-7 เรื่อง Stop Distance / Sizing / Risk Calibration

### Conservative baseline
- `Probe Mode = false`
- `Allow Stop Relax In Diagnostic = false`
- `Min Stop Distance = 1.5`
- `Max Stop Distance = 20.0` ถึง `24.0` สำหรับ XAUUSD ที่ stop แกว่งกว้างจริง
- `Risk % = 0.25` คงไว้ก่อนเพื่อพิสูจน์ risk alignment ไม่ใช่เพื่อบังคับให้ bot trade
- `Risk Calibration Warning Mult = 1.20`

### Downstream debug preset
ใช้เมื่อ confirmation ผ่านแล้ว แต่อยากรู้ว่า risk layer / broker layer เป็นคอขวดหรือไม่:
- `Probe Mode = true`
- `Allow Stop Relax In Probe = true`
- `Relaxed Stop Max Multiplier = 1.60`
- `Relaxed Stop Min Multiplier = 0.80`
- `Diagnostic Mode = true`

### FixedLot smoke test preset
ใช้เพื่อตรวจ flow ของ manual size แบบระวัง:
- `Sizing Mode = FixedLot`
- `Fixed Size (Lots) = 0.01` เริ่มเล็กสุดก่อน
- `Risk % = 0.25` คงไว้เพื่อให้ระบบยังคำนวณ `targetRisk` และเตือน oversize ได้
- `Risk Calibration Warning Mult = 1.20`
- `Probe Mode = false` สำหรับรอบแรก ถ้าต้องการทดสอบ downstream ค่อยเปิด
- ต้องดู `SIZING CONFIG`, `FIXED LOT CHECK`, `ORDER REQUEST`, `ENTRY`

### ต้องดู log อะไร
- `STOP DISTANCE LIMITS`
- `STOP CHECK`
- `RISK CALIBRATION`
- `SIZING CONFIG`
- `SIZING`
- `FIXED LOT CHECK` (เมื่อใช้ `Sizing Mode = FixedLot`)
- `FIXED LOT RISK WARNING` (ถ้า fixed lot เสี่ยงสูงกว่าค่า target risk มาก)
- `RISK WARNING`
- `GATE SIZING PASS/REJECT`
- `ORDER REQUEST`
- `REALIZED VS PLAN`

### การตีความผล
- ถ้าเห็น `GATE SIZING REJECT | reason=Broker min volume exceeds target risk` ให้ตีความว่า account/symbol spec ยังไม่รองรับ risk target นี้อย่างปลอดภัยใน `RiskBased`
- ถ้าเห็น `GATE SIZING REJECT | reason=Fixed lot converts below broker minimum volume` หรือ `Fixed lot above symbol maximum volume` ให้แปลว่า fixed lot ที่ตั้งไว้ไม่ผ่าน broker spec
- ถ้าเห็น `ORDER REQUEST` พร้อม `expectedStopLoss` ใกล้ `targetRisk` แปลว่า risk sizing เริ่ม align ดีขึ้นแล้ว
- ถ้าใช้ `Sizing Mode = FixedLot` ให้ดู `FIXED LOT CHECK` และเทียบ `expectedStopLoss` กับ `targetRisk` ก่อนสรุปผล
- ถ้าเห็น `FIXED LOT RISK WARNING` ให้ถือว่า fixed lot ปัจจุบันใหญ่กว่ากรอบ nominal risk ที่ตั้งไว้พอสมควร แม้ระบบยังอาจส่งคำสั่งได้
- อย่าเพิ่ม `Risk %` หรือ `Fixed Size (Lots)` แบบสุ่มเพื่อหลบ reject จนกว่าจะตรวจ `SYMBOL SPEC`, `SIZING CONFIG`, และ `SIZING` log ครบ
