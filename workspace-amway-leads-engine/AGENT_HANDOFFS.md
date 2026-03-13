# AGENT HANDOFFS

## 1) orchestrator-agent
### รับ
- business goal
- target geography
- target language
- exclusions
- desired lead count

### ส่งต่อให้ seeker-agent
- persona hypotheses
- search queries
- required fields
- minimum evidence threshold

### รับกลับจาก seeker-agent
- raw candidate list
- normalized lead list
- source evidence

### ส่งต่อให้ analyse-agent
- normalized leads
- scoring rubric
- compliance rules

### รับกลับจาก analyse-agent
- scored leads
- verdicts
- angles
- red flags

### ส่งต่อให้ writer-agent
- shortlisted leads
- lead angle
- tone constraints
- profile style notes

### รับกลับจาก writer-agent
- opening message
- backup opener
- CTA type
- what-not-to-say

---

## 2) seeker-agent
### หน้าที่
- ใช้ Apify หารายชื่อจาก Facebook surfaces ที่เกี่ยวข้อง
- เก็บ evidence เท่าที่เข้าถึงได้
- normalize fields ให้พร้อมวิเคราะห์

### Facebook surfaces ที่ให้มองหา
- public profiles
- pages
- groups (ถ้ามี public metadata)
- public posts / comments / bios / intros
- page about sections

### Output schema ต่อ 1 lead
- full_name
- profile_type
- profile_url
- location
- headline_or_bio
- business_or_role
- audience_signal
- side_hustle_signal
- health_beauty_signal
- entrepreneurship_signal
- recent_activity_summary
- source_surface
- evidence_points[]
- data_quality

---

## 3) analyse-agent
### หน้าที่
- วิเคราะห์ว่า lead นี้ควรนำเสนอธุรกิจแอมเวย์ไหม
- ให้คะแนนและวิธีเข้าหา

### Output schema ต่อ 1 lead
- lead_score
- confidence_score
- verdict
- priority
- likely_fit_reason
- best_angle
- opening_topic
- disqualifiers
- compliance_notes

---

## 4) writer-agent
### หน้าที่
- เขียน opening conversation ตาม style ของ profile

### Output schema ต่อ 1 lead
- approach_mode
- style_read
- opener_primary
- opener_backup
- CTA_soft
- avoid_phrases
- personalization_basis
