# UPWellness TikTok Engine

ระบบปฏิบัติการคอนเทนต์ TikTok สำหรับช่อง **UPWellness**

โฟกัสหลัก:
- คอนเทนต์ภาษาไทยเกี่ยวกับ longevity / wellness / healthy living
- ทำคอนเทนต์ให้น่าสนใจ ชวนคุย ชวนสงสัย ไม่ขายแข็ง
- พาไปสู่บทสนทนาต่อเรื่องสินค้า/ไลฟ์สไตล์/โอกาสทางธุรกิจอย่างระมัดระวัง
- หลีกเลี่ยง medical claims และ income claims
- รองรับการทำงานแบบ multi-agent handoff

## สิ่งที่อยู่ในโปรเจกต์นี้

เอกสารแกนระบบ:
- `PROJECT_BRIEF.md`
- `CONTENT_STRATEGY.md`
- `AUDIENCE_PERSONAS.md`
- `CONTENT_PILLARS.md`
- `DAILY_PRODUCTION_SYSTEM.md`
- `AGENT_ARCHITECTURE.md`
- `HOOK_LIBRARY.md`
- `CTA_AND_LEAD_ROUTING.md`
- `COMPLIANCE_GUARDRAILS.md`
- `WORKFLOWS.md`

โฟลเดอร์ใช้งาน:
- `agents/` prompt พร้อมใช้สำหรับแต่ละ agent
- `templates/` แม่แบบคอนเทนต์และปฏิทิน
- `trackers/` ตัวติดตามคิวงาน ไอเดีย และ lead
- `examples/` ตัวอย่างคอนเทนต์พร้อมใช้เริ่มต้น

## วิธีเริ่มใช้งานเร็วที่สุด

1. อ่าน `PROJECT_BRIEF.md` และ `COMPLIANCE_GUARDRAILS.md`
2. เปิด `DAILY_PRODUCTION_SYSTEM.md` เพื่อเลือกโหมดการผลิต
3. ใช้ prompt ใน `agents/` เพื่อให้ agent ผลิตงานตามลำดับ handoff
4. บันทึกแผนโพสต์ใน `trackers/content-calendar-template.csv`
5. ใช้ `examples/` เป็น baseline สำหรับวันแรก

## Daily Output Target

### Sustainable Mode
- 2 ชิ้น/วัน
  - 1 วิดีโอ talking-head หรือ B-roll voiceover
  - 1 static/slideshow/photo-style post

### Higher-Output Mode
- 4 ชิ้น/วัน
  - 2 วิดีโอ
  - 1 slideshow
  - 1 comment-bait / quick hook / reply-style post

## Recommended Weekly Mix
- 30% sleep / recovery / energy
- 25% nutrition / habits / daily routine
- 20% healthy aging / longevity mindset
- 15% curiosity / myth / surprising facts
- 10% community / personal story / opinion

## Success Metrics
- Hook hold rate 3 วินาทีแรก
- Average watch time
- Comment rate
- Profile visits
- DM starts / inbound conversations
- Qualified conversations ที่ไปต่อแบบไม่กดดัน

## Important
ระบบนี้ออกแบบเพื่อ **สร้างความสนใจและความไว้วางใจ** ไม่ใช่เร่งปิดการขายด้วยคำกล่าวอ้างเกินจริง
