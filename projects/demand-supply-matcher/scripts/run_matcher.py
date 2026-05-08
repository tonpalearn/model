#!/usr/bin/env python3
import csv
import json
import re
from pathlib import Path

BASE = Path(__file__).resolve().parents[1]
DATA = BASE / 'data'
OUT = BASE / 'output'
OUT.mkdir(parents=True, exist_ok=True)

DEMAND_FILE = DATA / 'demand.csv'
SUPPLY_FILE = DATA / 'supply.csv'
MATCH_FILE = OUT / 'matches.csv'
SUMMARY_FILE = OUT / 'summary.md'

STOPWORDS = {
    'and','the','for','with','from','into','your','this','that','are','you','our','their','can','all','any',
    'global','worldwide','online','service','services','product','products','solutions','solution','business',
    'supplier','suppliers','buyer','buyers','manufacturer','manufacturers','company','companies','request','requests'
}


def tokenize(text: str):
    words = re.findall(r'[a-zA-Z0-9\-\+]+', (text or '').lower())
    return {w for w in words if len(w) > 2 and w not in STOPWORDS}


def load_csv(path: Path):
    with path.open(newline='', encoding='utf-8') as f:
        return list(csv.DictReader(f))


def overlap_score(a, b):
    if not a or not b:
        return 0.0
    inter = a & b
    union = a | b
    return len(inter) / len(union) if union else 0.0


def location_score(demand, supply):
    d = (demand.get('location') or '').lower()
    s = (supply.get('location') or '').lower()
    if not d or not s:
        return 0.4
    if d == s:
        return 1.0
    if any(x in s for x in d.split('/')) or any(x in d for x in s.split('/')):
        return 0.7
    return 0.2


def price_score(demand, supply):
    def to_num(v):
        if not v:
            return None
        m = re.search(r'\d+(?:\.\d+)?', v.replace(',', ''))
        return float(m.group()) if m else None
    dmin = to_num(demand.get('budget_min'))
    dmax = to_num(demand.get('budget_max'))
    smin = to_num(supply.get('price_min'))
    smax = to_num(supply.get('price_max'))
    if all(v is None for v in [dmin, dmax, smin, smax]):
        return 0.5
    low = max(v for v in [dmin, smin] if v is not None) if any(v is not None for v in [dmin, smin]) else None
    high = min(v for v in [dmax, smax] if v is not None) if any(v is not None for v in [dmax, smax]) else None
    if low is not None and high is not None and low <= high:
        return 1.0
    return 0.3


def make_reason(tokens_d, tokens_s, demand, supply, score):
    shared = sorted((tokens_d & tokens_s))[:6]
    bits = []
    if shared:
        bits.append('shared keywords: ' + ', '.join(shared))
    if demand.get('location') and supply.get('location'):
        bits.append(f"location: {demand['location']} ↔ {supply['location']}")
    if demand.get('quantity') or supply.get('moq'):
        bits.append(f"quantity/MOQ: demand {demand.get('quantity','n/a')} vs supply MOQ {supply.get('moq','n/a')}")
    bits.append(f"confidence {score:.2f}")
    return ' | '.join(bits)


def main():
    demands = load_csv(DEMAND_FILE)
    supplies = load_csv(SUPPLY_FILE)
    matches = []
    for demand in demands:
        td = tokenize(' '.join([demand.get('title',''), demand.get('description',''), demand.get('category','')]))
        for supply in supplies:
            ts = tokenize(' '.join([supply.get('title',''), supply.get('description',''), supply.get('category','')]))
            kw = overlap_score(td, ts)
            loc = location_score(demand, supply)
            price = price_score(demand, supply)
            score = kw * 0.55 + loc * 0.2 + price * 0.15 + 0.1
            if kw < 0.08:
                continue
            matches.append({
                'score': round(score, 4),
                'demand_id': demand['id'],
                'demand_title': demand['title'],
                'demand_source': demand['source'],
                'demand_url': demand['url'],
                'demand_location': demand.get('location',''),
                'demand_contact': demand.get('contact',''),
                'supply_id': supply['id'],
                'supply_title': supply['title'],
                'supply_source': supply['source'],
                'supply_url': supply['url'],
                'supply_location': supply.get('location',''),
                'supply_contact': supply.get('contact',''),
                'reason': make_reason(td, ts, demand, supply, score),
                'next_action': 'Validate specs, confirm availability, then outreach both sides with tailored intro.'
            })
    matches.sort(key=lambda x: x['score'], reverse=True)
    top = matches[:100]
    with MATCH_FILE.open('w', newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=list(top[0].keys()) if top else ['score'])
        writer.writeheader()
        writer.writerows(top)
    lines = [
        '# Demand-Supply Matching Summary',
        '',
        f'- Demand records: {len(demands)}',
        f'- Supply records: {len(supplies)}',
        f'- Candidate matches found: {len(matches)}',
        f'- Top matches exported: {len(top)}',
        '',
        '## Top 10 matches',
        ''
    ]
    for row in top[:10]:
        lines += [
            f"### {row['demand_title']} ↔ {row['supply_title']}",
            f"- Score: {row['score']}",
            f"- Demand: {row['demand_source']} | {row['demand_url']}",
            f"- Supply: {row['supply_source']} | {row['supply_url']}",
            f"- Reason: {row['reason']}",
            f"- Next action: {row['next_action']}",
            ''
        ]
    SUMMARY_FILE.write_text('\n'.join(lines), encoding='utf-8')
    print(json.dumps({'matches_total': len(matches), 'matches_exported': len(top), 'out': str(MATCH_FILE)}))

if __name__ == '__main__':
    main()
