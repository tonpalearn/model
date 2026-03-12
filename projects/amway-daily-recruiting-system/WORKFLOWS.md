# WORKFLOWS.md

## Workflow 1: Daily startup
1. เปิด `trackers/recruiting_lead_tracker.csv`
2. เปิด `trackers/facebook_group_signal_tracker.csv`
3. เปิด `trackers/outreach_queue.csv`
4. เปิด `trackers/followup_queue.csv`
5. เปิด `trackers/daily_recruiting_scoreboard.csv`
6. เลือก top priorities:
   - 3 follow-up สำคัญ
   - 2-5 contextual group DMs ที่มี signal จริง
   - 5-10 outreach ใหม่
   - 1 content ชิ้นหลัก

## Workflow 2: From content engagement to DM
1. มีคน react/comment/post reply
2. จดชื่อใน lead tracker พร้อม source = content
3. ส่ง DM ด้วยบริบทจาก engagement
4. ถ้าตอบ -> ไป light qualification
5. ถ้ามี signal -> invite หรือ nurture ตาม stage

## Workflow 3: Facebook Group prospecting
1. ตรวจเฉพาะกลุ่ม Tier A / Tier B ที่มีกิจกรรมจริง
2. หาโพสต์/คอมเมนต์ที่มี signal เรื่อง pain, goal, readiness
3. ให้คะแนนจาก `templates/facebook_group_signal_scorecard.md`
4. log ลง `trackers/facebook_group_signal_tracker.csv`
5. แยก Warm / Watchlist / Skip
6. ทำ soft engagement ก่อน DM เมื่อ context ยังไม่พอ
7. ส่ง DM เฉพาะรายที่มีบริบทจริง
8. update `trackers/recruiting_lead_tracker.csv` และ `trackers/outreach_queue.csv`

## Workflow 4: Warm outreach
1. คัดรายชื่อจาก outreach queue
2. ให้ Outreach Copy Agent ช่วย personalize
3. ตรวจ compliance
4. ส่งทีละคนอย่างมีสติ ไม่ยิงชุดเดียวแบบ robot
5. บันทึก last contact + stage

## Workflow 5: Qualification to invite
1. สรุป pain/goal/readiness
2. ให้ Qualification Agent จัด fit score
3. ถ้า fit กลางขึ้นไปและ timing ดี -> invite
4. ถ้ายังไม่พร้อม -> nurture + set follow-up date
5. ถ้า no fit -> close politely

## Workflow 6: Daily review
1. นับ activity + outcomes
2. กรอก scoreboard
3. เขียน 3 บรรทัด
   - วันนี้อะไรเวิร์ก
   - อะไรไม่เวิร์ก
   - พรุ่งนี้จะปรับอะไร

## Workflow 7: Weekly review
1. ดู weekly pipeline review
2. หาว่า bottleneck อยู่จุดไหน
3. เลือก experiment 1-2 อย่างสำหรับสัปดาห์หน้า
   - ปรับ opener
   - ปรับ CTA
   - ปรับ invite framing
   - ปรับคุณภาพรายชื่อ

## Workflow 8: Automation-assisted follow-up
1. ใช้ reminder/CRM ดึง daily queue จาก next_action_date
2. ให้ AI ช่วยร่างข้อความจาก stage + note ล่าสุด
3. review โดยมนุษย์ก่อนส่งทุกครั้ง
4. update tracker หลังมีผลลัพธ์
5. ถ้าเป็น flow ที่เริ่มจาก comment-to-DM หรือ lead form ให้ส่งเฉพาะสิ่งที่คนร้องขอหรือยอมรับไว้

## Workflow 9: Prospect closure
ใช้เมื่ออีกฝ่ายไม่สนใจ / ไม่เหมาะ / ไม่ตอบหลายรอบ
- ส่ง close-the-loop แบบสุภาพ
- mark stage = Not now หรือ No fit
- archive ออกจาก active queue
