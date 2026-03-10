# Ethical Sponsor Simulation Lab

ระบบจำลองบทสนทนาแบบ multi-agent สำหรับทดสอบการชวนทำธุรกิจขายตรง/เครือข่ายแบบ **โปร่งใส ไม่กดดัน และเน้นคัดกรองความเหมาะสม** แทนการ optimize เพื่อปิดการชวนให้ได้มากที่สุด

## เป้าหมาย

ใช้ระบบนี้เพื่อหา:

- persona แบบไหนเหมาะ / ไม่เหมาะ
- opening / framing แบบไหนสร้างความเชื่อใจมากกว่า
- objection ไหนต้องตอบอย่างไรจึงชัดเจนและไม่กดดัน
- red flags ที่ควรหยุดบทสนทนา
- qualification rubric สำหรับคัดออกอย่างรับผิดชอบ

## สิ่งที่ระบบนี้ไม่ทำ

- ไม่ optimize วิธีชักจูงตาม demographic เพื่อให้คนยอมเข้าร่วมมากที่สุด
- ไม่ออกแบบข้อความโกหก ปกปิดต้นทุน หรือ overpromise รายได้
- ไม่ใช้ emotional pressure, shame, urgency, social coercion

## โครงสร้างระบบ

1. **Orchestrator**
   - วาง batch
   - จับคู่ sponsor strategy x prospect persona
   - ส่งงานต่อให้ agent อื่น
   - รวมผลและสรุป final recommendation

2. **Persona Generator**
   - สร้าง prospect persona ที่สมจริง
   - ระบุ motivations, concerns, trust threshold, prior MLM experience

3. **Sponsor Strategy Designer**
   - สร้างสไตล์การคุยหลายแบบภายใต้ ethical guardrails
   - ไม่ใช้คำพูดหลอกล่อหรือเร่งตัดสินใจ

4. **Simulation Runner**
   - รันบทสนทนา 8-12 turns ต่อ scenario
   - บังคับให้ realistic และไม่ปิดจบง่ายเกินจริง

5. **Evaluator**
   - ให้คะแนน trust / clarity / pressure / fit / next step quality
   - ระบุ turning points, helpful phrases, harmful phrases

6. **Adversarial Safety Reviewer**
   - จับ manipulation risk
   - ชี้จุดที่เสี่ยง backlash / misunderstanding / unethical pressure

7. **Analyst / Synthesizer**
   - สรุปผลข้ามทุก transcript
   - ทำ playbook + qualification rubric + conversation tree

## โครงสร้างไฟล์

- `prompts/` prompt pack สำหรับ agent แต่ละบทบาท
- `schemas/` schema ของ persona, scenario, scorecard, aggregate report
- `runs/` เก็บ input/output ของแต่ละรอบ
- `reports/` รายงานสรุปรอบทดลอง
- `templates/` ไฟล์ตั้งต้นของ batch และ experiment matrix

## Workflow แนะนำ

### Phase 1 — Design
- สร้าง prospect personas 20-30 แบบ
- สร้าง sponsor approaches 6-8 แบบ
- นิยาม scorecard และ safety rubric

### Phase 2 — Batch 1
- รัน 30-60 scenarios
- เก็บ transcript และ structured evaluation

### Phase 3 — Analysis
- หา pattern ที่สร้าง trust
- หา pattern ที่ทำให้เกิด pressure / distrust
- หา persona ที่ควร qualify out

### Phase 4 — Improvement
- ปรับ opening, framing, objections handling
- rerun เฉพาะ difficult personas

### Phase 5 — Final Deliverable
- persona-fit map
- best openings by persona type
- objections + best responses
- do / do-not-say list
- qualification rubric
- recommended conversation tree

## วิธีเอาไปใช้กับ OpenClaw

### แบบใช้ sub-agents
- Spawn 1 orchestrator session
- ให้ orchestrator ส่งงานไปหา sub-agents ตามบทบาท
- เก็บ structured outputs ลง `runs/`
- ให้ analyst สรุป final report ลง `reports/`

### แบบใช้ ACP harness
ใช้ `sessions_spawn` แยกบทบาท เช่น:
- `runtime: "acp"`, `agentId: "codex"` สำหรับ analyst / orchestrator
- `runtime: "acp"`, `agentId: "claude-code"` หรือ agent อื่นสำหรับ simulation-heavy roles

> หมายเหตุ: ควรใช้ prompt + schema เดียวกันทุกครั้งเพื่อให้เทียบผลข้ามรอบได้

## Success Criteria

ผลลัพธ์ที่ถือว่า “สำเร็จ” คือ:
- ได้ insight ว่า persona ไหนควรไปต่อ / ควรคัดออก
- ได้รูปแบบการคุยที่เพิ่มความชัดเจนและความเชื่อใจ
- ลด manipulation risk และ false expectations
- ได้ playbook ที่นำไปใช้ซ้ำได้จริง

## Suggested First Batch

- personas: 18
- strategies: 6
- conversations: 36-54
- turns per conversation: 8-12
- evaluation: 1 judge + 1 safety reviewer ต่อ transcript

ดูไฟล์ `RUNBOOK.md` และ `templates/experiment-batch.yaml` เพื่อเริ่มรันงาน
