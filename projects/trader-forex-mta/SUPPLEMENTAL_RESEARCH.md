# SUPPLEMENTAL_RESEARCH

อัปเดตล่าสุด: 2026-03-09 08:4x ICT
แหล่งที่ตรวจเพิ่ม: canonical Facebook page/reels/videos, HTML/OG metadata, header response, Bing RSS search traces

> เอกสารนี้เป็นงานเสริมจากรอบวิจัยหลัก เน้นเฉพาะ “หลักฐานสาธารณะที่พอเห็นเพิ่มได้” โดยแยกให้ชัดว่าอะไรคือสิ่งที่เห็นตรง ๆ และอะไรคือข้อสรุปเชิงระมัดระวัง

---

## 1) หลักฐานใหม่ที่พบ

### 1.1 ตัวเลข social proof มีการขยับเล็กน้อยจาก snapshot ก่อนหน้า
จากการดึง metadata รอบนี้ที่ `https://www.facebook.com/MasterTraderAcademy` และ `.../reels/` พบว่า:
- ถูกใจ **128,103 คน**
- กำลังพูดถึงสิ่งนี้ **11,245 คน**
- เคยมาที่นี่ **23 คน**

เทียบกับไฟล์วิจัยหลักที่เคยบันทึก 128,099 likes แปลได้เพียงว่า:
- เพจยัง active ในระดับหนึ่ง
- ตัวเลขเป็น snapshot ที่ขยับได้จริง
- ยังไม่พอใช้สรุป momentum ของ audience หรือคุณภาพคอนเทนต์

### 1.2 ยืนยันได้เพิ่มว่าเพจมีทั้ง tab หน้าแรก / reels / videos แบบเป็นระบบ
จาก HTML ที่เปิดได้สาธารณะ พบ route/metadata ที่ชี้ชัดว่าเพจมี section เหล่านี้:
- หน้าเพจหลัก: `https://www.facebook.com/MasterTraderAcademy`
- Reels tab: `https://www.facebook.com/MasterTraderAcademy/reels/`
- Videos tab: `https://www.facebook.com/MasterTraderAcademy/videos`

และใน HTML มี route name ที่น่าสนใจ:
- `comet.fbweb.CometProfilePlusLoggedOutRoute`
- `comet.fbweb.CometProfileReelsTabTabRoute`
- `comet.fbweb.ProfileCometProfilePlusVideosRoute`

สิ่งนี้ไม่ได้บอก strategy โดยตรง แต่ช่วยยืนยันว่าแบรนด์นี้ไม่ได้พึ่งแค่โพสต์หน้า feed อย่างเดียว — มีการจัดคอนเทนต์ในรูปแบบวิดีโอ/คลิปสั้นเป็นโครงสร้างปกติของเพจ

### 1.3 พบ page/profile identifiers เพิ่มเติม
จาก HTML สาธารณะพบ identifier หลักดังนี้:
- vanity: `MasterTraderAcademy`
- profile/user id: `100083132751008`
- page id (พบใน videos route payload): `105425355561988`
- deep link app: `fb://profile/100083132751008`

ผลเชิงวิจัย:
- ช่วยยืนยันว่า source canonical ที่ควรอ้างคือเพจนี้จริง
- ถ้าทีมต้องการเก็บ snapshot เพิ่มในอนาคต ควรอ้างทั้ง vanity + ids เพื่อกัน rename / redirect

### 1.4 คำอธิบายของ Reels สะท้อน framing แบบ “คลิปล่าสุด” ชัดเจน
จาก `og:description` ของหน้า reels มีข้อความช่วงท้ายว่า:
- **“รับชมคลิป Reels ล่าสุดจาก Master Trader...”**

แม้จะยังไม่ได้ title/caption รายคลิป แต่ข้อความนี้ช่วย reinforce ว่า:
- Reels น่าจะเป็นช่องทางสื่อสารที่ใช้งานจริง ไม่ใช่ tab ที่มีไว้เฉย ๆ
- ถ้า MTA มีสไตล์สอนหรือสไตล์ signal เฉพาะตัว ส่วนหนึ่งน่าจะถูกย่อยให้อยู่ในรูป short-form ได้

### 1.5 พบเพียง “ร่องรอยคำ” นอก Facebook น้อยมาก และยังไม่พอถือเป็นหลักฐานเสริมแข็งแรง
ลองตรวจ Bing RSS search ด้วยคำค้น เช่น:
- `"Master Trader Academy TH"`
- `"วิเคราะห์กราฟทองทุกวัน"`
- `"#pexstrategy"`
- `"THE MTA WEBSITE is already ready for the launch"`

ผลที่ได้:
- ไม่พบ off-platform trace ที่ชัดเจนพอจะโยงกลับมาที่ MTA แบบมั่นใจสูง
- คำค้นเฉพาะอย่าง `#pexstrategy` และ quote เดิมเรื่อง website launch **ยังไม่ถูกยืนยันซ้ำ** จากผลค้นหาสาธารณะรอบนี้

ข้อสรุปที่ปลอดภัยที่สุดคือ:
- ตอนนี้ “ร่องรอยนอก Facebook” ยังอ่อนมาก
- สิ่งที่หนักแน่นที่สุดยังคงเป็นคำอธิบายหน้าเพจและโครงสร้าง tabs บน Facebook เอง

### 1.6 มี URL รูปโปรไฟล์ CDN สาธารณะ แต่ยังไม่ควรตีความเกินภาพแบรนด์
จาก HTML หน้าเพจพบลิงก์รูปภาพโปรไฟล์บน fbcdn (`422591067_...jpg`)

สิ่งที่บอกได้จริง:
- เพจมี asset รูปโปรไฟล์ที่ถูกเสิร์ฟแบบ public CDN
- ถ้าทีมต้องการทำ visual audit ภายหลัง สามารถใช้เป็นจุดเริ่มต้นได้

สิ่งที่ยังบอกไม่ได้:
- โลโก้/โทนภาพนี้สื่อแนวกลยุทธ์ใดโดยตรง
- ความเชื่อมโยงกับ Pex หรือ strategy brand อื่น

---

## 2) ระดับความมั่นใจ

### สูง
- เพจใช้แบรนด์ `Master Trader Academy TH`
- มี messaging หลักว่า **สอนเทรด Forex / วิเคราะห์กราฟทองทุกวัน / ห้อง Signal**
- มี Reels tab และ Videos tab จริง
- มี identifiers ที่เก็บอ้างอิงได้: vanity, profile/user id, page id

### กลาง
- Short-form video น่าจะเป็นช่องทางสื่อสารสำคัญ ไม่ใช่แค่ส่วนเสริม
- เนื้อหา gold analysis น่าจะถูกทำซ้ำอย่างสม่ำเสมอในรูป daily cadence
- การสื่อสารน่าจะเน้นเนื้อหาที่ “ย่อยเป็นคลิปสั้นได้” เช่น bias, level, setup, plan

### ต่ำถึงกลาง
- `Pex`, `#pexstrategy`, หรือข้อความ launch เก่า ยังเป็นเพียง historical fragment จากรอบก่อน
- ยังไม่พบ search-engine trace ภายนอกที่ช่วยยืนยัน branding/sub-brand นี้เพิ่ม
- ยังไม่เห็น caption รายคลิปหรือ post text เต็มพอจะฟันธงศัพท์ recurring แบบละเอียด

---

## 3) ผลกระทบที่เป็นไปได้ต่อการออกแบบกลยุทธ์

### 3.1 ควรคิดระบบให้อธิบายเป็น “คลิปสั้น/โพสต์สั้น” ได้
เพราะเพจมีทั้ง Reels และ Videos อย่างชัดเจน หากจะออกแบบ logic ที่ได้แรงบันดาลใจจาก MTA แบบ conservative ควรเป็นระบบที่สรุปเป็นหน่วยสั้น ๆ ได้ เช่น:
- daily bias
- โซนที่รอราคาเข้า
- เงื่อนไขยืนยันก่อนเข้า
- จุด invalidation / stop
- เป้าราคาเบื้องต้น

นัยสำคัญคือ กลยุทธ์ที่ซับซ้อนหลายชั้นเกินไปอาจไม่สอดคล้องกับสไตล์ content delivery ที่เพจนี้น่าจะใช้

### 3.2 Gold-first ยังสมเหตุผล แต่ควรเสริม “content cadence layer” เข้าไปด้วย
จากคำว่า “วิเคราะห์กราฟทองทุกวัน” บวกกับการมี reels/videos เป็นระบบ มีผลต่อ bot design ว่า:
- ไม่ใช่แค่หา entry model
- แต่ควรมี layer สำหรับสรุป market view รายวันด้วย

พูดอีกแบบคือ ถ้าจะเลียนโครงวิธีสื่อสารของเพจ มากกว่าทำ signal ดิบอย่างเดียว ควรมีทั้ง:
1. daily market framing
2. setup conditions
3. executable signal (เมื่อเงื่อนไขครบ)

### 3.3 แยก “analysis mode” ออกจาก “signal mode” น่าจะเหมาะกับ brand fit นี้
เพราะหน้าเพจใช้ทั้งคำว่า “วิเคราะห์กราฟทองทุกวัน” และ “ห้อง Signal” พร้อมกัน จึงมีผลเชิงออกแบบว่า bot อาจควรมี 2 ชั้น:
- **Analysis mode**: สรุป bias, level, scenario
- **Signal mode**: ส่งเฉพาะตอนมี trigger/confirmation

นี่เป็นข้อเสนอเชิงโครงสร้าง ไม่ใช่การอ้างว่า MTA ทำแบบนี้แน่นอน แต่เป็น interpretation ที่เข้ากับ positioning ของเพจที่สุดจากหลักฐานปัจจุบัน

### 3.4 อย่า overfit ไปที่คำหรือแบรนด์รองที่ยังยืนยันไม่ได้
เพราะ off-platform trace ยังไม่แข็งแรง การออกแบบ strategy ไม่ควรผูกกับสิ่งเหล่านี้เร็วเกินไป:
- Pex / pexstrategy
- website launch narrative
- ชื่อคอร์ส/ชื่อระบบย่อยที่ยังหา source ตรงไม่ได้

ควรยึดแกนที่เห็นจริงก่อน: Forex + gold daily analysis + signal-room framing + short-form/video usage

---

## 4) คำถามที่ยังไม่คลี่คลาย

1. **คำศัพท์ที่ใช้ซ้ำจริงในโพสต์/รีลคืออะไร?**
   - เช่น bias, breakout, retest, liquidity, structure, BOS/CHoCH, sweep, TP/SL ฯลฯ
   - ตอนนี้ยังไม่มีหลักฐานตรงพอ

2. **สัญญาณของ MTA เป็นแบบไหน?**
   - ส่งเป็น buy/sell ตรง ๆ หรือเป็น scenario analysis
   - เน้น intraday, scalp, swing หรือ session-specific
   - ยังยืนยันไม่ได้

3. **ทองคำถูกเทรดในกรอบแนวคิดใด?**
   - price action ทั่วไป, SMC/ICT, indicator-based, session/news driven หรือ hybrid
   - จากข้อมูลรอบนี้ยังสรุปไม่ได้

4. **Videos tab กับ Reels tab ใช้คนละบทบาทหรือไม่?**
   - เช่น Reels สำหรับ hook/สรุปไว และ Videos สำหรับ walkthrough ยาวกว่า
   - มีความเป็นไปได้ แต่ยังไม่มีหลักฐาน caption/title รายคลิปมารองรับ

5. **มี ecosystem นอก Facebook จริงหรือไม่?**
   - website, Telegram/LINE/Discord, คอร์ส, ห้องสมาชิก, sub-brand อื่น
   - รอบนี้ยังหา public trace เพิ่มไม่ได้แบบมั่นใจ

---

## 5) สรุปสั้นสำหรับทีมหลัก

หลักฐานเสริมที่มีประโยชน์จริงในรอบนี้คือ:
- ยืนยันโครงเพจว่ามีทั้ง **หน้าเพจหลัก + Reels + Videos**
- เก็บ identifier ได้เพิ่ม: `user/profile id 100083132751008` และ `page id 105425355561988`
- snapshot engagement ขยับเป็น **128,103 likes / 11,245 talking about this / 23 visited**
- ยังไม่พบ off-platform trace ที่แข็งแรงพอจะยืนยัน `Pex` หรือ narrative อื่นเพิ่ม

ดังนั้น ถ้าจะใช้รอบนี้ไปช่วยออกแบบ strategy ให้ conservative ที่สุด:
- ให้ยึด MTA เป็น **gold-focused daily analysis brand with signal-room framing**
- และคิดระบบให้สื่อสารได้ทั้งในรูป **analysis summary** และ **actionable signal**
- แต่ยังไม่ควรอ้างศัพท์หรือ mechanics เชิงลึกของกลยุทธ์ จนกว่าจะมีโพสต์/คลิปที่เข้าถึงได้ชัดกว่านี้
