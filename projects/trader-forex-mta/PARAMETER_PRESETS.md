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

## หมายเหตุรอบ 6 เรื่อง Stop Distance / Sizing

### Conservative baseline
- `Probe Mode = false`
- `Allow Stop Relax In Diagnostic = false`
- `Min Stop Distance = 1.5`
- `Max Stop Distance = 20.0` ถึง `24.0` สำหรับ XAUUSD ที่ stop แกว่งกว้างจริง

### Downstream debug preset
ใช้เมื่อ confirmation ผ่านแล้ว แต่อยากรู้ว่าติด broker execution ต่อหรือไม่:
- `Probe Mode = true`
- `Allow Stop Relax In Probe = true`
- `Relaxed Stop Max Multiplier = 1.60`
- `Relaxed Stop Min Multiplier = 0.80`

### ต้องดู log อะไร
- `STOP DISTANCE LIMITS`
- `STOP CHECK`
- `GATE SIZING PASS/REJECT`
- `SIZING`
- `ORDER REQUEST`

ถ้าเห็น `ORDER REQUEST` แปลว่า stop-distance และ sizing math ผ่านแล้ว และ bottleneck ถัดไปอยู่ที่ broker/order submit layer แทน
