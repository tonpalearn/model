# leads-ver-agent

Agent สำหรับคัดกรองและ verify ว่า lead ที่ส่งเข้ามาเป็น "lead จริงที่ควรทัก" หรือไม่ พร้อมให้คะแนนความน่าสนใจ และแนะนำวิธี approach ที่เหมาะสม

## เป้าหมาย

ช่วยตอบ 4 คำถามหลัก:
1. Lead นี้ "มีอยู่จริง / ดูน่าเชื่อถือ" แค่ไหน
2. Lead นี้ "ตรงกลุ่มที่เราควรคุย" แค่ไหน
3. Lead นี้ "ควรทักไหมตอนนี้" แค่ไหน
4. ถ้าจะทัก ควร approach แบบไหน

## Input ที่ agent รับได้

- รายชื่อบุคคล / บริษัท / แบรนด์
- ลิงก์โปรไฟล์, เว็บไซต์, landing page, social profile
- ข้อความแนะนำตัว, bio, about page
- รายละเอียดประกอบ เช่น role, industry, geography, audience, team size, recent activity
- notes เพิ่มเติมจากคนส่ง lead เข้ามา

## Output หลัก

- Verdict: `GO` / `MAYBE` / `NO-GO`
- Lead Score: 0-100
- Confidence Score: 0-100
- Priority: `Hot` / `Warm` / `Cold`
- Lead Type
- เหตุผลว่าทำไมใช่ / ไม่ใช่ lead ที่ควรทัก
- ความเสี่ยง / red flags
- วิธี approach ที่เหมาะ
- Opening angle / outreach angle
- สิ่งที่ต้องเก็บเพิ่มก่อนทัก

## ไฟล์สำคัญ

- `SYSTEM_PROMPT.md` — prompt ตั้งต้นของ agent
- `RUBRIC.md` — เกณฑ์ scoring และการตัดสิน
- `WORKFLOW.md` — ขั้นตอนการทำงาน
- `examples/sample-input.md` — ตัวอย่าง input
- `examples/sample-output.md` — ตัวอย่าง output

## Use case หลัก

- คัดรายชื่อ leads ก่อนทีม sales / outreach ทัก
- ตรวจว่า lead เป็นคนจริง / บริษัทจริง / มีสัญญาณซื้อจริงไหม
- แยกว่า lead ไหนควรทักด้วย DM, email, comment, intro call หรือ nurture ก่อน
- ลดการเสียเวลาไปกับ lead ที่ profile ดูดีแต่ intent ต่ำ

## หลักคิด

Agent นี้ไม่ได้แค่ถามว่า "lead ดีไหม" แต่ถามว่า:
- ดีสำหรับ "เรา" ไหม
- ดี "ตอนนี้" ไหม
- ควรคุยแบบไหนถึงมีโอกาสปิดหรือได้นัด

## Quick start

1. เอา prompt จาก `SYSTEM_PROMPT.md` ไปใช้เป็น system / developer prompt
2. แนบ `RUBRIC.md` ให้ agent ใช้เป็นเกณฑ์ตัดสิน
3. ส่งข้อมูล lead เข้าไปเป็นชุด
4. ให้ agent ตอบตาม format เดียวกันทุกครั้ง

## หมายเหตุ

ถ้าข้อมูลไม่พอ Agent ต้องไม่เดาสุ่มเกินจริง แต่ให้แยกชัดว่า:
- อะไรคือ fact
- อะไรคือ inference
- อะไรที่ยังต้อง verify เพิ่ม
