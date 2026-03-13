# workspace-amway-leads-engine

Multi-agent workspace สำหรับหา คัดกรอง วิเคราะห์ และร่างข้อความเปิดบทสนทนา สำหรับ lead ที่ "อาจเหมาะ" กับการนำเสนอธุรกิจแอมเวย์อย่างรับผิดชอบ

## เป้าหมาย

ระบบนี้ทำ 4 อย่างต่อเนื่อง:
1. หา candidate leads จาก Facebook ผ่าน Apify
2. คัดกรองว่าใคร "น่าจะเหมาะ" กับการนำเสนอธุรกิจแอมเวย์
3. วิเคราะห์ว่าควรเข้าหาแต่ละ lead ด้วยมุมไหน
4. ร่าง opening message ตามสไตล์ profile ของแต่ละคน

## Agents

- `orchestrator-agent` — คุม flow ทั้งระบบ, ส่งต่องาน, รวมผลลัพธ์
- `seeker-agent` — ค้นหา candidate leads จาก Facebook ด้วย Apify และ normalize ข้อมูล
- `analyse-agent` — ให้คะแนนว่า lead นี้ควรนำเสนอธุรกิจแอมเวย์ไหม พร้อม angle ที่ควรใช้
- `writer-agent` — ร่าง opening DM / comment-first / warm opener ให้เหมาะกับ profile style

## Output สุดท้าย

สำหรับอย่างน้อย 30 leads ระบบควรได้:
- ชื่อ / โปรไฟล์ / ลิงก์
- เหตุผลที่เข้าข่าย
- score
- verdict
- angle ที่ควรเข้าหา
- opening message
- compliance notes

## โครงไฟล์

- `SYSTEM_OVERVIEW.md` — ภาพรวมระบบ
- `AGENT_HANDOFFS.md` — input/output contract ของแต่ละ agent
- `COMPLIANCE.md` — guardrails สำคัญ
- `SCORING_MODEL.md` — rubric สำหรับ analyse-agent
- `apify/ACTOR_PLAN.md` — แผน Apify / ตัวเลือก actor / field mapping
- `prompts/` — prompt ตั้งต้นทุก agent
- `schemas/` — schema ของไฟล์ JSON/CSV
- `templates/` — ตัวอย่าง input/output
- `runs/` — output run จริง

## สถานะตอนนี้

โครงระบบพร้อมแล้ว แต่ **ยังไม่มี APIFY token หรือ Facebook credential ใน environment** จึงยังไม่สามารถดึง Facebook ผ่าน Apify ได้จริงจากเครื่องนี้ทันที

สิ่งที่ทำได้ทันที:
- ปรับ prompt / rubric / workflow
- เสียบ token แล้วรันได้ต่อ
- ใช้ข้อมูล lead ที่มีอยู่แล้วเข้า analyse-agent + writer-agent ได้เลย
