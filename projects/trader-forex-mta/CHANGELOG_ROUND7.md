# CHANGELOG_ROUND7.md

สถานะ: Round 7 Risk Calibration / Realized PnL Alignment

## สรุปสั้น

รอบนี้ไม่ได้แตะ strategy alpha เป็นหลัก แต่แก้จุดที่ runtime จริงพิสูจน์แล้วว่า sizing กับ realized PnL ไม่ตรงกันบน XAUUSD/cTrader

runtime ที่ยืนยันปัญหา:
- `SIZING | ... riskAmt=0.25 ... rawVol=1.17 normVol=1.00`
- เปิด order ได้จริง
- ปิดแล้ว `net=-21.97`
- จากนั้นโดน `Daily loss cap` ทั้งที่ target risk เดิมแค่ `0.25`

ความหมายคือ order pipeline ใช้งานได้แล้ว แต่ชั้น risk economics ยังตีความ symbol/broker ไม่ตรงกับของจริง

---

## สิ่งที่แก้หลัก

### 1) เปลี่ยน risk calibration basis ให้ยึด broker symbol economics ตรงกว่าเดิม

แกนสำคัญคือเลิกหาร `Symbol.PipValue` ด้วย `LotSize` อีกชั้น

รอบก่อน logic เดิมทำให้ risk ต่อ 1 volume unit ของ XAUUSD ถูกประเมินต่ำกว่าจริงมาก จนบอทคิดว่า volume 1 ยังเสี่ยงแค่หลักเศษ แต่ผลจริงขาดทุนระดับ ~22 currency units เมื่อโดน stop จริง

รอบนี้จึงใช้:
- `Symbol.PipValue` ตรงเป็นฐานสำหรับคำนวณ stop-loss estimate
- คำนวณ expected stop loss จาก
  - `stopPips`
  - `pipValuePerUnit`
  - `volumeInUnits`

เป้าหมายคือให้ `target risk -> estimated stop loss -> realized PnL` อยู่ในโลกตัวเลขเดียวกันมากขึ้น

---

### 2) เพิ่ม observability แบบ explicit สำหรับ risk layer

มี log ใหม่/ขยายเพิ่มที่ควรมองหา:

- `RISK CALIBRATION | ...`
- `SIZING | ... rawLoss=... normLoss=... minVolLoss=... normRiskMult=... minRiskMult=...`
- `RISK WARNING | ...`
- `ORDER REQUEST #... | ... targetRisk=... expectedStopLoss=... minVolLoss=...`
- `ENTRY Buy/Sell #... | ... targetRisk=... expectedStopLoss=...`
- `REALIZED VS PLAN #... | ... realizedVsExpected=... realizedVsTarget=...`

ผลคือ operator จะเห็นพร้อมกันว่า:
- ตั้งใจเสี่ยงเท่าไร (`targetRisk`)
- volume ดิบที่สูตรอยากได้ (`rawVol`)
- volume หลัง normalize (`normVol`)
- ถ้าโดน broker min volume จะเสี่ยงจริงเท่าไร (`minVolLoss`)
- ถ้าถือจน stop ตามแผน จะคาดว่าจะเสียเท่าไร (`expectedStopLoss`)
- ตอนปิดจริง PnL เบี่ยงจากแผนแค่ไหน (`REALIZED VS PLAN`)

---

### 3) เพิ่ม guard แบบ conservative เมื่อ broker min volume ใหญ่เกิน risk target

ถ้า sizing คำนวณได้ volume ต่ำกว่า `Symbol.VolumeInUnitsMin`
รอบนี้จะ reject ชัดด้วย:

- `GATE SIZING REJECT | reason=Broker min volume exceeds target risk | ...`

รายละเอียดจะบอกครบ เช่น:
- `targetRisk`
- `rawVol`
- `minVol`
- `rawLoss`
- `minVolLoss`
- `minRiskMult`

ความหมายเชิงปฏิบัติ:
- ถ้า broker บังคับขั้นต่ำ 1.00 แต่ 1.00 unit บน XAUUSD เสี่ยงจริงสูงกว่าที่ตั้งใจหลายเท่า
- บอทจะ **ไม่ฝืนส่ง order**

นี่คือ safeguard หลักของรอบ 7

---

### 4) เก็บ normalized-volume distortion ไว้ใน log แบบอ่านออก

ใน `SIZING` จะเห็นทั้ง:
- `rawVol`
- `normVol`
- `rawLoss`
- `normLoss`
- `minVolLoss`
- `normRiskMult`
- `minRiskMult`
- `rawBelowMin`

ดังนั้นจะเห็นทันทีว่า:
- rounding down ทำให้ under-risk หรือไม่
- broker min volume ทำให้ order เป็นไปไม่ได้ภายใต้ risk target หรือไม่
- warning threshold ถูกชนหรือไม่

---

### 5) เพิ่ม realized-vs-plan comparison หลังปิด position

เมื่อ position ปิด จะมี log เพิ่ม:
- `REALIZED VS PLAN #... | ...`

เพื่อเทียบ:
- `targetRisk`
- `expectedStopLoss`
- `rawPlannedLoss`
- `minVolLoss`
- `net`
- `realizedVsExpected`
- `realizedVsTarget`

นี่ช่วยแยกว่า:
- sizing model ยังผิดอยู่
- หรือ sizing ถูกแล้ว แต่ slippage/spread/exit path ทำให้ realized ต่างจาก stop estimate

---

## ไฟล์ที่แก้

### โค้ด
- `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`

### เอกสาร
- `CHANGELOG_ROUND7.md`
- `RISK_CALIBRATION_NOTES.md`
- `PARAMETER_PRESETS.md`
- `RUNBOOK_CTRADER.md`
- `SETUP.md`

---

## Runtime strings ที่ควรจับตาในการเทสต์รอบถัดไป

### ตอนเริ่มรัน
- `SYMBOL SPEC | pipSize=... pipValue=... tickValue=...`
- `RISK CALIBRATION | riskPct=... pipValueMode=DirectSymbolPipValue ...`

### ตอนถึงจุด sizing
- `STOP CHECK #...`
- `SIZING | ... rawLoss=... normLoss=... minVolLoss=...`
- `RISK WARNING | ...` (ถ้ามี)

### ถ้า broker minimum บังคับให้ oversize
- `GATE SIZING REJECT | reason=Broker min volume exceeds target risk | ...`

### ถ้า sizing ผ่านและส่ง order
- `ORDER REQUEST #... | ... targetRisk=... expectedStopLoss=...`
- `ENTRY Buy/Sell #... | ... targetRisk=... expectedStopLoss=...`

### หลังปิด position
- `REALIZED VS PLAN #... | ... realizedVsExpected=... realizedVsTarget=...`

---

## แนวทางเทสต์ที่แนะนำ

1. ยังใช้ `Diagnostic Mode = true`
2. ถ้าต้องการทะลุ confirm ให้ใช้ค่า confirm/probe ที่เคยผ่านแล้ว
3. แต่รอบนี้ให้โฟกัสว่า **ถึงแม้ order path ผ่าน บอทจะยอม skip เองหรือไม่เมื่อ broker min volume ทำให้ risk เกินจริงมาก**
4. ถ้าเห็น `Broker min volume exceeds target risk` ให้ถือว่านี่คือผลลัพธ์ที่ถูกต้องเชิง safety ไม่ใช่ bug ใหม่
5. ส่งกลับมาทั้งชุด log ที่มี:
   - `SYMBOL SPEC`
   - `RISK CALIBRATION`
   - `STOP CHECK`
   - `SIZING`
   - `ORDER REQUEST` หรือ `GATE SIZING REJECT`
   - `REALIZED VS PLAN` (ถ้ามี trade ปิด)

---

## ข้อจำกัดที่ยังตั้งใจคงไว้

- ยังไม่อ้าง profitability
- ยังไม่อ้างว่า clone พฤติกรรม MTA ได้ตรงเป๊ะ
- ยังไม่ได้แก้ strategy alpha
- รอบนี้เน้นให้ sizing realism, observability, และ safety ชัดขึ้นก่อน
