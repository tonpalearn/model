# Scoring Rubric — leads-ver-agent

ให้คะแนนรวมเต็ม 100 โดยใช้เกณฑ์นี้

## 1) Reality / legitimacy — 20 คะแนน
คำถาม: lead นี้ดูเป็น entity จริงที่มีตัวตนจริงไหม

ดูจาก:
- มีเว็บไซต์ / social / company page / footprint ที่สอดคล้องกัน
- ข้อมูลไม่ขัดกันเอง
- มี recent activity
- ดูไม่ใช่ spam / fake / scraped junk

ให้คะแนนคร่าวๆ:
- 0-5 = น่าสงสัยมาก / หาแทบไม่เจอ / profile บางผิดปกติ
- 6-10 = มีตัวตนบางส่วน แต่ยัง verify ไม่พอ
- 11-15 = ค่อนข้างจริง มีหลายสัญญาณรองรับ
- 16-20 = ชัดเจนว่าเป็น lead จริงและ active

## 2) ICP fit — 20 คะแนน
คำถาม: lead นี้ตรงกับกลุ่มเป้าหมายของเรามากแค่ไหน

ดูจาก:
- industry / niche
- company size / stage
- business model
- audience / market
- use case ที่ตรงกับ offer ของเรา

ให้คะแนนคร่าวๆ:
- 0-5 = แทบไม่ตรง
- 6-10 = ตรงบางส่วน
- 11-15 = fit ดีพอควร
- 16-20 = fit สูงมาก

## 3) Commercial potential — 15 คะแนน
คำถาม: lead นี้มีโอกาสสร้างดีลหรือมูลค่าทางธุรกิจแค่ไหน

ดูจาก:
- มี offer / product / service ชัดไหม
- มีลูกค้า / funnel / monetization
- มีงบหรือศักยภาพในการซื้อ / ร่วมงาน
- มี strategic upside ไหม

ช่วงคะแนน:
- 0-4 = มูลค่าต่ำ / ไม่ชัดว่าจะซื้ออะไร
- 5-8 = พอมีโอกาส แต่ไม่เด่น
- 9-12 = มีโอกาสดี
- 13-15 = โอกาสสูง / มูลค่าดี

## 4) Decision power — 10 คะแนน
คำถาม: คน/บัญชีนี้มีอำนาจตัดสินใจหรือ influence จริงไหม

ดูจาก:
- founder / owner / head / director / lead
- เป็นผู้ถือ budget หรือไม่
- เป็นผู้ใช้ที่มีอำนาจชี้นำหรือไม่

ช่วงคะแนน:
- 0-2 = แทบไม่มีอำนาจ
- 3-5 = influence บางส่วน
- 6-8 = มีอิทธิพลสูงหรือใกล้ decision-maker
- 9-10 = decision-maker ชัดเจน

## 5) Intent / timing — 15 คะแนน
คำถาม: ตอนนี้มีสัญญาณว่าควรคุยหรือควรทักไหม

ดูจาก:
- recent hiring / launch / fundraising / expansion
- recent posts ที่สะท้อน pain / need
- active looking / open-to / inquiry behavior
- seasonality หรือ timing ที่เหมาะ

ช่วงคะแนน:
- 0-4 = ไม่มีสัญญาณ timing
- 5-8 = มี hint บางอย่าง
- 9-12 = timing ดี
- 13-15 = timing แรงมาก ควรทักเร็ว

## 6) Reachability / access — 10 คะแนน
คำถาม: เข้าถึง lead นี้ได้ง่ายและเหมาะไหม

ดูจาก:
- มีช่องทางติดต่อชัดไหม
- DM ได้ไหม / email เปิดเผยไหม / มีฟอร์มไหม
- มี social presence ที่ใช้ outreach ได้จริงไหม

ช่วงคะแนน:
- 0-2 = ติดต่อยากมาก
- 3-5 = พอมีทาง แต่ไม่ชัด
- 6-8 = ติดต่อได้ค่อนข้างดี
- 9-10 = ช่องทางชัดและเหมาะต่อ outreach

## 7) Risk / mismatch adjustment — หัก 0 ถึง 10 คะแนน
หักคะแนนเมื่อ:
- lead mismatch ชัด
- ดูเป็น low-quality / spam / dead profile
- มี reputational risk
- ดูไม่มี buying power เลย
- ข้อมูล conflict กันเยอะ

แนวทาง:
- 0 = ไม่มี red flag สำคัญ
- -1 ถึง -3 = มี red flag เล็กน้อย
- -4 ถึง -7 = มีความเสี่ยงปานกลาง
- -8 ถึง -10 = red flag หนัก ควรเลี่ยง

---

# Score interpretation

## 80-100
- คุณภาพสูง
- ถ้าข้อมูลพอ → `GO`
- Priority มักเป็น `Hot`

## 65-79
- น่าสนใจ
- outreach ได้ ถ้ามี angle ชัด
- `GO` หรือ `MAYBE` ตาม confidence

## 50-64
- กลางๆ
- ควรหา info เพิ่มก่อน
- `MAYBE`

## 0-49
- โอกาสต่ำ / mismatch / verify ไม่ผ่าน
- มักเป็น `NO-GO`

---

# Confidence score

ให้แยกจาก lead score

## Confidence 80-100
- มี evidence หลายจุด
- สรุปได้ค่อนข้างมั่นใจ

## Confidence 60-79
- มีข้อมูลพอใช้ แต่ยังมี unknowns

## Confidence 40-59
- ข้อมูลจำกัด ต้องระวังการสรุป

## Confidence 0-39
- ข้อมูลน้อยมาก หรือขัดแย้งกัน
- ห้ามฟันธงแรง
