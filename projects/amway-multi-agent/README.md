# Amway Multi-Agent Operating System

ระบบปฏิบัติการธุรกิจแบบหลายเอเจนต์สำหรับผู้ทำธุรกิจ Amway แบบ solo operator + AI assistance

> เวอร์ชันนี้ออกแบบให้ “ใช้งานจริงได้ทันที” โดยโฟกัสที่กิจกรรมรายวัน, การติดตาม lead, การทำคอนเทนต์, การคัดกรองบทสนทนา, และการทบทวนผลแบบมีวินัย
>
> **สำคัญ:** เอกสารนี้ไม่รับประกันรายได้, โบนัส, ระดับ 12%, หรือยอดขาย 100,000 บาท/เดือน ผลลัพธ์ขึ้นกับทักษะ, ความสม่ำเสมอ, ตลาด, ความเหมาะสมของสินค้า, คุณภาพความสัมพันธ์, และการปฏิบัติตามกฎของบริษัท/กฎหมาย

## ใช้ระบบนี้เพื่ออะไร
- เปลี่ยนเป้าหมายกว้าง ๆ ให้เป็นระบบงานรายวัน/รายสัปดาห์
- ให้ AI ช่วยเป็น “ทีมงานหลังบ้าน” โดยไม่หลุด compliance
- ลดความสะเปะสะปะของการโพสต์, แชต, follow-up, และการวัดผล
- ช่วยให้ผู้ทำคนเดียวทำงานแบบมี pipeline ชัดเจน

## โครงสร้างไฟล์
- `PROJECT_BRIEF.md` — ภาพรวมโครงการและหลักคิด
- `BUSINESS_GOAL_MODEL.md` — แปลงเป้าหมาย 12% / 100k เป็น activity + KPI model แบบไม่การันตี
- `AGENT_ARCHITECTURE.md` — สถาปัตยกรรมเอเจนต์, บทบาท, handoff
- `DAILY_OPERATING_SYSTEM.md` — ตารางทำงานรายวัน
- `WEEKLY_REVIEW.md` — ระบบรีวิวรายสัปดาห์
- `CONTENT_ENGINE.md` — เครื่องยนต์คอนเทนต์แบบ Thai-first
- `CONVERSATION_AND_FOLLOWUP_SYSTEM.md` — ระบบคุย, คัดกรอง, follow-up
- `COMPLIANCE_GUARDRAILS.md` — ข้อห้าม/ข้อควรระวังสำคัญ
- `sops/` — SOP ใช้งานจริง
- `templates/` — tracker และ template กรอกใช้ได้เลย
- `prompts/` — prompt พร้อมใช้สำหรับแต่ละเอเจนต์

## คำแนะนำเริ่มต้น
### เริ่มแบบแนะนำ: 3 agents
สำหรับผู้ทำคนเดียว ให้เริ่มจาก 3 ตัวก่อน:
1. **Ops Manager** — สรุปงาน, จัดลำดับความสำคัญ, คุม KPI
2. **Content Agent** — วิจัย + เขียนคอนเทนต์ + CTA
3. **CRM & Follow-up Agent** — คัดกรองบทสนทนา, บันทึก lead, เตือน follow-up

เหตุผล: ชุดนี้ครอบคลุมกิจกรรมที่สำคัญที่สุดก่อน คือ **ดึงคนเข้า, คุยต่อ, และไม่ปล่อย lead หลุด**

### เวอร์ชันขยาย: 6 agents
เมื่อเริ่มมี lead เข้าและงานเริ่มแน่น ค่อยเพิ่ม:
4. **Product Routing Agent**
5. **Business Opportunity Agent**
6. **Performance Analyst & Compliance Checker**

## สิ่งที่พร้อมใช้ทันที
- prompt agent พร้อมคัดลอกไปใช้
- content calendar / lead tracker / KPI tracker
- daily routine และ weekly review
- workflow สำหรับ content > lead > chat > follow-up > routing > review

## วิธีเริ่มใน 30 นาที
1. เปิด `PROJECT_BRIEF.md` และ `BUSINESS_GOAL_MODEL.md`
2. เปิด `templates/lead_tracker.csv` กับ `templates/content_calendar.csv`
3. ใช้ prompt ใน `prompts/STARTER_*.md` เพื่อเริ่มทำงานวันนี้ทันที

## หมายเหตุด้าน compliance
- ห้ามการันตีรายได้หรือผลลัพธ์
- หลีกเลี่ยง health claims เกินจริง
- แยก “ประสบการณ์ส่วนตัว” ออกจาก “ข้อเท็จจริงของสินค้า/ข้อมูลสุขภาพ” ให้ชัด
- ถ้าเคสเสี่ยงทางสุขภาพ ให้แนะนำพบแพทย์ ไม่วินิจฉัยเอง
