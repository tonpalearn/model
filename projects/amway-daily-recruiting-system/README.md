# Amway Daily Recruiting System (Thai-first)

ระบบปฏิบัติการรายวันสำหรับงาน recruiting แบบ ethical + practical สำหรับผู้ทำธุรกิจ Amway แบบเดี่ยวที่ใช้ AI ช่วยคิด ช่วยจัดคิว ช่วยสรุป และช่วยเขียนข้อความ

## เป้าหมายของระบบ
- สร้าง “บทสนทนาใหม่” ทุกวันอย่างสม่ำเสมอ
- ทำให้ flow ตั้งแต่ content -> warm outreach -> follow-up -> qualification -> invite มีมาตรฐาน
- ช่วยคัดแยกคนที่ “เหมาะ / ยังไม่เหมาะ / ไม่เหมาะ” อย่างชัดเจน
- เน้นความน่าเชื่อถือ ความต่อเนื่อง และ conversion hygiene

## สิ่งที่ระบบนี้ไม่ทำ
- ไม่รับประกันรายได้ สมัครรายวัน หรือผลลัพธ์แน่นอน
- ไม่ออกแบบ spam, fake scarcity, pressure tactic หรือ deceptive recruiting
- ไม่ทำ outreach จริงแทนผู้ใช้โดยอัตโนมัติ

## โครงสร้างไฟล์
- `PROJECT_BRIEF.md` ภาพรวมโครงการ
- `AGENT_ARCHITECTURE.md` บทบาท agent ทั้งระบบ
- `DAILY_RECRUITING_OS.md` ระบบปฏิบัติการรายวัน
- `CONTENT_ATTRACTION_SYSTEM.md` ระบบ content เพื่อดึงบทสนทนาเข้า
- `OUTREACH_SYSTEM.md` ระบบ warm outreach
- `QUALIFICATION_FLOW.md` ระบบคัดกรอง
- `FOLLOWUP_SYSTEM.md` ระบบติดตามผล
- `INVITE_AND_CONVERSION_SYSTEM.md` ระบบ invite ไป call / business conversation
- `KPI_MODEL.md` ตัวชี้วัด
- `COMPLIANCE_GUARDRAILS.md` รั้วความเสี่ยง
- `WORKFLOWS.md` workflow ใช้งานจริง
- `templates/` ข้อความและสคริปต์พร้อมใช้
- `trackers/` tracker พร้อมกรอก

## วิธีเริ่มใช้ทันที
1. อ่าน `COMPLIANCE_GUARDRAILS.md` ก่อน เพื่อกำหนดขอบเขตการสื่อสาร
2. เปิด `DAILY_RECRUITING_OS.md` แล้วเลือกโหมด
   - Starter Mode = ทำคนเดียวแบบเบาแต่ต่อเนื่อง
   - Expanded Mode = เพิ่มปริมาณงานและ review
3. คัดลอกไฟล์ใน `trackers/` ไปใช้เป็น working copy ประจำวัน/สัปดาห์
4. ใช้ template ใน `templates/` ให้ AI ช่วย personalize ก่อนส่งทุกครั้ง

## Starter Mode
เหมาะสำหรับทำคนเดียว 60-90 นาที/วัน
- คอนเทนต์ 1 ชิ้น/วัน
- warm outreach 10-15 คน/วัน
- follow-up 5-10 เคส/วัน
- invite 1-3 คน/วัน
- review scoreboard 10 นาทีท้ายวัน

## Expanded Mode
เหมาะเมื่อเริ่มมี pipeline ชัดเจน 2-3 ชั่วโมง/วัน
- คอนเทนต์ 1-2 ชิ้น/วัน + story/short update
- warm outreach 20-30 คน/วัน
- follow-up 15-25 เคส/วัน
- invite 3-5 คน/วัน
- weekly review เชิงสถิติ + ปรับข้อความ + ปรับ ICP

## หลักการใช้งาน
- ใช้ AI เพื่อ “เตรียมงาน” ไม่ใช่ “แกล้งเป็นคน”
- ทุกข้อความต้องสอดคล้องกับความจริง ประสบการณ์จริง และข้อกำกับของบริษัท/กฎหมายในพื้นที่
- ถ้าคนไม่สนใจ ให้จบอย่างสุภาพ และลบออกจาก active follow-up

## Ready-to-use assets
### Templates
- soft inbound CTA posts
- warm opener messages
- follow-up messages by stage
- qualification questions
- invite-to-call scripts
- invite-to-business-conversation scripts
- daily scoreboard template

### Trackers
- recruiting lead tracker
- daily recruiting scoreboard
- weekly pipeline review
- outreach queue
- follow-up queue

## ข้อแนะนำสำคัญ
ระบบนี้จะทำงานดีที่สุดเมื่อใช้กับ 3 วินัยพร้อมกัน:
1. ลงบันทึกทุก interaction
2. follow-up ตาม stage ไม่ส่งมั่ว
3. review ตัวเลขทุกสัปดาห์แล้วปรับข้อความตามข้อมูลจริง
