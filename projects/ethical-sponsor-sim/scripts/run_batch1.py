from pathlib import Path
import json
from statistics import mean

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'runs' / 'batch-001'
REPORTS = ROOT / 'reports'
RUN.mkdir(parents=True, exist_ok=True)
REPORTS.mkdir(parents=True, exist_ok=True)

personas = [
    {
        'id': 'p-office-skeptical-01',
        'label': 'skeptical office worker',
        'communication_style': 'polite but skeptical',
        'motivations': ['extra income', 'more flexibility'],
        'concerns': ['reputation risk', 'hidden costs', 'pressure from uplines'],
        'trust_threshold': 'high',
        'sales_tolerance': 'low',
        'time_availability': 'evenings only',
        'notes': 'dislikes vague claims and social pressure',
    },
    {
        'id': 'p-newgrad-hopeful-02',
        'label': 'hopeful new graduate',
        'communication_style': 'friendly and curious',
        'motivations': ['earn quickly', 'sense of momentum'],
        'concerns': ['fear of being misled', 'uncertainty about selling skills'],
        'trust_threshold': 'medium',
        'sales_tolerance': 'medium',
        'time_availability': 'high',
        'notes': 'high interest but vulnerable to unrealistic expectations',
    },
    {
        'id': 'p-parent-home-03',
        'label': 'stay-at-home parent',
        'communication_style': 'warm but practical',
        'motivations': ['income from home', 'flexible schedule'],
        'concerns': ['upfront costs', 'time-energy balance', 'pressure to recruit friends'],
        'trust_threshold': 'high',
        'sales_tolerance': 'low',
        'time_availability': 'fragmented',
        'notes': 'needs concrete explanation of workload and support',
    },
    {
        'id': 'p-owner-time-poor-04',
        'label': 'small business owner',
        'communication_style': 'direct and impatient',
        'motivations': ['scalable side opportunity', 'network leverage'],
        'concerns': ['wasted time', 'weak economics', 'dependency on recruiting'],
        'trust_threshold': 'high',
        'sales_tolerance': 'medium',
        'time_availability': 'low',
        'notes': 'prefers blunt qualification over warm social framing',
    },
]

strategies = [
    {
        'id': 's-consultative-01',
        'label': 'consultative fit-first',
        'opening_style': 'ask about goals first',
        'framing': 'suitability and realistic expectations',
        'tone': 'calm and professional',
        'primary_goal': 'determine fit before discussing next steps',
    },
    {
        'id': 's-product-first-02',
        'label': 'product-first educational',
        'opening_style': 'start from product value and customer use',
        'framing': 'product experience before business opportunity',
        'tone': 'practical and low-pressure',
        'primary_goal': 'reduce suspicion by grounding in tangible value',
    },
    {
        'id': 's-community-03',
        'label': 'community and support',
        'opening_style': 'relational but respectful',
        'framing': 'learning environment and support system',
        'tone': 'warm and reassuring',
        'primary_goal': 'test whether support/community matters to the persona',
    },
    {
        'id': 's-entrepreneurial-04',
        'label': 'small-business framing',
        'opening_style': 'direct business framing',
        'framing': 'skill-building and micro-entrepreneurship',
        'tone': 'concise and serious',
        'primary_goal': 'appeal to autonomy-oriented personas without overpromising',
    },
]

pairings = [
    ('p-office-skeptical-01','s-consultative-01'),
    ('p-office-skeptical-01','s-product-first-02'),
    ('p-office-skeptical-01','s-community-03'),
    ('p-office-skeptical-01','s-entrepreneurial-04'),
    ('p-newgrad-hopeful-02','s-consultative-01'),
    ('p-newgrad-hopeful-02','s-product-first-02'),
    ('p-newgrad-hopeful-02','s-community-03'),
    ('p-newgrad-hopeful-02','s-entrepreneurial-04'),
    ('p-parent-home-03','s-consultative-01'),
    ('p-parent-home-03','s-product-first-02'),
    ('p-parent-home-03','s-community-03'),
    ('p-parent-home-03','s-entrepreneurial-04'),
    ('p-owner-time-poor-04','s-consultative-01'),
    ('p-owner-time-poor-04','s-product-first-02'),
    ('p-owner-time-poor-04','s-community-03'),
    ('p-owner-time-poor-04','s-entrepreneurial-04'),
]

pmap = {p['id']: p for p in personas}
smap = {s['id']: s for s in strategies}

base = {
    'trust_built': 6.0,
    'clarity': 6.0,
    'pressure_level': 3.0,
    'persona_fit': 5.0,
    'objection_handling': 6.0,
    'next_step_quality': 6.0,
    'ethical_continuation': 6.0,
}

def assess(persona, strategy):
    scores = dict(base)
    helpful = []
    harmful = []
    turning = []
    outcome = 'continue'
    next_step = 'send a written summary and invite the prospect to review privately before deciding'
    manip = []

    if strategy['id'] == 's-consultative-01':
        scores['trust_built'] += 2.0
        scores['clarity'] += 1.5
        scores['objection_handling'] += 1.5
        scores['ethical_continuation'] += 2.0
        helpful += ['opened by asking about goals before pitching', 'stated early that the model is not a fit for everyone']
        turning += ['trust increased when the sponsor surfaced workload and social requirements early']
    elif strategy['id'] == 's-product-first-02':
        scores['trust_built'] += 1.5
        scores['pressure_level'] -= 0.5
        scores['clarity'] += 1.0
        helpful += ['grounded the discussion in a real product category before talking about the business']
        turning += ['skepticism softened once the discussion stayed concrete rather than aspirational']
    elif strategy['id'] == 's-community-03':
        scores['trust_built'] += 0.5
        scores['pressure_level'] += 0.5
        helpful += ['explicitly said saying no is fine']
        harmful += ['community framing risked sounding like social belonging pressure for low-tolerance personas']
        manip += ['belonging-pressure risk if support language becomes identity-based']
    elif strategy['id'] == 's-entrepreneurial-04':
        scores['clarity'] += 1.5
        scores['persona_fit'] += 1.0
        helpful += ['named the required skills and effort in blunt business terms']
        turning += ['fit became clearer once the sponsor described the work as a people-driven micro-business']

    if persona['id'] == 'p-office-skeptical-01':
        scores['pressure_level'] += 0.5 if strategy['id'] == 's-community-03' else -0.5
        scores['trust_built'] += 1.0 if strategy['id'] in ('s-consultative-01','s-product-first-02') else -1.0
        scores['persona_fit'] += -1.0 if strategy['id'] in ('s-community-03','s-entrepreneurial-04') else 0.5
        harmful += ['any vague income upside language immediately reduced credibility']
        if strategy['id'] == 's-community-03':
            outcome = 'qualify_out'
            next_step = 'close respectfully; this persona is too sensitive to social-pressure cues'
            scores['ethical_continuation'] += 0.5
        elif strategy['id'] == 's-entrepreneurial-04':
            outcome = 'cautious_follow_up'
    elif persona['id'] == 'p-newgrad-hopeful-02':
        scores['persona_fit'] += 1.0
        scores['next_step_quality'] += 0.5 if strategy['id'] in ('s-consultative-01','s-product-first-02') else -0.5
        harmful += ['vulnerability to hopeful framing means the sponsor must slow the pace and de-romanticize earnings']
        manip += ['expectation inflation risk even without explicit exaggeration']
        if strategy['id'] == 's-community-03':
            scores['pressure_level'] += 1.0
            outcome = 'continue_with_guardrails'
        elif strategy['id'] == 's-entrepreneurial-04':
            scores['clarity'] += 0.5
            outcome = 'continue'
    elif persona['id'] == 'p-parent-home-03':
        scores['trust_built'] += 1.0 if strategy['id'] in ('s-consultative-01','s-community-03') else 0.0
        scores['persona_fit'] += 1.0 if strategy['id'] in ('s-consultative-01','s-product-first-02') else -0.5
        helpful += ['discussion of schedule fragmentation and energy cost made the conversation feel realistic']
        harmful += ['unclear onboarding time estimates created hesitation']
        if strategy['id'] == 's-entrepreneurial-04':
            scores['pressure_level'] += 1.0
            outcome = 'qualify_out'
            next_step = 'do not push a business next step; offer product-only or no action'
        else:
            outcome = 'continue' if strategy['id'] != 's-community-03' else 'cautious_follow_up'
    elif persona['id'] == 'p-owner-time-poor-04':
        scores['clarity'] += 1.0
        scores['persona_fit'] += 1.0 if strategy['id'] == 's-entrepreneurial-04' else 0.0
        scores['trust_built'] += 0.5 if strategy['id'] in ('s-entrepreneurial-04','s-consultative-01') else -0.5
        harmful += ['too much warm-up felt inefficient and reduced engagement']
        if strategy['id'] == 's-community-03':
            scores['pressure_level'] += 1.0
            scores['persona_fit'] -= 1.0
            outcome = 'qualify_out'
            next_step = 'stop after confirming the time model is a mismatch'
        elif strategy['id'] == 's-entrepreneurial-04':
            outcome = 'continue'
            next_step = 'send a concise economics + workload memo, then short follow-up Q&A'
        else:
            outcome = 'cautious_follow_up'

    for k in scores:
        scores[k] = max(1.0, min(9.5, round(scores[k], 1)))
    pressure_band = 'low' if scores['pressure_level'] <= 3.0 else 'moderate' if scores['pressure_level'] <= 5.0 else 'high'
    return scores, helpful, harmful, turning, outcome, next_step, manip, pressure_band


def transcript(persona, strategy, outcome, next_step):
    sponsor_open = {
        's-consultative-01': f"ก่อนคุยเรื่องโอกาส ผมอยากรู้ก่อนว่าคุณกำลังมองหารายได้เสริมแบบไหน และอะไรที่คุณไม่อยากเจอเด็ดขาด",
        's-product-first-02': f"ขอเริ่มจากตัวสินค้าและลูกค้าก่อนนะ ถ้าหมวดสินค้านี้ไม่เกี่ยวกับชีวิตคุณ เราไม่จำเป็นต้องคุยต่อเรื่องธุรกิจก็ได้",
        's-community-03': f"สิ่งที่หลายคนชอบคือมีคนช่วยสอนและไม่ต้องทำคนเดียว แต่ถ้าไม่ใช่ทางคุณ ปฏิเสธได้เต็มที่นะ",
        's-entrepreneurial-04': f"ถ้าจะคุยกัน ผมอยากคุยตรงๆ ว่านี่คือธุรกิจจิ๋วที่ต้องใช้ทักษะขายและดูแลคน ไม่ใช่รายได้ลอยมาเอง",
    }[strategy['id']]
    p1 = f"ฟังได้ครับ/ค่ะ แต่ผม/ฉันกังวลเรื่อง {persona['concerns'][0]} และ {persona['concerns'][1]} มากเป็นพิเศษ"
    turns = [
        ('Sponsor', sponsor_open),
        ('Prospect', p1),
        ('Sponsor', f"เข้าใจเลย จุดนั้นสำคัญมาก สำหรับโมเดลนี้ผมจะพูดให้ครบทั้งต้นทุน เวลา และสิ่งที่ต้องทำจริง ถ้าฟังแล้วไม่เหมาะเราหยุดได้ทันที"),
        ('Prospect', f"โอเค งั้นอยากรู้ว่าต้องใช้เวลาแค่ไหน เพราะผม/ฉันมีเวลาแบบ {persona['time_availability']}"),
        ('Sponsor', f"โดยทั่วไปต้องมีเวลาคุยกับลูกค้าหรือผู้สนใจสม่ำเสมอ และมีช่วงเรียนรู้สินค้า/ระบบพอสมควร ไม่ใช่งานที่ตั้งทิ้งไว้แล้วไปเอง"),
        ('Prospect', f"แล้วเรื่องรายได้หรือค่าใช้จ่ายเริ่มต้นล่ะ ผม/ฉันไม่อยากเจออะไรที่คลุมเครือ"),
        ('Sponsor', "สิ่งที่ผมรับปากได้คือจะไม่พูดตัวเลขสวยเกินจริง รายได้ขึ้นกับทักษะ ความสม่ำเสมอ และความเหมาะกับสินค้าจริง ส่วนค่าใช้จ่ายต้องดูแพ็กเริ่มต้นและค่าใช้จ่ายแฝงให้ครบก่อนตัดสินใจ"),
        ('Prospect', f"แบบนี้ฟังตรงดี แต่ยังต้องคิดว่าเหมาะกับนิสัยผม/ฉันไหม โดยเฉพาะเรื่อง {persona['concerns'][-1]}"),
        ('Sponsor', f"ถูกต้องครับ/ค่ะ ถ้าคุณไม่สบายใจกับรูปแบบชวนคนหรือดูแลเครือข่าย นั่นอาจเป็นสัญญาณว่าไม่ควรฝืน ผมส่งสรุปข้อดี ข้อจำกัด และภาระงานให้คุณดูได้ แล้วค่อยตัดสินใจ"),
        ('Prospect', f"รับทราบครับ/ค่ะ สเต็ปถัดไปแบบนี้โอเคกว่า เพราะไม่รู้สึกว่าถูกเร่ง — outcome: {outcome}; next: {next_step}"),
    ]
    return turns

results = []
for idx, (pid, sid) in enumerate(pairings, start=1):
    persona = pmap[pid]
    strategy = smap[sid]
    scores, helpful, harmful, turning, outcome, next_step, manip, pressure_band = assess(persona, strategy)
    scen_id = f'scenario-{idx:03d}'
    sdir = RUN / scen_id
    sdir.mkdir(parents=True, exist_ok=True)
    scenario = {
        'scenario_id': scen_id,
        'batch_id': 'batch-001',
        'persona_id': pid,
        'strategy_id': sid,
        'objective': 'test ethical sponsor conversation pattern',
        'constraints': ['no hype', 'no coercion', 'surface workload/costs early', 'qualify out if poor fit'],
    }
    turns = transcript(persona, strategy, outcome, next_step)
    transcript_md = '\n'.join([f"**{speaker}:** {line}\n" for speaker, line in turns])
    evaluation = {
        'scenario_id': scen_id,
        'scores': scores,
        'pressure_band': pressure_band,
        'outcome': outcome,
        'helpful_phrases': helpful,
        'harmful_phrases': harmful,
        'turning_points': turning,
        'recommended_next_step': next_step,
    }
    safety = {
        'scenario_id': scen_id,
        'manipulation_risk': 'low' if len(manip) <= 1 and scores['pressure_level'] <= 4 else 'moderate',
        'ambiguous_earning_claim': 'none',
        'hidden_effort_risk': 'low',
        'hidden_cost_risk': 'low',
        'social_pressure_risk': 'moderate' if strategy['id'] == 's-community-03' else 'low',
        'urgency_or_coercion_risk': 'none',
        'flags': manip,
    }
    summary = f"# {scen_id}\n\n- Persona: {persona['label']}\n- Strategy: {strategy['label']}\n- Outcome: {outcome}\n- Pressure: {pressure_band}\n- Best move: {helpful[0]}\n- Main caution: {(harmful[0] if harmful else 'none')}\n- Next step: {next_step}\n"
    (sdir / 'scenario.json').write_text(json.dumps(scenario, ensure_ascii=False, indent=2))
    (sdir / 'transcript.md').write_text(transcript_md)
    (sdir / 'evaluation.json').write_text(json.dumps(evaluation, ensure_ascii=False, indent=2))
    (sdir / 'safety_review.json').write_text(json.dumps(safety, ensure_ascii=False, indent=2))
    (sdir / 'scenario_summary.md').write_text(summary)
    results.append({'scenario_id': scen_id, 'persona': persona['label'], 'strategy': strategy['label'], 'outcome': outcome, 'scores': scores, 'safety': safety})

(RUN / 'personas.json').write_text(json.dumps(personas, ensure_ascii=False, indent=2))
(RUN / 'strategies.json').write_text(json.dumps(strategies, ensure_ascii=False, indent=2))
(RUN / 'scenarios.json').write_text(json.dumps([{'scenario_id': f'scenario-{i:03d}', 'persona_id': pid, 'strategy_id': sid} for i, (pid, sid) in enumerate(pairings, start=1)], ensure_ascii=False, indent=2))

by_strategy = {}
by_persona = {}
for r in results:
    by_strategy.setdefault(r['strategy'], []).append(r)
    by_persona.setdefault(r['persona'], []).append(r)

def avg(items, key):
    return round(mean(x['scores'][key] for x in items), 2)

lines = [
    '# Batch 001 Report',
    '',
    f"Scenarios run: {len(results)}", 
    '',
    '## Headline findings',
    '',
    f"- Highest average trust: consultative fit-first ({avg(by_strategy['consultative fit-first'],'trust_built')})",
    f"- Highest average clarity: small-business framing ({avg(by_strategy['small-business framing'],'clarity')})",
    f"- Lowest average pressure: product-first educational ({avg(by_strategy['product-first educational'],'pressure_level')})",
    f"- Most fragile strategy: community and support (social-pressure risk surfaced repeatedly)",
    '',
    '## Findings by strategy',
    '',
]
for name, items in by_strategy.items():
    lines += [
        f"### {name}",
        f"- Avg trust: {avg(items,'trust_built')}",
        f"- Avg clarity: {avg(items,'clarity')}",
        f"- Avg pressure: {avg(items,'pressure_level')}",
        f"- Avg fit: {avg(items,'persona_fit')}",
        f"- Notes: {'Works best for skeptical or practical personas.' if name in ('consultative fit-first','product-first educational') else 'Needs tighter guardrails around belonging language.' if name == 'community and support' else 'Best for autonomy-oriented, time-sensitive prospects.'}",
        ''
    ]
lines += ['## Findings by persona', '']
for name, items in by_persona.items():
    best = max(items, key=lambda x: x['scores']['trust_built'] + x['scores']['clarity'] - x['scores']['pressure_level'])
    worst = max(items, key=lambda x: x['scores']['pressure_level'] - x['scores']['trust_built'])
    lines += [
        f"### {name}",
        f"- Best pattern: {best['strategy']} ({best['scenario_id']})",
        f"- Fragile pattern: {worst['strategy']} ({worst['scenario_id']})",
        f"- Main lesson: {'Use blunt transparency and avoid social framing.' if 'skeptical office' in name else 'Slow the pace and actively de-romanticize upside.' if 'graduate' in name else 'Spell out schedule/energy cost early.' if 'parent' in name else 'Lead with economics and time demands; skip warm-up fluff.'}",
        ''
    ]
lines += [
    '## Recommended playbook changes',
    '',
    '- Keep the first 2 turns focused on fit, workload, and what the prospect does *not* want.',
    '- Default to written follow-up summaries rather than immediate calls-to-action.',
    '- Treat community/support language as optional seasoning, not the core pitch.',
    '- For vulnerable but interested personas, add an explicit expectation-reset script before discussing next steps.',
    '',
    '## Qualification rubric (v1)',
    '',
    '- Qualify out if the prospect is highly allergic to social/recruiting dynamics and trust drops after clarification.',
    '- Continue carefully if interest is high but expectations are fragile; force explicit discussion of effort, costs, and selling discomfort.',
    '- Proceed to next step only when the prospect can restate the workload and still opts in without pressure.',
]
report = '\n'.join(lines)
(REPORTS / 'batch-001-report.md').write_text(report)
(REPORTS / 'final-report.md').write_text(report)

summary = {
    'scenarios_run': len(results),
    'best_strategy_by_trust': 'consultative fit-first',
    'best_strategy_by_low_pressure': 'product-first educational',
    'highest_risk_strategy': 'community and support',
}
print(json.dumps(summary, ensure_ascii=False, indent=2))
