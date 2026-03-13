# SYSTEM OVERVIEW

## Objective
หา 30 candidate leads จาก Facebook ที่มีแนวโน้มเหมาะกับการนำเสนอธุรกิจแอมเวย์แบบไม่ hard-sell ไม่ overclaim และไม่หลุด compliance

## High-level flow

1. **orchestrator-agent** รับ brief และกำหนดเกณฑ์ target
2. **seeker-agent** ใช้ Apify ดึง candidate profiles/posts/pages/groups signals จาก Facebook
3. seeker-agent normalize ข้อมูลลง schema เดียว
4. **analyse-agent** ให้ score / verdict / angle / red flags
5. **writer-agent** ร่างข้อความเปิดบทสนทนาให้เหมาะกับ style ของแต่ละ lead
6. orchestrator รวมผลลัพธ์สุดท้ายเป็น shortlist 30 รายชื่อ

## Target lead categories (initial hypothesis)

เน้นคนที่มีสัญญาณอย่างน้อยบางข้อ:
- สนใจรายได้เสริม / side hustle / work from home
- มีพฤติกรรม creator / seller / online business owner
- แม่ค้าออนไลน์ / small business owner / freelancer ที่อยากเพิ่มรายได้
- คนที่โพสต์เรื่องอยากเปลี่ยนงาน / อยากมีรายได้เพิ่ม / มองหาโอกาสใหม่
- คนที่มี network และมีแนวโน้มชอบ community / coaching / self-development
- คนที่สนใจสุขภาพ ความงาม lifestyle และมี audience ที่ตรงบางส่วน

## Exclusions

ไม่ควร target เป็นหลักถ้า:
- ผู้เยาว์
- คนที่มีภาวะเปราะบางชัดเจนหรือกำลังเดือดร้อนหนัก
- คนที่โพสต์หางานแบบ distress สูงมาก
- คนที่ profile ดูปลอม / spam / abandoned
- คนที่มีสัญญาณต่อต้าน MLM/network marketing ชัดเจน
- คนที่ไม่มี buying/working autonomy เลย และไม่มีเหตุผลจะคุยต่อ

## Success criteria

Lead ที่ผ่านควรมี:
- โปรไฟล์จริงพอสมควร
- angle ที่ plausible ว่าคุยต่อได้
- opening message ที่ไม่ดู spam
- evidence ว่าทำไม shortlist นี้ควรถูกเลือก
