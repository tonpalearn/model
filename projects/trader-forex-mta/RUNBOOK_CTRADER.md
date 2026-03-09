# RUNBOOK_CTRADER

เอกสารนี้ใช้สำหรับลองรัน bot ใน cTrader แบบเป็นระบบ

## เป้าหมาย
- compile ให้ผ่าน
- attach bot ลง chart ได้
- ตรวจว่ามี signal/trade candidate หรือไม่
- เก็บ log ให้พอสำหรับ debug รอบถัดไป

## Step 1: Compile
- เปิดไฟล์ `src/MtaGoldBreakoutRetestBot/MtaGoldBreakoutRetestBot.cs`
- compile ใน cTrader Automate
- ถ้ามี error ให้จด:
  - บรรทัด
  - ข้อความ error
  - code snippet ที่เกี่ยวข้อง

## Step 2: Attach ลง chart
- Symbol: `XAUUSD`
- Timeframe chart ที่แนะนำสำหรับ attach: `M15`
- ตรวจว่าบอทรันโดยไม่มี runtime error ทันที

## Step 3: ตรวจ input parameters
ก่อน run ให้จดค่าที่ใช้จริง เช่น:
- session start/end
- breakout lookback
- retest timeout
- ATR filter
- spread filter
- allow same-bar retest confirm
- trading day offset hours

## Step 4: เปิด log
ระหว่างรันให้สังเกตข้อความ log เช่น:
- BIAS
- ARMED
- RETEST
- INVALIDATE
- STOP DISTANCE LIMITS
- STOP CHECK
- GATE SIZING PASS / REJECT
- SIZING
- ORDER REQUEST
- ENTRY
- BE
- TIME EXIT

## Step 5: ถ้าไม่เข้าออเดอร์
ให้ตรวจตามลำดับ:
1. bot รันอยู่จริงไหม
2. symbol ถูกไหม
3. timeframe ถูกไหม
4. session filter บล็อกหรือไม่
5. spread/ATR filter ตัดทิ้งหรือไม่
6. bias เป็น neutral ตลอดหรือไม่
7. breakout ถูก arm แต่ invalidate ก่อนหรือไม่
8. retest timeout สั้นเกินหรือไม่
9. `STOP CHECK` reject เพราะ band strict แคบเกินจริงหรือไม่
10. `SIZING` log แสดง `pipValuePerUnit`, `rawVol`, `normVol` สมเหตุผลหรือไม่
11. ถ้า `ORDER REQUEST` โผล่แล้ว แต่ไม่เข้า order ให้ดู `GATE ORDER_SUBMIT REJECT`

## Step 6: สิ่งที่ควรส่งกลับมาให้วิเคราะห์
- screenshot หน้า parameters
- compile errors (ถ้ามี)
- log ช่วงที่น่าจะมี setup
- screenshot chart ตอน bot ไม่เข้าออเดอร์
- broker / server / symbol spec ถ้าพอเห็น

## เป้าหมายรอบแรก
ยังไม่ใช่ทำกำไร
แต่คือยืนยันว่า bot:
- compile ได้
- รันได้
- มีเหตุผลใน log
- มี candidate trades หรืออย่างน้อยรู้ว่าติดคอขวดตรงไหน
