# OpenClaw Execution Plan

เอกสารนี้คือวิธีสั่งงานให้ระบบเดินเองแบบเป็นขั้นตอน

## Option A — ใช้ main agent เป็น orchestrator แล้วแตกงานเป็น sub-agents

ลำดับงาน:
1. สร้าง batch config จาก `templates/experiment-batch.yaml`
2. สร้าง personas และ strategies จาก templates/sample files หรือ generate ใหม่
3. สำหรับแต่ละ scenario:
   - spawn simulation runner
   - ให้ runner สร้าง transcript
   - spawn evaluator เพื่ออ่าน transcript และออก `evaluation.json`
   - spawn safety reviewer เพื่ออ่าน transcript และออก `safety_review.json`
4. เมื่อครบ batch ให้ spawn analyst เพื่อสรุปลง `reports/batch-001-report.md`
5. orchestrator สร้าง `reports/final-report.md`

## Option B — ใช้ ACP harness หลายตัว

แนะนำ mapping:
- Orchestrator / Analyst -> agent ที่เก่ง synthesis
- Simulation Runner -> agent ที่โต้ตอบเป็นธรรมชาติ
- Safety Reviewer -> agent ที่ strict เรื่อง language risk

## Suggested Task Contracts

### 1) Persona generation task
สร้าง personas 18 แบบตาม `schemas/persona.schema.yaml`
เขียนผลไปที่ `runs/batch-001/personas.yaml`

### 2) Strategy generation task
สร้าง sponsor strategies 6 แบบตาม `schemas/strategy.schema.yaml`
เขียนผลไปที่ `runs/batch-001/strategies.yaml`

### 3) Scenario planning task
จับคู่ persona + strategy เป็น 36-54 scenarios
เขียนผลไปที่ `runs/batch-001/scenarios.yaml`

### 4) Simulation task
รับ persona + strategy + scenario constraints
สร้าง transcript 8-12 turns และบันทึกที่ `runs/batch-001/scenario-XXX/transcript.md`

### 5) Evaluation task
อ่าน transcript แล้วเขียน `evaluation.json`

### 6) Safety task
อ่าน transcript แล้วเขียน `safety_review.json`

### 7) Batch analysis task
อ่าน outputs ทั้ง batch แล้วสรุป report

## Important Operating Rules

- ใช้ schema เดิมทุกรอบเพื่อเทียบผลได้
- ถ้าพบว่า persona ไม่เหมาะ ให้ qualify out ได้ทันที
- ถ้าประโยคไหน borderline manipulative ให้ safety reviewer flag แม้ transcript โดยรวมดูดี
- final report ต้องแยก clearly ระหว่าง:
  - pattern ที่ช่วยให้คุยชัดเจนขึ้น
  - pattern ที่เสี่ยงกดดันหรือ misleading

## Suggested Directory for Batch 1

```text
runs/
  batch-001/
    personas.yaml
    strategies.yaml
    scenarios.yaml
    scenario-001/
    scenario-002/
    ...
```

## Definition of Done

งานถือว่าเสร็จเมื่อมี:
- prompt pack
- schemas
- batch config
- initial personas/strategies examples
- execution plan
- runbook
- พร้อมให้ orchestrator หรือ sub-agents เริ่ม batch แรกได้
