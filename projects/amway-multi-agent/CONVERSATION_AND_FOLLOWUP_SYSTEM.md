# CONVERSATION_AND_FOLLOWUP_SYSTEM

## เป้าหมาย
ทำให้ทุกบทสนทนามีโครงสร้างชัดเจน: รู้ว่าอีกฝ่ายสนใจอะไร, อยู่ stage ไหน, ควรตอบแบบไหน, และควร follow-up เมื่อไร

## Conversation stages
- **New** — เพิ่งเข้ามา ยังไม่รู้ need
- **Qualified** — รู้ need เบื้องต้นแล้ว
- **Product route** — สนใจแนวทาง/สินค้า
- **Business route** — สนใจโอกาสธุรกิจ
- **Nurture** — ยังไม่พร้อม ต้องติดตามแบบเบา ๆ
- **Dormant** — เงียบเกิน 14–30 วัน
- **Closed / Not fit** — ไม่เหมาะหรือจบการคุยแล้ว

## Triage flow
### Step 1: Identify intent
อ่านข้อความแล้วจัด 1 ใน 3 แบบ:
- Product
- Business
- Unclear

### Step 2: Ask 1–3 clarifying questions
#### ถ้า Product
- ตอนนี้สนใจดูแลเรื่องไหนเป็นหลักคะ
- เป้าหมายหลักคืออะไรคะ
- เคยลองวิธีไหนมาบ้างไหม

#### ถ้า Business
- สนใจหารายได้เสริมหรือกำลังมองหางานแบบยืดหยุ่นอยู่คะ
- ตอนนี้มีเวลาประมาณไหนต่อสัปดาห์
- อยากดูภาพรวมก่อน หรืออยากคุยเรื่องวิธีเริ่มคะ

#### ถ้า Unclear
- ขอบคุณที่ทักมานะคะ อยากทราบว่าตอนนี้สนใจด้านสินค้า สุขภาพ หรือโอกาสธุรกิจเป็นพิเศษไหมคะ

### Step 3: Set next step
- ส่งข้อมูลสรุป
- นัดคุย
- follow-up ภายหลัง
- ปิดเคสอย่างสุภาพ

## Follow-up cadence แนะนำ
### Product route
- Day 0: ตอบ initial + ถามคัดกรอง
- Day 1: ส่งสรุป/ตอบคำถาม
- Day 3: เช็กว่าได้อ่านไหม + มีคำถามไหม
- Day 7: follow-up เบา ๆ พร้อม asset เพิ่ม
- Day 14: ถ้ายังไม่พร้อม ย้าย nurture

### Business route
- Day 0: ตอบ initial + คัดกรอง intent
- Day 1–2: ส่งภาพรวมแบบสั้น
- Day 3–5: ถาม reflection / คำถามค้าง
- Day 7: ชวนคุย step ถัดไปถ้ายังสนใจ
- Day 14: ถ้ายังเงียบ ย้าย nurture

## Message design rules
- 1 ข้อความ = 1 จุดประสงค์
- ใช้ภาษาธรรมชาติ ไม่ copy sales script แข็ง ๆ
- ถามคำถามปลายเปิดสั้น ๆ
- อย่าทิ้งข้อความยาวโดยไม่มี context
- ถ้าอีกฝ่ายเงียบ อย่าตามถี่เกินจำเป็น

## Data fields ที่ต้องเก็บทุก lead
- วันที่เข้ามา
- ชื่อ / handle
- ช่องทาง
- interest type
- main need
- stage
- last interaction date
- next follow-up date
- summary note
- risk/compliance note

## Sample workflow 1: Lead capture
1. มีคนตอบ CTA หรือ DM เข้ามา
2. CRM Agent สรุป intent
3. บันทึกลง lead tracker
4. ส่งคำถามคัดกรองสั้น ๆ
5. ตั้ง due date follow-up

## Sample workflow 2: Conversation triage
1. รับข้อความดิบ
2. tag เป็น product/business/unclear
3. สรุป need + urgency + objection
4. สร้าง suggested reply
5. route ไป agent ถัดไปถ้าจำเป็น

## Sample workflow 3: Follow-up
1. เปิด due list วันนี้
2. เรียง Hot > Warm > Nurture
3. ส่ง follow-up ที่มีบริบท
4. อัปเดตผลลัพธ์: replied / no reply / rescheduled / closed

## Sample workflow 4: Product recommendation routing
1. เจอ lead สายสินค้า
2. ถาม goal และบริบทเพิ่ม
3. สรุป need แบบไม่วินิจฉัย
4. Route ไป Product Routing Agent เพื่อช่วยจัดกรอบการคุย
5. ให้เจ้าของธุรกิจตรวจข้อความก่อนส่งจริงถ้าเป็นเคสละเอียด

## Sample workflow 5: Business-opportunity routing
1. เจอ lead สายธุรกิจ
2. ถาม motivation + time + expectation
3. คัดคนที่มองหารายได้เร็วเกินจริงออกจาก framing เสี่ยง
4. ให้ข้อมูลภาพรวมแบบรับผิดชอบ
5. ถ้าสนใจจริง ค่อยนัดคุยลึกโดย human-led
