# RUNBOOK

## 1) Objective

สร้างระบบ multi-agent simulation เพื่อทดสอบการ sponsor ธุรกิจขายตรง/เครือข่ายแบบ ethical โดยวัดว่า:

- อธิบายธุรกิจชัดไหม
- ผู้ฟังรู้สึกถูกกดดันไหม
- persona นี้เหมาะจะไปต่อไหม
- บทพูดแบบไหนควรใช้ / ห้ามใช้

## 2) Standard Output Per Scenario

แต่ละ scenario ต้องให้ผลลัพธ์อย่างน้อย:

1. `scenario.json`
2. `transcript.md`
3. `evaluation.json`
4. `safety_review.json`
5. `scenario_summary.md`

## 3) Recommended Directory Layout

```text
runs/
  batch-001/
    scenario-001/
      scenario.json
      transcript.md
      evaluation.json
      safety_review.json
      scenario_summary.md
```

## 4) Agent Sequence

### Step A — Persona Generator
สร้าง personas ตาม schema ใน `schemas/persona.schema.yaml`

### Step B — Strategy Designer
สร้าง sponsor styles ตาม schema ใน `schemas/strategy.schema.yaml`

### Step C — Batch Planner
จับคู่ persona x strategy โดยไม่ต้อง full factorial ตั้งแต่แรก

แนะนำ matrix เริ่มต้น:
- 18 personas
- 6 strategies
- จับคู่แบบ stratified sample 36-54 scenarios

### Step D — Simulation Runner
กติกา:
- 8-12 turns
- realistic pacing
- prospect ไม่ควรตอบ yes ง่ายเกินจริง
- ถ้าไม่ fit ให้บทสนทนาออกมาทาง qualify-out ได้

### Step E — Evaluator
ให้คะแนน:
- trust_built
- clarity
- pressure_level
- persona_fit
- objection_handling
- next_step_quality
- ethical_continuation

### Step F — Safety Reviewer
ตรวจ:
- manipulation risk
- ambiguous earning claim
- hidden effort / hidden cost risk
- social pressure risk
- urgency / coercion risk

### Step G — Analyst
สรุป batch เป็น:
- persona-fit map
- strongest openings
- failed openings
- key objections
- best responses
- red flags
- qualification rubric

## 5) Batch Review Questions

หลังจบรอบ ให้ตอบคำถามนี้ทุกครั้ง:

1. Persona ไหนเปิดรับ แต่ไม่ควรไปต่อเพราะ expectation mismatch?
2. Persona ไหน distrust สูง แต่ถ้าคุยแบบ consultative จะไปต่อได้?
3. ประโยคไหนเพิ่ม trust มากที่สุด?
4. ประโยคไหนทำให้ pressure พุ่ง?
5. มี claim ไหนฟังเหมือน overpromise แม้ไม่ได้โกหกตรงๆ?
6. จุดไหนควรหยุดคุยและ qualify out?

## 6) Final Deliverables

ใน `reports/final-report.md` ควรมี:

- Executive summary
- Experiment design
- Persona segmentation
- Findings by strategy
- Findings by persona type
- Ethical risk findings
- Recommended playbook
- Do / Do-not-say list
- Qualification rubric
- Recommended conversation tree
- Next experiments

## 7) OpenClaw Execution Pattern

ตัวอย่างลำดับงาน:

1. spawn orchestrator
2. orchestrator สร้าง personas + strategies
3. orchestrator แตกงาน simulation เป็นชุด
4. แต่ละ simulation เขียนไฟล์ลง `runs/batch-001/...`
5. evaluator + safety reviewer อ่าน transcript แล้วเขียนผล
6. analyst อ่านทั้ง batch แล้วสร้าง `reports/batch-001-report.md`
7. orchestrator สรุป `reports/final-report.md`

## 8) Practical Tip

ถ้าต้องการความต่างของมุมมอง:
- ใช้ model/agent คนละตัวระหว่าง runner กับ evaluator
- ให้ evaluator ใช้ rubric เข้ม
- ให้ safety reviewer ทำหน้าที่ adversarial โดยเฉพาะ
