# AGENTS.md — workspace-amway-leads-engine

## Purpose
Workspace นี้ใช้สำหรับระบบ multi-agent หาและคัดกรอง leads จาก Facebook เพื่อการ outreach ธุรกิจแอมเวย์อย่างรับผิดชอบ

## Agents in this workspace
- orchestrator-agent
- seeker-agent
- analyse-agent
- writer-agent

## Operating rules
- ไม่มั่วรายชื่อ
- ไม่สร้าง lead ปลอมเติมให้ครบจำนวน
- ถ้าดึงข้อมูลจริงไม่ได้ ให้รายงานตามจริง
- เคารพ compliance และไม่ใช้ messaging แบบกดดัน
- ถ้า profile เป็นผู้เยาว์ / distressed / anti-MLM ชัด ให้ตัดออก

## Expected artifact
ผลลัพธ์ท้ายสุดควรเป็น shortlist พร้อม score + angle + opener
