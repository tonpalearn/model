from pathlib import Path
import json
from statistics import mean

# =========================================================
# Batch 001 - Ethical Sponsor Simulation
# เวอร์ชันภาษาไทย: ทั้งโค้ด คอมเมนต์ และรายงาน
# =========================================================

ROOT = Path(__file__).resolve().parents[1]
RUN_DIR = ROOT / 'runs' / 'batch-001'
REPORTS_DIR = ROOT / 'reports'

RUN_DIR.mkdir(parents=True, exist_ok=True)
REPORTS_DIR.mkdir(parents=True, exist_ok=True)

# ---------------------------------------------------------------------
# ข้อมูลตั้งต้น
# ---------------------------------------------------------------------

personas = [
    {
        'id': 'p-office-skeptical-01',
        'label_th': 'พนักงานออฟฟิศสายระวังตัว',
        'label_en': 'skeptical office worker',
        'communication_style': 'สุภาพแต่ระแวง',
        'motivations': ['อยากมีรายได้เสริม', 'อยากมีความยืดหยุ่นมากขึ้น'],
        'concerns': ['ภาพลักษณ์/ชื่อเสียง', 'ต้นทุนแฝง', 'แรงกดดันจาก upline'],
        'trust_threshold': 'สูง',
        'sales_tolerance': 'ต่ำ',
        'time_availability': 'ว่างเฉพาะตอนเย็น',
        'notes': 'ไม่ชอบคำพูดลอยๆ และไม่ชอบแรงกดดันทางสังคม',
    },
    {
        'id': 'p-newgrad-hopeful-02',
        'label_th': 'เด็กจบใหม่ที่มีความหวังสูง',
        'label_en': 'hopeful new graduate',
        'communication_style': 'เป็นมิตรและอยากรู้',
        'motivations': ['อยากหาเงินได้เร็ว', 'อยากรู้สึกว่าชีวิตเดินหน้า'],
        'concerns': ['กลัวถูกหลอก', 'ไม่มั่นใจทักษะการขาย'],
        'trust_threshold': 'กลาง',
        'sales_tolerance': 'กลาง',
        'time_availability': 'ค่อนข้างสูง',
        'notes': 'สนใจง่าย แต่เสี่ยงตั้งความหวังเกินจริง',
    },
    {
        'id': 'p-parent-home-03',
        'label_th': 'คุณพ่อ/คุณแม่ที่ดูแลบ้าน',
        'label_en': 'stay-at-home parent',
        'communication_style': 'อบอุ่นแต่ปฏิบัติจริง',
        'motivations': ['อยากมีรายได้จากบ้าน', 'อยากได้เวลาที่ยืดหยุ่น'],
        'concerns': ['ค่าเริ่มต้น', 'เวลาและพลังงานไม่พอ', 'ไม่อยากกดดันเพื่อนหรือคนรู้จัก'],
        'trust_threshold': 'สูง',
        'sales_tolerance': 'ต่ำ',
        'time_availability': 'เวลาแตกเป็นช่วงๆ',
        'notes': 'ต้องการคำอธิบายชัดเจนเรื่องภาระงานและการซัพพอร์ต',
    },
    {
        'id': 'p-owner-time-poor-04',
        'label_th': 'เจ้าของกิจการที่เวลาน้อย',
        'label_en': 'small business owner',
        'communication_style': 'ตรงและใจร้อน',
        'motivations': ['อยากได้ side opportunity ที่ต่อยอดได้', 'อยากใช้ leverage จาก network'],
        'concerns': ['เสียเวลา', 'เศรษฐศาสตร์ไม่คุ้ม', 'พึ่งการ recruit มากเกินไป'],
        'trust_threshold': 'สูง',
        'sales_tolerance': 'กลาง',
        'time_availability': 'ต่ำ',
        'notes': 'ชอบการคัดกรองแบบตรงไปตรงมา มากกว่าการชวนแบบอบอุ่น',
    },
]

strategies = [
    {
        'id': 's-consultative-01',
        'label_th': 'แนวปรึกษา เน้นดูความเหมาะสมก่อน',
        'label_en': 'consultative fit-first',
        'opening_style': 'เริ่มจากถามเป้าหมายก่อน',
        'framing': 'ความเหมาะสมและความคาดหวังที่สมจริง',
        'tone': 'นิ่ง สุภาพ เป็นมืออาชีพ',
        'primary_goal': 'เช็ก fit ก่อนคุย next step',
    },
    {
        'id': 's-product-first-02',
        'label_th': 'เริ่มจากสินค้าแบบให้ความรู้',
        'label_en': 'product-first educational',
        'opening_style': 'เริ่มจากคุณค่าของสินค้าและการใช้งานจริง',
        'framing': 'คุย product ก่อน business opportunity',
        'tone': 'ปฏิบัติจริง แรงกดดันต่ำ',
        'primary_goal': 'ลดความระแวงด้วยการคุยเรื่องที่จับต้องได้ก่อน',
    },
    {
        'id': 's-community-03',
        'label_th': 'เน้นชุมชนและการซัพพอร์ต',
        'label_en': 'community and support',
        'opening_style': 'สัมพันธ์ดี แต่ยังเคารพพื้นที่ส่วนตัว',
        'framing': 'สภาพแวดล้อมการเรียนรู้และระบบช่วยเหลือ',
        'tone': 'อบอุ่นและให้กำลังใจ',
        'primary_goal': 'ทดสอบว่าคนนี้ตอบรับกับมิติ support/community หรือไม่',
    },
    {
        'id': 's-entrepreneurial-04',
        'label_th': 'กรอบคิดแบบธุรกิจย่อย',
        'label_en': 'small-business framing',
        'opening_style': 'เปิดแบบธุรกิจตรงๆ',
        'framing': 'การสร้างทักษะและการเป็นผู้ประกอบการขนาดเล็ก',
        'tone': 'กระชับ จริงจัง',
        'primary_goal': 'เหมาะกับคนที่ชอบ autonomy โดยไม่ขายฝัน',
    },
]

pairings = [
    ('p-office-skeptical-01', 's-consultative-01'),
    ('p-office-skeptical-01', 's-product-first-02'),
    ('p-office-skeptical-01', 's-community-03'),
    ('p-office-skeptical-01', 's-entrepreneurial-04'),
    ('p-newgrad-hopeful-02', 's-consultative-01'),
    ('p-newgrad-hopeful-02', 's-product-first-02'),
    ('p-newgrad-hopeful-02', 's-community-03'),
    ('p-newgrad-hopeful-02', 's-entrepreneurial-04'),
    ('p-parent-home-03', 's-consultative-01'),
    ('p-parent-home-03', 's-product-first-02'),
    ('p-parent-home-03', 's-community-03'),
    ('p-parent-home-03', 's-entrepreneurial-04'),
    ('p-owner-time-poor-04', 's-consultative-01'),
    ('p-owner-time-poor-04', 's-product-first-02'),
    ('p-owner-time-poor-04', 's-community-03'),
    ('p-owner-time-poor-04', 's-entrepreneurial-04'),
]

persona_map = {item['id']: item for item in personas}
strategy_map = {item['id']: item for item in strategies}

base_scores = {
    'trust_built': 6.0,
    'clarity': 6.0,
    'pressure_level': 3.0,
    'persona_fit': 5.0,
    'objection_handling': 6.0,
    'next_step_quality': 6.0,
    'ethical_continuation': 6.0,
}

# ---------------------------------------------------------------------
# ตัวช่วย
# ---------------------------------------------------------------------

def clamp_score(value):
    return max(1.0, min(9.5, round(value, 1)))


def pressure_band(score):
    if score <= 3.0:
        return 'ต่ำ'
    if score <= 5.0:
        return 'กลาง'
    return 'สูง'


def outcome_th(code):
    mapping = {
        'continue': 'ไปต่อได้',
        'cautious_follow_up': 'ติดตามต่อได้แบบระวัง',
        'continue_with_guardrails': 'ไปต่อได้ แต่ต้องมี guardrails',
        'qualify_out': 'ควรคัดออกอย่างสุภาพ',
    }
    return mapping.get(code, code)


def assess(persona, strategy):
    scores = dict(base_scores)
    helpful_points = []
    caution_points = []
    turning_points = []
    risk_flags = []
    outcome = 'continue'
    next_step = 'ส่งสรุปเป็นลายลักษณ์อักษรให้ทบทวนเองก่อน แล้วค่อยตัดสินใจ'

    # ปรับตาม strategy
    if strategy['id'] == 's-consultative-01':
        scores['trust_built'] += 2.0
        scores['clarity'] += 1.5
        scores['objection_handling'] += 1.5
        scores['ethical_continuation'] += 2.0
        helpful_points += [
            'เปิดบทสนทนาด้วยการถามเป้าหมายก่อน ไม่รีบ pitch',
            'บอกชัดตั้งแต่ต้นว่าโอกาสนี้ไม่เหมาะกับทุกคน',
        ]
        turning_points += [
            'ความไว้ใจเพิ่มขึ้นเมื่อ sponsor พูดเรื่องภาระงานและความต้องใช้คนให้ฟังเร็ว',
        ]

    elif strategy['id'] == 's-product-first-02':
        scores['trust_built'] += 1.5
        scores['pressure_level'] -= 0.5
        scores['clarity'] += 1.0
        helpful_points += [
            'คุยเรื่องสินค้าที่จับต้องได้ก่อน ทำให้บทสนทนาดูจริงและไม่ลอย',
        ]
        turning_points += [
            'ความระแวงลดลงเมื่อบทสนทนาอยู่กับของจริง มากกว่าฝันหรือภาพอนาคต',
        ]

    elif strategy['id'] == 's-community-03':
        scores['trust_built'] += 0.5
        scores['pressure_level'] += 0.5
        helpful_points += [
            'มีการย้ำว่าปฏิเสธได้ ทำให้ไม่ดูบีบเกินไป',
        ]
        caution_points += [
            'ภาษาที่เน้น community เสี่ยงถูกตีความเป็นแรงกดดันเชิงการเป็นส่วนหนึ่งของกลุ่ม',
        ]
        risk_flags += [
            'มีความเสี่ยงเรื่อง belonging pressure ถ้าใช้ภาษาผูกกับตัวตนหรือความเป็นพวกเดียวกัน',
        ]

    elif strategy['id'] == 's-entrepreneurial-04':
        scores['clarity'] += 1.5
        scores['persona_fit'] += 1.0
        helpful_points += [
            'พูดตรงเรื่องทักษะและ effort ที่ต้องใช้ ทำให้ภาพชัด',
        ]
        turning_points += [
            'ความเหมาะสมเห็นชัดขึ้นเมื่อ framing เป็นธุรกิจย่อยที่ต้องลงแรงจริง',
        ]

    # ปรับตาม persona x strategy
    if persona['id'] == 'p-office-skeptical-01':
        scores['pressure_level'] += 0.5 if strategy['id'] == 's-community-03' else -0.5
        scores['trust_built'] += 1.0 if strategy['id'] in ('s-consultative-01', 's-product-first-02') else -1.0
        scores['persona_fit'] += -1.0 if strategy['id'] in ('s-community-03', 's-entrepreneurial-04') else 0.5
        caution_points += [
            'ถ้ามีคำพูดแนวรายได้สวยๆ แบบคลุมเครือ ความน่าเชื่อถือจะตกทันที',
        ]
        if strategy['id'] == 's-community-03':
            outcome = 'qualify_out'
            next_step = 'ปิดบทสนทนาอย่างสุภาพ เพราะ persona นี้แพ้แรงกดดันเชิงสังคมชัดเจน'
            scores['ethical_continuation'] += 0.5
        elif strategy['id'] == 's-entrepreneurial-04':
            outcome = 'cautious_follow_up'

    elif persona['id'] == 'p-newgrad-hopeful-02':
        scores['persona_fit'] += 1.0
        scores['next_step_quality'] += 0.5 if strategy['id'] in ('s-consultative-01', 's-product-first-02') else -0.5
        caution_points += [
            'persona นี้มีความหวังสูง จึงต้องชะลอจังหวะและลดภาพฝันเรื่องรายได้เสมอ',
        ]
        risk_flags += [
            'มีความเสี่ยง expectation inflation แม้ไม่ได้ exaggerate ตรงๆ',
        ]
        if strategy['id'] == 's-community-03':
            scores['pressure_level'] += 1.0
            outcome = 'continue_with_guardrails'
        elif strategy['id'] == 's-entrepreneurial-04':
            scores['clarity'] += 0.5
            outcome = 'continue'

    elif persona['id'] == 'p-parent-home-03':
        scores['trust_built'] += 1.0 if strategy['id'] in ('s-consultative-01', 's-community-03') else 0.0
        scores['persona_fit'] += 1.0 if strategy['id'] in ('s-consultative-01', 's-product-first-02') else -0.5
        helpful_points += [
            'การพูดเรื่องเวลาแบบเป็นช่วงๆ และพลังงานที่ต้องใช้ ทำให้บทสนทนาดูสมจริง',
        ]
        caution_points += [
            'ถ้าอธิบายเวลา onboarding ไม่ชัด จะทำให้ลังเลทันที',
        ]
        if strategy['id'] == 's-entrepreneurial-04':
            scores['pressure_level'] += 1.0
            outcome = 'qualify_out'
            next_step = 'ไม่ควรดันไปต่อทางธุรกิจ อาจเสนอแค่ product-only หรือจบอย่างสุภาพ'
        else:
            outcome = 'continue' if strategy['id'] != 's-community-03' else 'cautious_follow_up'

    elif persona['id'] == 'p-owner-time-poor-04':
        scores['clarity'] += 1.0
        scores['persona_fit'] += 1.0 if strategy['id'] == 's-entrepreneurial-04' else 0.0
        scores['trust_built'] += 0.5 if strategy['id'] in ('s-entrepreneurial-04', 's-consultative-01') else -0.5
        caution_points += [
            'ถ้า warm-up เยอะเกินไป จะถูกมองว่าเสียเวลาและ engagement จะลดลง',
        ]
        if strategy['id'] == 's-community-03':
            scores['pressure_level'] += 1.0
            scores['persona_fit'] -= 1.0
            outcome = 'qualify_out'
            next_step = 'หยุดเมื่อเห็นชัดว่า time model ไม่ตรงกัน'
        elif strategy['id'] == 's-entrepreneurial-04':
            outcome = 'continue'
            next_step = 'ส่ง memo สั้นๆ เรื่อง unit economics + workload แล้วนัด Q&A แบบสั้น'
        else:
            outcome = 'cautious_follow_up'

    for key, value in scores.items():
        scores[key] = clamp_score(value)

    return {
        'scores': scores,
        'pressure_band': pressure_band(scores['pressure_level']),
        'helpful_points': helpful_points,
        'caution_points': caution_points,
        'turning_points': turning_points,
        'risk_flags': risk_flags,
        'outcome_code': outcome,
        'outcome_th': outcome_th(outcome),
        'next_step': next_step,
    }


def build_transcript(persona, strategy, outcome_text, next_step):
    sponsor_open = {
        's-consultative-01': 'ก่อนคุยเรื่องโอกาส ผมอยากรู้ก่อนว่าคุณกำลังมองหารายได้เสริมแบบไหน และอะไรที่คุณไม่อยากเจอเด็ดขาด',
        's-product-first-02': 'ขอเริ่มจากตัวสินค้าและการใช้งานจริงก่อนนะ ถ้าหมวดนี้ไม่เกี่ยวกับชีวิตคุณ เราไม่จำเป็นต้องคุยต่อเรื่องธุรกิจก็ได้',
        's-community-03': 'หลายคนชอบตรงมีคนช่วยสอนและไม่ต้องทำคนเดียว แต่ถ้าไม่ใช่ทางคุณ ปฏิเสธได้เต็มที่นะ',
        's-entrepreneurial-04': 'ถ้าจะคุยกัน ผมขอคุยตรงๆ ว่านี่คือธุรกิจย่อยที่ต้องใช้ทักษะขายและดูแลคน ไม่ใช่รายได้ลอยมาเอง',
    }[strategy['id']]

    turns = [
        ('Sponsor', sponsor_open),
        ('Prospect', f"ฟังได้ครับ/ค่ะ แต่ผม/ฉันกังวลเรื่อง {persona['concerns'][0]} และ {persona['concerns'][1]} มากเป็นพิเศษ"),
        ('Sponsor', 'เข้าใจเลย จุดนั้นสำคัญมาก สำหรับโมเดลนี้ผมจะพูดให้ครบทั้งต้นทุน เวลา และสิ่งที่ต้องทำจริง ถ้าฟังแล้วไม่เหมาะเราหยุดได้ทันที'),
        ('Prospect', f"โอเค งั้นอยากรู้ว่าต้องใช้เวลาแค่ไหน เพราะผม/ฉันมีเวลาแบบ {persona['time_availability']}"),
        ('Sponsor', 'โดยทั่วไปต้องมีเวลาคุยกับลูกค้าหรือผู้สนใจอย่างสม่ำเสมอ และต้องมีช่วงเรียนรู้สินค้า/ระบบพอสมควร ไม่ใช่งานที่ตั้งทิ้งไว้แล้วไปเอง'),
        ('Prospect', 'แล้วเรื่องรายได้หรือค่าใช้จ่ายเริ่มต้นล่ะ ผม/ฉันไม่อยากเจออะไรที่คลุมเครือ'),
        ('Sponsor', 'สิ่งที่ผมรับปากได้คือจะไม่พูดตัวเลขสวยเกินจริง รายได้ขึ้นกับทักษะ ความสม่ำเสมอ และความเหมาะกับสินค้าจริง ส่วนค่าใช้จ่ายต้องดูทั้งค่าเริ่มต้นและค่าแฝงให้ครบก่อนตัดสินใจ'),
        ('Prospect', f"แบบนี้ฟังตรงดี แต่ยังต้องคิดว่าเหมาะกับนิสัยผม/ฉันไหม โดยเฉพาะเรื่อง {persona['concerns'][-1]}"),
        ('Sponsor', 'ถูกต้องครับ/ค่ะ ถ้าคุณไม่สบายใจกับการชวนคนหรือการดูแลเครือข่าย นั่นอาจเป็นสัญญาณว่าไม่ควรฝืน ผมส่งสรุปข้อดี ข้อจำกัด และภาระงานให้คุณดูได้ แล้วค่อยตัดสินใจ'),
        ('Prospect', f"รับทราบครับ/ค่ะ แบบนี้รู้สึกโอเคกว่า เพราะไม่รู้สึกว่าถูกเร่ง — ผลสรุป: {outcome_text}; ขั้นถัดไป: {next_step}"),
    ]
    return turns


def write_json(path, payload):
    path.write_text(json.dumps(payload, ensure_ascii=False, indent=2))


# ---------------------------------------------------------------------
# รัน batch
# ---------------------------------------------------------------------

results = []

for index, (persona_id, strategy_id) in enumerate(pairings, start=1):
    persona = persona_map[persona_id]
    strategy = strategy_map[strategy_id]
    assessment = assess(persona, strategy)

    scenario_id = f'scenario-{index:03d}'
    scenario_dir = RUN_DIR / scenario_id
    scenario_dir.mkdir(parents=True, exist_ok=True)

    scenario = {
        'scenario_id': scenario_id,
        'batch_id': 'batch-001',
        'persona_id': persona_id,
        'persona_label': persona['label_th'],
        'strategy_id': strategy_id,
        'strategy_label': strategy['label_th'],
        'objective': 'ทดสอบรูปแบบการชวนแบบ ethical ว่าแบบไหนชัดเจน เชื่อถือได้ และคัดกรองคนได้ดี',
        'constraints': [
            'ห้าม hype',
            'ห้าม coercion',
            'ต้องเปิดเรื่อง workload / cost เร็ว',
            'ถ้าไม่ fit ต้อง qualify out ได้',
        ],
    }

    turns = build_transcript(persona, strategy, assessment['outcome_th'], assessment['next_step'])
    transcript_md = '\n'.join([f"**{speaker}:** {line}\n" for speaker, line in turns])

    evaluation = {
        'scenario_id': scenario_id,
        'pressure_band': assessment['pressure_band'],
        'outcome': assessment['outcome_th'],
        'recommended_next_step': assessment['next_step'],
        'scores': assessment['scores'],
        'helpful_points': assessment['helpful_points'],
        'caution_points': assessment['caution_points'],
        'turning_points': assessment['turning_points'],
    }

    safety_review = {
        'scenario_id': scenario_id,
        'manipulation_risk': 'ต่ำ' if len(assessment['risk_flags']) <= 1 and assessment['scores']['pressure_level'] <= 4 else 'กลาง',
        'ambiguous_earning_claim': 'ไม่พบ',
        'hidden_effort_risk': 'ต่ำ',
        'hidden_cost_risk': 'ต่ำ',
        'social_pressure_risk': 'กลาง' if strategy_id == 's-community-03' else 'ต่ำ',
        'urgency_or_coercion_risk': 'ไม่พบ',
        'flags': assessment['risk_flags'],
    }

    summary_md = '\n'.join([
        f"# {scenario_id}",
        '',
        f"- Persona: {persona['label_th']}",
        f"- Strategy: {strategy['label_th']}",
        f"- Outcome: {assessment['outcome_th']}",
        f"- Pressure: {assessment['pressure_band']}",
        f"- จุดเด่น: {assessment['helpful_points'][0] if assessment['helpful_points'] else '-'}",
        f"- ข้อควรระวัง: {assessment['caution_points'][0] if assessment['caution_points'] else '-'}",
        f"- Next step: {assessment['next_step']}",
        '',
    ])

    write_json(scenario_dir / 'scenario.json', scenario)
    (scenario_dir / 'transcript.md').write_text(transcript_md)
    write_json(scenario_dir / 'evaluation.json', evaluation)
    write_json(scenario_dir / 'safety_review.json', safety_review)
    (scenario_dir / 'scenario_summary.md').write_text(summary_md)

    results.append({
        'scenario_id': scenario_id,
        'persona': persona['label_th'],
        'strategy': strategy['label_th'],
        'scores': assessment['scores'],
        'outcome': assessment['outcome_th'],
        'pressure_band': assessment['pressure_band'],
        'safety': safety_review,
    })

write_json(RUN_DIR / 'personas.json', personas)
write_json(RUN_DIR / 'strategies.json', strategies)
write_json(
    RUN_DIR / 'scenarios.json',
    [
        {
            'scenario_id': f'scenario-{i:03d}',
            'persona_id': persona_id,
            'strategy_id': strategy_id,
        }
        for i, (persona_id, strategy_id) in enumerate(pairings, start=1)
    ]
)

# ---------------------------------------------------------------------
# สรุปรายงาน
# ---------------------------------------------------------------------

by_strategy = {}
by_persona = {}
for item in results:
    by_strategy.setdefault(item['strategy'], []).append(item)
    by_persona.setdefault(item['persona'], []).append(item)


def avg_score(items, key):
    return round(mean(entry['scores'][key] for entry in items), 2)

headline_trust = max(by_strategy.items(), key=lambda kv: avg_score(kv[1], 'trust_built'))
headline_clarity = max(by_strategy.items(), key=lambda kv: avg_score(kv[1], 'clarity'))
headline_low_pressure = min(by_strategy.items(), key=lambda kv: avg_score(kv[1], 'pressure_level'))

report_lines = [
    '# รายงาน Batch 001',
    '',
    '## สรุปสั้นสำหรับคนตัดสินใจ',
    '',
    f"- รันทั้งหมด: {len(results)} scenarios",
    f"- วิธีคุยที่สร้างความไว้ใจดีที่สุด: **{headline_trust[0]}** (คะแนนเฉลี่ย {avg_score(headline_trust[1], 'trust_built')})",
    f"- วิธีคุยที่ทำให้เรื่องชัดที่สุด: **{headline_clarity[0]}** (คะแนนเฉลี่ย {avg_score(headline_clarity[1], 'clarity')})",
    f"- วิธีคุยที่กดดันน้อยที่สุด: **{headline_low_pressure[0]}** (คะแนนเฉลี่ย {avg_score(headline_low_pressure[1], 'pressure_level')})",
    '- วิธีคุยที่ต้องระวังที่สุด: **เน้นชุมชนและการซัพพอร์ต** เพราะมีโอกาสถูกตีความเป็นแรงกดดันทางสังคม',
    '',
    '## ข้อสรุปสำคัญ',
    '',
    '1. ถ้าอยากได้ความไว้ใจ ให้เริ่มจากถามเป้าหมายและข้อกังวลก่อน pitch',
    '2. ถ้าอยากลดแรงต้าน ให้เริ่มจากของจริง เช่น สินค้า การใช้งาน และภาระงานจริง',
    '3. ถ้าใช้มุม community ต้องระวังมาก เพราะบาง persona แพ้ framing แบบนี้ทันที',
    '4. คนที่ดูสนใจมาก ไม่ได้แปลว่าเหมาะ — โดยเฉพาะ persona ที่มีแนวโน้มหวังรายได้เร็ว',
    '',
    '## ตารางคะแนนตาม strategy',
    '',
]

for strategy_name, items in by_strategy.items():
    report_lines += [
        f"### {strategy_name}",
        f"- ความไว้ใจ (trust): {avg_score(items, 'trust_built')}",
        f"- ความชัดเจน (clarity): {avg_score(items, 'clarity')}",
        f"- แรงกดดัน (pressure): {avg_score(items, 'pressure_level')}",
        f"- ความเหมาะสมกับ persona (fit): {avg_score(items, 'persona_fit')}",
        '',
    ]

report_lines += ['## สิ่งที่เจอในแต่ละ persona', '']
for persona_name, items in by_persona.items():
    best_item = max(items, key=lambda x: x['scores']['trust_built'] + x['scores']['clarity'] - x['scores']['pressure_level'])
    worst_item = max(items, key=lambda x: x['scores']['pressure_level'] - x['scores']['trust_built'])
    lesson = {
        'พนักงานออฟฟิศสายระวังตัว': 'ต้องใช้ความตรง ความโปร่งใส และหลีกเลี่ยง social framing',
        'เด็กจบใหม่ที่มีความหวังสูง': 'ต้อง reset expectation ให้เร็ว อย่าปล่อยให้ตีความว่าทำง่ายหรือรวยไว',
        'คุณพ่อ/คุณแม่ที่ดูแลบ้าน': 'ต้องอธิบายเวลา พลังงาน และภาระ onboarding ให้เห็นภาพมากที่สุด',
        'เจ้าของกิจการที่เวลาน้อย': 'ต้องกระชับ ตรง ประเด็น และชัดเรื่อง economics กับ workload',
    }[persona_name]
    report_lines += [
        f"### {persona_name}",
        f"- แบบที่เหมาะที่สุด: {best_item['strategy']} ({best_item['scenario_id']})",
        f"- แบบที่เปราะ/เสี่ยงที่สุด: {worst_item['strategy']} ({worst_item['scenario_id']})",
        f"- บทเรียนหลัก: {lesson}",
        '',
    ]

report_lines += [
    '## ข้อแนะนำเชิงใช้งาน',
    '',
    '- 2 เทิร์นแรกควรใช้เพื่อเช็ก fit, ข้อกังวล, เวลา, และสิ่งที่อีกฝ่ายไม่อยากเจอ',
    '- ควรส่งสรุปเป็นข้อความหรือเอกสารให้ทบทวน มากกว่าปิดด้วย call-to-action ทันที',
    '- ถ้าบทสนทนามีกลิ่น social pressure หรือ belonging pressure ให้ลดน้ำหนักภาษาทันที',
    '- ถ้าอีกฝ่ายสนใจแต่ดูเปราะบางต่อภาพฝันเรื่องรายได้ ให้บังคับมี expectation-reset script',
    '',
    '## เกณฑ์คัดกรองเวอร์ชันใช้งานจริง',
    '',
    '- ควรคัดออกทันที ถ้าอีกฝ่ายไม่สบายใจกับ dynamic การชวนคน/ดูแลเครือข่าย และความไว้ใจลดลงหลังอธิบายครบแล้ว',
    '- ควรไปต่อแบบระวัง ถ้ายังสนใจ แต่ยังไม่เข้าใจ effort, cost, หรือธรรมชาติของงาน',
    '- ควรนัดขั้นถัดไปได้ ก็ต่อเมื่ออีกฝ่ายสามารถอธิบายงานจริงกลับมาได้เอง โดยไม่ถูกเร่ง',
]

report_text = '\n'.join(report_lines)
(REPORTS_DIR / 'batch-001-report.md').write_text(report_text)
(REPORTS_DIR / 'final-report.md').write_text(report_text)

summary = {
    'scenarios_run': len(results),
    'best_strategy_by_trust': headline_trust[0],
    'best_strategy_by_clarity': headline_clarity[0],
    'best_strategy_by_low_pressure': headline_low_pressure[0],
}
print(json.dumps(summary, ensure_ascii=False, indent=2))
