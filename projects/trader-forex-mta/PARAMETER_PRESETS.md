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
