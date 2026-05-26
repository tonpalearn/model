#!/usr/bin/env python3
import csv
import json
import re
import zipfile
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path('/Users/ckawin/.openclaw/workspace')
DATA = ROOT / 'projects' / 'demand-supply-matcher' / 'data'
OUT = ROOT / 'projects' / 'demand-supply-matcher' / 'output'
OUT.mkdir(parents=True, exist_ok=True)

DEMAND_FILES = [DATA / 'demand.csv', DATA / 'real_demand.csv']
SUPPLY_FILES = [DATA / 'supply.csv', DATA / 'real_supply.csv']
CSV_OUT = OUT / 'dsmatch_50_matches.csv'
XLSX_OUT = OUT / 'dsmatch_50_matches.xlsx'
JSON_OUT = OUT / 'dsmatch_summary.json'

STOPWORDS = {
    'and','the','for','with','from','into','your','this','that','are','you','our','their','can','all','any',
    'global','worldwide','online','service','services','product','products','solutions','solution','business',
    'supplier','suppliers','buyer','buyers','manufacturer','manufacturers','company','companies','request','requests'
}


def tokenize(text: str):
    words = re.findall(r'[a-zA-Z0-9\-\+]+', (text or '').lower())
    return {w for w in words if len(w) > 2 and w not in STOPWORDS}


def load_csv(path: Path):
    if not path.exists():
        return []
    with path.open(newline='', encoding='utf-8') as f:
        return list(csv.DictReader(f))


def load_all(paths):
    rows = []
    for p in paths:
        rows.extend(load_csv(p))
    return rows


def to_num(v):
    if not v:
        return None
    m = re.search(r'\d+(?:\.\d+)?', str(v).replace(',', ''))
    return float(m.group()) if m else None


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
    if 'global' in d or 'global' in s:
        return 0.7
    if any(x in s for x in d.split('/')) or any(x in d for x in s.split('/')):
        return 0.7
    return 0.2


def price_overlap(demand, supply):
    dmin = to_num(demand.get('budget_min'))
    dmax = to_num(demand.get('budget_max'))
    smin = to_num(supply.get('price_min'))
    smax = to_num(supply.get('price_max'))
    if all(v is None for v in [dmin, dmax, smin, smax]):
        return 0.5, None, None, 'No public pricing on one or both sides; heuristic only.'
    low = max(v for v in [dmin, smin] if v is not None) if any(v is not None for v in [dmin, smin]) else None
    high = min(v for v in [dmax, smax] if v is not None) if any(v is not None for v in [dmax, smax]) else None
    if low is not None and high is not None and low <= high:
        margin_low = max(0.0, low - (smin or low))
        margin_high = max(0.0, (dmax or high) - (smax or high))
        return 1.0, margin_low, margin_high, 'Both sides expose compatible public price/budget ranges.'
    return 0.3, 0.0, 0.0, 'Public price/budget ranges do not clearly overlap.'


def brokerability(score, margin_high, demand, supply):
    contact_ok = 1 if (demand.get('contact') and supply.get('contact')) else 0.6
    source_sep = 1 if demand.get('url') != supply.get('url') else 0.2
    margin_signal = 1 if (margin_high or 0) > 0 else 0.5
    raw = score * 0.55 + contact_ok * 0.15 + source_sep * 0.15 + margin_signal * 0.15
    if raw >= 0.75:
        return 'high', raw
    if raw >= 0.55:
        return 'medium', raw
    return 'low', raw


def reasoning(shared, demand, supply, margin_basis, broker_score):
    parts = []
    if shared:
        parts.append('shared keywords: ' + ', '.join(shared[:6]))
    parts.append(f"locations: {demand.get('location','n/a')} ↔ {supply.get('location','n/a')}")
    parts.append(margin_basis)
    parts.append(f'broker score {broker_score:.2f}')
    return ' | '.join(parts)


def suggested_offer(demand, supply, brokerability_label, margin_high):
    base = f"Offer to broker {demand.get('title','this demand')} with {supply.get('title','this supplier')}"
    if brokerability_label == 'high' and (margin_high or 0) > 0:
        return base + ' on a success-fee or margin-share basis.'
    if brokerability_label == 'medium':
        return base + ' after validating specs, timing, and commercial terms.'
    return base + ' only after manual verification of budget, fit, and contact responsiveness.'


def write_xlsx(rows, path: Path):
    headers = list(rows[0].keys()) if rows else []
    strings = []
    s_index = {}
    def s_id(v):
        v = '' if v is None else str(v)
        if v not in s_index:
            s_index[v] = len(strings)
            strings.append(v)
        return s_index[v]
    for r in rows:
        for h in headers:
            s_id(r.get(h,''))
    shared = ['<?xml version="1.0" encoding="UTF-8" standalone="yes"?>',
              '<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="%d" uniqueCount="%d">' % (len(strings), len(strings))]
    for s in strings:
        shared.append(f'<si><t>{escape(s)}</t></si>')
    shared.append('</sst>')
    sheet = ['<?xml version="1.0" encoding="UTF-8" standalone="yes"?>',
             '<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>']
    all_rows = [dict(zip(headers, headers))] + rows
    for ridx, row in enumerate(all_rows, start=1):
        sheet.append(f'<row r="{ridx}">')
        for cidx, h in enumerate(headers, start=1):
            col = ''
            n = cidx
            while n:
                n, rem = divmod(n-1, 26)
                col = chr(65+rem) + col
            val = h if ridx == 1 else row.get(h,'')
            sheet.append(f'<c r="{col}{ridx}" t="s"><v>{s_id(val)}</v></c>')
        sheet.append('</row>')
    sheet.append('</sheetData></worksheet>')
    workbook = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="matches" sheetId="1" r:id="rId1"/></sheets></workbook>'''
    workbook_rels = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/><Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/></Relationships>'''
    root_rels = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>'''
    content_types = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/><Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/></Types>'''
    styles = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts><fills count="1"><fill><patternFill patternType="none"/></fill></fills><borders count="1"><border/></borders><cellStyleXfs count="1"><xf/></cellStyleXfs><cellXfs count="1"><xf xfId="0"/></cellXfs></styleSheet>'''
    with zipfile.ZipFile(path, 'w', compression=zipfile.ZIP_DEFLATED) as z:
        z.writestr('[Content_Types].xml', content_types)
        z.writestr('_rels/.rels', root_rels)
        z.writestr('xl/workbook.xml', workbook)
        z.writestr('xl/_rels/workbook.xml.rels', workbook_rels)
        z.writestr('xl/worksheets/sheet1.xml', '\n'.join(sheet))
        z.writestr('xl/sharedStrings.xml', '\n'.join(shared))
        z.writestr('xl/styles.xml', styles)


def main():
    demands = load_all(DEMAND_FILES)
    supplies = load_all(SUPPLY_FILES)
    results = []
    seen = set()
    for d in demands:
        td = tokenize(' '.join([d.get('title',''), d.get('description',''), d.get('category','')]))
        for s in supplies:
            if d.get('url') == s.get('url'):
                continue
            key = (d.get('url'), s.get('url'))
            if key in seen:
                continue
            ts = tokenize(' '.join([s.get('title',''), s.get('description',''), s.get('category','')]))
            kw = overlap_score(td, ts)
            if kw < 0.07:
                continue
            loc = location_score(d, s)
            price_fit, margin_low, margin_high, margin_basis = price_overlap(d, s)
            score = kw * 0.5 + loc * 0.2 + price_fit * 0.2 + 0.1
            label, bscore = brokerability(score, margin_high, d, s)
            shared = sorted(td & ts)
            results.append({
                'score': round(score, 4),
                'brokerability': label,
                'estimated_margin_low': '' if margin_low is None else round(margin_low, 2),
                'estimated_margin_high': '' if margin_high is None else round(margin_high, 2),
                'margin_basis': margin_basis,
                'demand_title': d.get('title',''),
                'demand_url': d.get('url',''),
                'demand_contact': d.get('contact',''),
                'supply_title': s.get('title',''),
                'supply_url': s.get('url',''),
                'supply_contact': s.get('contact',''),
                'reasoning': reasoning(shared, d, s, margin_basis, bscore),
                'suggested_offer': suggested_offer(d, s, label, margin_high),
            })
            seen.add(key)
    results.sort(key=lambda x: (x['brokerability'] == 'high', x['score'], x['estimated_margin_high'] if x['estimated_margin_high'] != '' else -1), reverse=True)
    top = results[:50]
    with CSV_OUT.open('w', newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=list(top[0].keys()) if top else [])
        writer.writeheader()
        writer.writerows(top)
    write_xlsx(top, XLSX_OUT)
    summary = {
        'demands': len(demands),
        'supplies': len(supplies),
        'matches_total': len(results),
        'matches_exported': len(top),
        'csv': str(CSV_OUT),
        'xlsx': str(XLSX_OUT),
    }
    JSON_OUT.write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps(summary))

if __name__ == '__main__':
    main()
