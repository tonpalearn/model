# CHANGELOG_ROUND2.md

อัปเดตล่าสุด: 2026-03-09

## สรุปการเปลี่ยนแปลงรอบ 2

### โค้ดหลัก
แก้ไฟล์:
- `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`

### เอกสารใหม่
เพิ่มไฟล์:
- `HARDENING_NOTES.md`
- `CHANGELOG_ROUND2.md`

---

## รายการเปลี่ยนแปลงสำคัญ

### 1) ปรับ bot label เป็นเวอร์ชันใหม่
- จาก `MTA_GOLD_BRC_v01`
- เป็น `MTA_GOLD_BRC_v02`

---

### 2) ปรับ H1 bias logic ให้ชัดขึ้น
- เพิ่ม `BiasSnapshot`
- bias log ละเอียดขึ้น
- เปลี่ยนการตรวจ swing intact ให้ดูการโดนทำลายด้วย **close หลัง swing เกิด**
- ลดปัญหา bias แบบหลวม/กำกวมของเวอร์ชันก่อน

---

### 3) แก้ daily loss cap ให้ deterministic
- เพิ่ม `_startOfDayEquity`
- รีเซ็ตตอนเปลี่ยน trading day
- ใช้ equity ต้นวันเป็นฐานสำหรับ `DailyLossCapPercent`

---

### 4) เพิ่ม trading day offset
เพิ่ม parameter:
- `Trading Day Offset Hours`

เพื่อให้ map วันเทรดตาม convention ที่ต้องการได้โดยไม่ต้องแก้โค้ด

---

### 5) คลายค่า default หลายตัวเพื่อลดภาวะ “รันได้แต่ไม่มีเทรด”
ปรับค่า default ดังนี้:
- `Break Buffer ATR` → 0.10
- `Fixed Min Break Buffer` → 0.30
- `Retest Zone ATR` → 0.30
- `Retest Timeout Bars` → 6
- `Confirm Body Min` → 0.35
- `Confirm Close Percent` → 0.45
- `Max Breakout Candle ATR` → 2.5
- `SL Buffer ATR` → 0.08
- `Fixed Min SL Buffer` → 0.20
- `Min Stop Distance` → 1.5
- `Max Stop Distance` → 20.0
- `Min ATR M15` → 1.00
- `Max ATR M15` → 20.0
- `Max Spread` → 2.50
- `Break Even Offset` → 0.10
- `New York End Hour` → 17

จุดประสงค์คือเพิ่มโอกาสเห็น setup จริงใน backtest โดยไม่ปลด guard จนหมด

---

### 6) เพิ่ม option ให้ choose behavior ของ confirmation flow
เพิ่ม parameter:
- `Allow Same-Bar Retest Confirm`

รองรับสองโหมด:
- `true` = bar เดียวกัน retest + confirm ได้
- `false` = ต้องรอ bar ถัดไปยืนยัน

ค่า default ตั้งเป็น `true`

---

### 7) เพิ่ม range memory แบบ lightweight
เพิ่ม:
- `_rangeAttempts`
- `BuildRangeKey()`
- `RegisterRangeAttempt()`
- `GetRangeAttempts()`
- parameter `Max Attempts Per Range`

ใช้เพื่อบล็อกการพยายามเข้า range เดิมซ้ำเกินกำหนด

---

### 8) เพิ่ม diagnostic logging หนักขึ้น
มี log ใหม่/ละเอียดขึ้นสำหรับ:
- symbol spec ตอนเริ่ม bot
- bias reason
- retest detection
- confirmation fail แบบ verbose
- sizing detail
- break-even fail
- time exit result
- daily reset พร้อม start-of-day equity

---

### 9) ปรับ volume sizing ให้เป็นมิตรกับ compile มากขึ้น
- ใช้ `double` ในการคำนวณ volume
- normalize volume ตอนท้าย
- ลดความเสี่ยงจาก type mismatch แบบที่ผู้ใช้เจอรอบก่อน

---

### 10) ตัด MinRewardRisk guard ออกจาก execution logic
เหตุผล:
- implementation เดิมไม่ได้วัด reward/risk ของ setup จริง
- มีไว้แล้วให้ความรู้สึกว่ามี guard แต่ในความจริงแทบไม่กรองอะไร

รอบนี้จึงถอดออกจาก flow เพื่อไม่ให้หลอกตัวเอง

---

## สิ่งที่ควรทำต่อหลังจากนี้

1. compile ใน cTrader จริงอีกครั้ง
2. backtest พร้อมเปิด log เต็ม
3. ตรวจว่าเริ่มมี `ARM / RETEST / ENTRY` หรือไม่
4. ตรวจ `SIZING` อย่างละเอียดกับ broker target
5. ถ้ายังเทรดน้อยเกินไป ค่อยปรับ parameter เพิ่มแบบทีละจุด ไม่ไล่สุ่มทั้งชุด

---

## หมายเหตุสำคัญ

เวอร์ชันนี้คือ **hardening/debuggability revision**
ไม่ใช่เวอร์ชันที่ยืนยันแล้วว่าใช้งานทำกำไรได้

เกณฑ์ความสำเร็จของรอบนี้คือ:
- compile ผ่านง่ายขึ้น
- bot ไม่เงียบโดยไม่มีคำอธิบาย
- มี log เพียงพอให้ trace ได้ว่าทำไมเข้า/ไม่เข้า
