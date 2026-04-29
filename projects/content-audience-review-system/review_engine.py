import re
from collections import Counter


STOPWORDS = {
    'the','a','an','and','or','but','to','of','in','on','for','with','is','are','was','were','be','been','that','this',
    'คือ','และ','ใน','ที่','ให้','ของ','กับ','เป็น','ได้','ว่า','มี','จาก','หรือ','ก็','ไป','มา','แล้ว','ยัง'
}


def tokenize(text):
    words = re.findall(r"[A-Za-zก-๙0-9']+", text.lower())
    return [w for w in words if w not in STOPWORDS and len(w) > 1]


def detect_tone(text):
    lower = text.lower()
    if any(x in lower for x in ['ด่วน', 'รีบ', 'urgent', 'must', 'ห้ามพลาด']):
        return 'high-pressure'
    if any(x in lower for x in ['วิธี', 'how', 'step', 'guide', 'framework']):
        return 'educational'
    if any(x in lower for x in ['โปร', 'promotion', 'discount', 'ลดราคา', 'ฟรี']):
        return 'promotional'
    return 'informational'


def score_hook(text):
    lines = [x.strip() for x in text.splitlines() if x.strip()]
    if not lines:
        return 0
    first = lines[0]
    score = 0
    if len(first) < 120:
        score += 1
    if any(x in first.lower() for x in ['คุณ', 'เคย', '?', 'why', 'how', 'ถ้า', 'ทำไม']):
        score += 1
    if any(x in first.lower() for x in ['เจอ', 'ปัญหา', 'ผิด', 'เสีย', 'lost', 'mistake', 'pain']):
        score += 1
    return score


def likely_content_type(text, forced=None):
    if forced:
        return forced
    t = text.lower()
    if len(text) < 500:
        return 'short_post'
    if any(x in t for x in ['headline', 'cta', 'caption']):
        return 'marketing_copy'
    return 'article'


def persona_review(persona, text, title='', target='general audience', content_format=None, preset=None):
    hook_score = score_hook(text)
    tone = detect_tone(text)
    content_type = likely_content_type(text, content_format)
    tokens = tokenize(text)
    top_terms = [w for w, _ in Counter(tokens).most_common(12)]

    first_impression = []
    emotional_reaction = []
    works = []
    loses = []
    change = []

    if hook_score >= 2:
        first_impression.append('เปิดเรื่องค่อนข้างจับความสนใจได้')
    else:
        first_impression.append('ช่วงเปิดยังไม่ดึงพอสำหรับ persona นี้')
        change.append('ปรับ hook 1-2 บรรทัดแรกให้แรงขึ้นและชัดขึ้น')

    if tone == 'promotional':
        emotional_reaction.append('รับรู้ว่าเป็น content เชิงขายหรือโปรโมตค่อนข้างชัด')
    elif tone == 'educational':
        emotional_reaction.append('รู้สึกว่า content นี้ตั้งใจให้ความรู้หรือจัดกรอบความคิด')
    else:
        emotional_reaction.append('อารมณ์โดยรวมยังกลาง ๆ และขึ้นอยู่กับ execution')

    if len(text) > 1800 and 'short-attention' in persona.get('traits', []):
        loses.append('เนื้อหาค่อนข้างยาวเกินไปสำหรับคนที่ไถเร็ว')
        change.append('ย่อหรือแตกย่อหน้าให้สั้นลง พร้อมใส่จุดพักสายตา')

    if 'results-oriented' in persona.get('traits', []):
        if any(x in text.lower() for x in ['ผลลัพธ์', 'result', 'roi', 'คุ้ม', 'ประหยัดเวลา']):
            works.append('เริ่มเห็นประโยชน์เชิงผลลัพธ์ที่จับต้องได้')
        else:
            loses.append('ยังไม่เห็นผลลัพธ์ทางธุรกิจหรือ practical value ชัดพอ')
            change.append('เพิ่มผลลัพธ์ที่ชัด เช่น ประหยัดเวลา เพิ่มยอด ลดข้อผิดพลาด')

    if 'trust-sensitive' in persona.get('traits', []):
        if any(x in text.lower() for x in ['ข้อมูล', 'ตัวอย่าง', 'พิสูจน์', 'รีวิว', 'case study', 'ผลลัพธ์จริง']):
            works.append('มีสัญญาณความน่าเชื่อถือพอสมควร')
        else:
            loses.append('ยังขาดตัวช่วยด้าน trust เช่น proof, example, case หรือ reference')
            change.append('เติม proof point หรือกรณีตัวอย่างจริง')

    if 'trend-sensitive' in persona.get('traits', []):
        if hook_score < 3:
            loses.append('ยังไม่หยุดนิ้วได้เร็วพอ')
            change.append('ทำ opening ให้ punchy, relatable, และ shareable มากขึ้น')
        else:
            works.append('opening มีโอกาสหยุดสายตาได้')

    if 'critical' in persona.get('traits', []):
        if any(x in text.lower() for x in ['ดีที่สุด', 'อันดับหนึ่ง', 'การันตี', '100%']):
            loses.append('มีคำที่ฟังดูขายแรงหรือเกินจริงสำหรับ persona นี้')
            change.append('ลดคำเคลมแรง และเพิ่ม specificity แทน')

    understood_value = 'พอเข้าใจว่า content นี้พยายามจะสื่ออะไร' if works or hook_score >= 1 else 'ยังไม่เข้าใจ value ชัดพอในช่วงแรก'
    trust_level = 'medium'
    if 'proof point หรือกรณีตัวอย่างจริง' in ' | '.join(change):
        trust_level = 'low-medium'
    if any('น่าเชื่อถือ' in x for x in works):
        trust_level = 'medium-high'

    likelihood = 'high' if len(works) > len(loses) else 'medium' if len(works) == len(loses) else 'low'
    suggested_hook = f"ลองเปิดด้วย pain ที่ชัดขึ้นสำหรับ {persona['label']} และเชื่อมไปยังประโยชน์หลักทันที"

    hook_score_10 = min(10, max(3, hook_score * 3))
    clarity_score_10 = 7 if 'พอเข้าใจ' in understood_value else 4
    trust_score_10 = 8 if trust_level == 'medium-high' else 6 if trust_level == 'medium' else 4
    attention_score_10 = 8 if likelihood == 'high' else 6 if likelihood == 'medium' else 4
    conversion_score_10 = 6
    if any(x in text.lower() for x in ['ทัก', 'สมัคร', 'ซื้อ', 'อ่านต่อ', 'คลิก']):
        conversion_score_10 = 7
    if any('CTA' in x for x in change):
        conversion_score_10 = 5

    return {
        'persona_id': persona['id'],
        'persona_label': persona['label'],
        'background': {
            'age': persona.get('age'),
            'gender': persona.get('gender'),
            'occupation': persona.get('occupation'),
        },
        'first_impression': '; '.join(first_impression) or 'neutral',
        'emotional_reaction': '; '.join(emotional_reaction) or 'neutral',
        'understood_value': understood_value,
        'what_works': works or ['มีแกนเนื้อหาที่พอจับได้'],
        'what_loses_attention': loses or ['ยังไม่มีจุดหลุดแรงมาก แต่ยังไม่โดดเด่นพอ'],
        'trust_level': trust_level,
        'likelihood_to_continue': likelihood,
        'what_to_change_for_me': change or ['เพิ่มความคมของ opening และชัดเจนเรื่อง value'],
        'suggested_hook': suggested_hook,
        'detected_terms': top_terms,
        'detected_tone': tone,
        'content_type': content_type,
        'scores': {
            'hook': hook_score_10,
            'clarity': clarity_score_10,
            'trust': trust_score_10,
            'attention': attention_score_10,
            'conversion': conversion_score_10,
        }
    }


def specialist_review(specialist, text):
    tone = detect_tone(text)
    hook_score = score_hook(text)
    strengths, weaknesses, risks, changes = [], [], [], []

    if specialist['id'] == 'copy_strategist':
        if hook_score >= 2:
            strengths.append('ช่วงเปิดมีโครงพอใช้ได้')
        else:
            weaknesses.append('Hook ยังไม่แรงพอ')
            changes.append('เริ่มด้วย pain, tension หรือ promise ที่ชัดขึ้น')
        if len(text.split()) > 350:
            weaknesses.append('เนื้อหายาวและมีโอกาสหลุด focus')
            changes.append('ตัดส่วนซ้ำและจัด flow ใหม่ให้ชัดเป็นช่วง')
        strengths.append('สามารถพัฒนา narrative ให้ชัดได้จากโครงปัจจุบัน')
    elif specialist['id'] == 'visual_strategist':
        weaknesses.append('ถ้ายังไม่มี visual hierarchy ชัด คนจะรับสารช้าลง')
        changes.extend([
            'ใช้ภาพที่สื่อผลลัพธ์หรือ pain ได้ทันที',
            'เพิ่ม visual contrast และจุดพักสายตา',
            'ถ้าเป็นโพสต์ ให้คิด thumbnail / key visual แยกจาก body text'
        ])
        if tone == 'promotional':
            risks.append('เสี่ยงดูขายแรงถ้า visual push เกินไป')
    elif specialist['id'] == 'conversion_strategist':
        if 'ซื้อ' in text or 'สมัคร' in text or 'ทัก' in text:
            strengths.append('มี intent เชิง action อยู่แล้ว')
        else:
            weaknesses.append('ยังไม่มี CTA ที่เฉพาะพอ')
            changes.append('เพิ่ม CTA ที่ชัดว่าอยากให้คนดูทำอะไรต่อ')
        risks.append('ถ้า value proposition ยังไม่ชัด ต่อให้มี CTA ก็ไม่ convert')

    scores = {
        'copy_strategist': {'hook': 6 if hook_score >= 2 else 4, 'clarity': 6, 'trust': 6, 'attention': 5, 'conversion': 5},
        'visual_strategist': {'hook': 5, 'clarity': 6, 'trust': 6, 'attention': 5, 'conversion': 4},
        'conversion_strategist': {'hook': 5, 'clarity': 6, 'trust': 6, 'attention': 5, 'conversion': 6},
    }.get(specialist['id'], {'hook': 5, 'clarity': 5, 'trust': 5, 'attention': 5, 'conversion': 5})
    return {
        'specialist_id': specialist['id'],
        'specialist_label': specialist['label'],
        'strengths': strengths or ['มีฐานให้พัฒนาต่อได้'],
        'weaknesses': weaknesses or ['ยังไม่มีจุดอ่อนร้ายแรง แต่ยังไม่คมพอ'],
        'key_risks': risks or ['เสี่ยงถูกเลื่อนผ่านถ้าไม่ sharpen opening'],
        'top_changes': changes or ['ทำ message ให้คมขึ้นอีกระดับ'],
        'upgraded_direction': 'ปรับให้คมขึ้นทั้ง hook, clarity, structure, และ action path',
        'scores': scores,
    }


def lead_summary(persona_reviews, specialist_reviews, title='', target='general audience'):
    repeated_losses = Counter()
    repeated_changes = Counter()
    repeated_strengths = Counter()
    for r in persona_reviews:
        for x in r['what_loses_attention']:
            repeated_losses[x] += 1
        for x in r['what_to_change_for_me']:
            repeated_changes[x] += 1
        for x in r['what_works']:
            repeated_strengths[x] += 1
    for s in specialist_reviews:
        for x in s['weaknesses']:
            repeated_losses[x] += 1
        for x in s['top_changes']:
            repeated_changes[x] += 1
        for x in s['strengths']:
            repeated_strengths[x] += 1

    strongest_assets = [x for x, _ in repeated_strengths.most_common(5)]
    biggest_problems = [x for x, _ in repeated_losses.most_common(6)]
    priority_fixes_now = [x for x, _ in repeated_changes.most_common(6)]

    score_buckets = ['hook', 'clarity', 'trust', 'attention', 'conversion']
    aggregate = {k: [] for k in score_buckets}
    for r in persona_reviews:
        for k, v in r.get('scores', {}).items():
            aggregate[k].append(v)
    for s in specialist_reviews:
        for k, v in s.get('scores', {}).items():
            aggregate[k].append(v)
    aggregate_scores = {k: round(sum(v)/len(v), 1) if v else 0 for k, v in aggregate.items()}

    executive_summary = (
        f"Content '{title or 'Untitled'}' มีโครงที่พอพัฒนาได้ แต่ feedback แบบซ้ำชี้ว่าจุดที่ต้องแก้ก่อนคือช่วงเปิด, "
        f"ความชัดของ value, และวิธีทำให้คนรู้สึกว่า content นี้พูดกับเขาจริง ๆ สำหรับ target '{target}'."
    )

    return {
        'executive_summary': executive_summary,
        'repeated_patterns': {
            'strengths': strongest_assets,
            'problems': biggest_problems,
            'changes': priority_fixes_now,
        },
        'strongest_assets': strongest_assets,
        'biggest_problems': biggest_problems,
        'priority_fixes_now': priority_fixes_now,
        'message_rewrite_direction': 'ทำ message ให้ตอบ 3 เรื่องเร็วขึ้น: นี่พูดกับใคร, ได้อะไร, ทำไมต้องสนใจตอนนี้',
        'format_rewrite_direction': 'จัดเนื้อหาให้สั้นลง, แบ่งเป็นช่วงชัด, และมีจุดพักสายตาหรือ section framing ที่ดีขึ้น',
        'visual_rewrite_direction': 'ใช้ภาพที่สะท้อน pain หรือ desired outcome โดยตรง และทำ hierarchy ให้จับสารได้ในไม่กี่วินาที',
        'CTA_rewrite_direction': 'ทำ CTA ให้เฉพาะและเชื่อมตรงกับ intent ของคนดู เช่น อ่านต่อ, ทัก, สมัคร, เซฟ, แชร์',
        'revised_content_brief': [
            'เริ่มด้วย hook ที่สะท้อน pain หรือ tension ของ target โดยตรง',
            'ตามด้วย value proposition ที่ชัดใน 1-2 บรรทัด',
            'ใช้โครงที่สั้นและอ่านง่ายขึ้น',
            'เพิ่ม trust signal เช่น example, proof point, or concrete detail',
            'ปิดด้วย CTA ที่ตรงกับ stage ของคนดู'
        ],
        'aggregate_scores': aggregate_scores,
    }
