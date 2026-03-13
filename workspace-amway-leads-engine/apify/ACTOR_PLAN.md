# APIFY ACTOR PLAN

## Goal
ใช้ Apify เป็นตัวดึง public Facebook signals เพื่อหา candidate leads

## ต้องมี environment
- `APIFY_TOKEN`
- (ถ้า actor ที่ใช้ต้อง login) credential ที่เกี่ยวข้องอย่างปลอดภัย

## Candidate approach

### Option A: Facebook pages/posts search actor
ใช้ actor ที่ดึง public Facebook pages/posts จาก keyword search
เหมาะสำหรับหา:
- side hustle
- work from home
- online seller
- looking for opportunity
- entrepreneur mom
- health beauty seller
- creator small business

### Option B: Custom Apify task
สร้าง Apify task ที่รับ keyword list + location hints + result limits แล้วคืน structured JSON

## Suggested search buckets
1. รายได้เสริม / side hustle
2. ขายของออนไลน์ / seller / owner
3. สุขภาพ ความงาม ไลฟ์สไตล์
4. personal development / leadership / growth mindset
5. looking for opportunities / career change

## Minimum fields to extract
- name
- profile/page url
- bio/about
- visible role/business
- location if visible
- recent post snippet
- page/category
- follower/friend/public signals if visible
- public contact signal if visible

## Expected output file
- `runs/latest/raw_candidates.json`

## Note
ตอนนี้ workspace นี้วาง integration plan ไว้แล้ว แต่ยังไม่รันจริงเพราะเครื่องนี้ไม่มี `APIFY_TOKEN`
