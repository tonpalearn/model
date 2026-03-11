# AGENT ARCHITECTURE

## ภาพรวม
ระบบนี้ออกแบบเป็น multi-agent content engine ที่คุยกันผ่าน **structured handoff** แทนการโยนงานมั่วๆ

หลักการ:
- แต่ละ agent มีหน้าที่ชัด
- ส่งต่องานด้วย brief สั้น กระชับ ตรวจสอบได้
- มี compliance gate ก่อน publish
- มี performance loop ป้อนกลับเข้าระบบ

## Starter Version (Lean Team)
เหมาะกับ 1 คน + AI หลายบทบาท

### 1) Ops/Editor Agent
หน้าที่:
- คุม calendar
- เลือกหัวข้อประจำวัน
- ตัดสินใจ final publish queue
- เช็กว่า format mix สมดุล

Input:
- trend ideas
- performance notes
- content queue

Output:
- daily brief 2 ชิ้น
- final approval notes

### 2) Trend/Research Agent
หน้าที่:
- หา topic angles, trend, recurring pain points
- แตกประเด็นเป็น content opportunities
- flag เรื่องที่เสี่ยง claim

Output:
- top 5 ideas วันนี้
- why now
- target persona
- suggested hook angle

### 3) Script/Caption Agent
หน้าที่:
- เขียน hook, body, CTA, caption, on-screen text
- แยก tone ตาม persona และ format

Output:
- สคริปต์พร้อมอ่าน
- caption
- first comment

### 4) Video Concept Agent
หน้าที่:
- เปลี่ยนหัวข้อให้เป็น shot-by-shot structure
- เลือก format: talking-head / B-roll / POV / listicle

Output:
- shot list
- visual cue
- timing 15-45 วินาที

### 5) Static/Image Post Agent
หน้าที่:
- เปลี่ยนหัวข้อเป็น carousel/slideshow/photo-text
- เขียนข้อความแต่ละสไลด์

Output:
- slide 1 hook
- slide 2-5 supporting points
- final CTA slide

### 6) Comment/DM Lead-Routing Agent
หน้าที่:
- เตรียม reply pack
- สร้าง comment bait keyword map
- ทำ DM qualification flow

Output:
- public replies
- DM opener
- route suggestion

### 7) Performance Review Agent
หน้าที่:
- ดูว่าโพสต์ไหนเวิร์ก
- สรุป lesson และ repeatable pattern

Output:
- weekly insights
- next-week recommendations

## Expanded Version (Growth Team)
เพิ่มบทบาทเฉพาะทาง:
- Compliance Agent
- Repurposing Agent
- Community Signal Agent
- Archive/Knowledge Agent
- Offer Bridge Agent

## Standard Handoff Schema
ทุก agent ควรส่งต่อในรูปแบบนี้:
- Objective:
- Audience:
- Pillar:
- Format:
- Hook angle:
- Key message:
- CTA:
- Risk notes:
- Next agent request:

## Example Handoff Chain
Trend/Research -> Ops/Editor -> Script/Caption -> Video Concept OR Static Post -> Compliance Check -> Ops Finalize -> Lead-Routing -> Performance Review

## Escalation Rules
- ถ้าประเด็นเสี่ยง claim ให้ส่งเข้า compliance review ทันที
- ถ้า topic ดูน่าเบื่อ ให้ส่งกลับ Script/Caption เพื่อ sharpen hook
- ถ้าโพสต์ชนะ ให้ส่งต่อไป Repurposing flow
