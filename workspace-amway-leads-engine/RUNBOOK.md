# RUNBOOK

## Current blocker
Environment นี้ยังไม่มี:
- `APIFY_TOKEN`
- Facebook credentials (ถ้า actor ที่เลือกต้องใช้)

## When token is available
1. ตั้ง env `APIFY_TOKEN`
2. เลือก/ยืนยัน actor ตาม `apify/ACTOR_PLAN.md`
3. ให้ `seeker-agent` ดึง candidate leads 60-100 รายชื่อ
4. ส่งเข้า `analyse-agent`
5. ส่ง shortlist เข้า `writer-agent`
6. export เป็น CSV ตาม schema ใน `schemas/final-output-schema.md`

## Best-effort fallback
ถ้ายังไม่มี Apify token แต่มีลิงก์หรือรายชื่อคร่าว ๆ อยู่แล้ว:
- feed เข้า `analyse-agent` ได้ทันที
- จากนั้นให้ `writer-agent` สร้าง opener ต่อ
