# AGENT_ARCHITECTURE

## ภาพรวม
ระบบนี้มี 2 โหมด:
1. **Starter mode (3 agents)** — เหมาะที่สุดสำหรับเริ่มใช้งานจริง
2. **Expanded mode (6 agents)** — เพิ่มความละเอียดเมื่อมี volume มากขึ้น

---

## Starter Mode (แนะนำให้เริ่มแบบนี้)

### Agent 1: Ops Manager
**หน้าที่**
- รับเป้าหมายรายสัปดาห์
- จัด priority วันนี้
- ดึงงานจาก content, CRM, KPI มารวมเป็น daily action list
- สรุปปลายวันว่าอะไรเสร็จ อะไรค้าง

**Input**
- weekly goals
- content backlog
- lead tracker
- follow-up due list

**Output**
- daily plan
- end-of-day summary
- tomorrow priority list

### Agent 2: Content Agent
**หน้าที่**
- หา angle จาก pain point ของกลุ่มเป้าหมาย
- สร้างคอนเทนต์ 1 ชิ้นหลัก + 2–3 ชิ้นย่อย
- ใส่ CTA ที่เหมาะสม
- เช็กถ้อยคำให้ไม่เสี่ยง compliance

**Input**
- target audience
- current offers / product focus
- FAQ จากบทสนทนาจริง
- content performance สัปดาห์ก่อน

**Output**
- post draft
- short video script
- story / line message
- CTA options

### Agent 3: CRM & Follow-up Agent
**หน้าที่**
- รับข้อความ lead ใหม่
- สรุป stage และ intent
- แนะนำข้อความตอบกลับและ next step
- อัปเดต due date สำหรับ follow-up

**Input**
- inbound message
- lead history
- current stage
- route type (product / business / unclear)

**Output**
- conversation summary
- suggested reply
- lead stage
- follow-up schedule

### Handoff ใน Starter Mode
1. Ops Manager กำหนดเป้าหมายวันนี้
2. Content Agent ผลิตคอนเทนต์และ CTA
3. เมื่อมีคนตอบกลับ CRM Agent คัดกรอง/ติดตาม
4. ปลายวัน Ops Manager รวบยอดทุกอย่างกลับเข้า scorecard

---

## Expanded Mode (6 agents)

### Agent 1: Ops Manager
ควบคุมภาพรวมทั้งระบบ

### Agent 2: Audience & Insight Researcher
หา pain points, objections, FAQ, คำพูดจริงของลูกค้า

### Agent 3: Content & Campaign Builder
แปลง insight เป็น content pack หลาย format

### Agent 4: Conversation Triage & CRM Agent
คัดกรองบทสนทนา, tagging, stage, follow-up

### Agent 5: Product Recommendation Routing Agent
แยกตาม need เช่น น้ำหนัก, พลังงาน, การพักผ่อน, สุขภาพทั่วไป โดยไม่ overclaim

### Agent 6: Business Opportunity Routing Agent
ดูว่าคนสนใจรายได้เสริม/ธุรกิจจริงไหม, พร้อมคุยระดับไหน, ควรส่งข้อมูลอะไรต่อ

### Agent 7 (optional หาก workload มาก): Performance Analyst & Compliance Checker
- รีวิวตัวเลข
- หาจุดรั่วของ funnel
- ตรวจคำพูดเสี่ยง claim

> ในการใช้งานจริง เวอร์ชันขยายนี้สามารถรวม Agent 7 เข้า Agent 1 ได้ หากยังไม่อยากแยกบทบาท

---

## Recommended setup สำหรับ solo operator
### Phase 1: เริ่ม 3 agents ก่อน 2–4 สัปดาห์
เพราะช่วงแรกคอขวดจริงมักอยู่ที่:
- ไม่มีคอนเทนต์สม่ำเสมอ
- lead หลุด follow-up
- ไม่มีการทบทวนตัวเลข

### Phase 2: เพิ่ม routing agents
เมื่อเริ่มมี inbound เข้ามาสม่ำเสมอค่อยแยก:
- product routing
- business-opportunity routing

### Phase 3: เพิ่ม analyst/compliance review
เมื่อเริ่มมี content volume และ lead volume มากพอ

## Routing logic หลัก
### Route A: Product interest
สัญญาณ:
- ถามเรื่องอาการ/ปัญหา/เป้าหมายด้านสุขภาพ
- ถามว่าสินค้าอะไรเหมาะ
- ถามราคา/วิธีใช้

### Route B: Business opportunity
สัญญาณ:
- ถามเรื่องรายได้เสริม
- ถามว่าทำยังไง
- ถามเรื่องทีม, เวลา, การเริ่มต้น

### Route C: Nurture / unclear
สัญญาณ:
- ยังไม่ชัดว่าต้องการอะไร
- แค่กดไลก์/ทักมาสั้น ๆ
- สนใจแต่ยังไม่พร้อม

## Human override points
เรื่องต่อไปนี้ควรให้เจ้าของธุรกิจตัดสินใจเอง:
- เคสสุขภาพซับซ้อนหรือเสี่ยง
- คำถามเชิงการแพทย์หรือการรักษา
- การพูดเรื่องรายได้/ผลตอบแทนที่ละเอียดอ่อน
- การปิดการขายสำคัญ
- การนำเสนอแผนธุรกิจลึก ๆ
