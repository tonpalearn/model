# AGENT_ARCHITECTURE.md

ระบบนี้ออกแบบเป็น multi-agent operating system สำหรับ solo operator โดย AI แต่ละ agent มีหน้าที่ชัด ไม่ก้าวก่ายกัน

## 1) Recruiting Ops Manager Agent
### หน้าที่
- จัดลำดับงานประจำวัน
- สรุป backlog จาก trackers
- แจกงานให้ agent อื่น
- เช็กว่าแต่ละ stage มีคอขวดตรงไหน

### Input
- daily scoreboard
- outreach queue
- follow-up queue
- lead tracker

### Output
- daily priorities 3-5 ข้อ
- รายชื่อที่ต้อง follow-up วันนี้
- รายชื่อที่ควร invite วันนี้
- end-of-day summary

### Prompt role summary
"คุณคือผู้จัดการงาน recruiting รายวัน ทำหน้าที่จัดลำดับ content, outreach, follow-up, qualification, invite โดยยึด compliance และเป้าหมายสร้างบทสนทนาใหม่อย่างสม่ำเสมอ"

## 2) Content Attraction Agent
### หน้าที่
- สร้าง idea และ draft content แบบ soft CTA
- ดึงจากเรื่องจริง ประสบการณ์จริง บทเรียนจริง
- วาง content mix: lifestyle, learning, routine, transformation without hype

### Output
- post draft
- story prompt
- CTA options
- comment reply suggestions

### กฎ
- ห้าม clickbait เกินจริง
- ห้ามใช้รายได้หรือ lifestyle claim แบบชวนฝันโดยไม่มีบริบท

## 3) Outreach Copy Agent
### หน้าที่
- เขียน warm opener และ context-based follow-up
- personalize จากความสัมพันธ์เดิม / content ที่เขาโพสต์ / เรื่องที่เคยคุย
- แยก opener ตามระดับความใกล้ชิด

### Output
- opener 3-5 เวอร์ชัน
- short follow-up options
- voice note outline (ถ้าจะอัดเอง)

## 4) Conversation Qualification Agent
### หน้าที่
- ช่วยประเมินว่า prospect เหมาะ, ยังไม่เหมาะ, หรือไม่เหมาะ
- สร้างคำถามคัดกรองแบบสุภาพ
- สรุป pain, goal, readiness, objections

### Output
- stage recommendation
- qualification summary
- next best question
- fit / not-fit recommendation

## 5) Follow-up Agent
### หน้าที่
- สร้าง follow-up ตาม stage และ timing
- ป้องกันการตามจี้เกินจำเป็น
- หยุดลำดับ follow-up เมื่อ prospect ไม่ตอบหรือปฏิเสธชัดเจน

### Output
- message by stage
- re-engagement draft
- close-the-loop draft

## 6) Invite-to-Call / Invite-to-Join Agent
### หน้าที่
- เลือก timing สำหรับ invite
- เขียน invite ไป call หรือ business conversation
- เตรียม pre-frame ว่าจะคุยเรื่องอะไร ไม่ใช่หลอกให้มา

### Output
- invite message
- confirmation message
- reminder message
- post-call next-step summary

## 7) Performance Review Agent
### หน้าที่
- วิเคราะห์ KPI รายวัน/รายสัปดาห์
- ชี้ bottleneck
- แนะนำว่าควรปรับ content, opener, qualification หรือ invite

### Output
- weekly insight report
- top 3 things to fix
- top 3 things to keep doing
- experiment ideas สำหรับสัปดาห์ถัดไป

## Handoff logic
1. Recruiting Ops Manager เปิดวัน -> ดึง backlog
2. Content Attraction Agent เตรียม content + CTA
3. Outreach Copy Agent เตรียม opener
4. Conversation Qualification Agent ช่วยระหว่าง chat
5. Follow-up Agent จัดคิวตาม stage
6. Invite Agent ทำ invite เมื่อ fit + timing พร้อม
7. Performance Review Agent ปิดสัปดาห์และแนะนำการปรับ

## Data schema ที่ทุก agent ใช้ร่วมกัน
- Lead name
- Source
- Relationship warmth (hot / warm / lukewarm)
- Current stage
- Last contact date
- Next action
- Fit score (Low/Medium/High)
- Notes / signals
- Compliance flags

## Suggested stage model
1. New lead
2. Warm contacted
3. Replied
4. Qualified-light
5. Qualified-strong
6. Invited
7. Call booked / conversation booked
8. Decision pending
9. Not now
10. No fit
11. Won (joined / next formal step)
