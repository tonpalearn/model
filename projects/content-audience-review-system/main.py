import argparse
import json
from pathlib import Path

import yaml

from review_engine import persona_review, specialist_review, lead_summary


def load_personas(path):
    with open(path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)


def read_content(path):
    return Path(path).read_text(encoding='utf-8')


def render_markdown_report(meta, persona_reviews, specialist_reviews, lead):
    lines = []
    lines.append(f"# Content Review Report: {meta['title']}")
    lines.append("")
    lines.append(f"- Target: {meta['target']}")
    lines.append(f"- Format: {meta['format']}")
    lines.append("")
    lines.append("## Lead Summary")
    lines.append(lead['executive_summary'])
    lines.append("")
    lines.append("### Strongest Assets")
    for x in lead['strongest_assets']:
        lines.append(f"- {x}")
    lines.append("")
    lines.append("### Biggest Problems")
    for x in lead['biggest_problems']:
        lines.append(f"- {x}")
    lines.append("")
    lines.append("### Priority Fixes Now")
    for x in lead['priority_fixes_now']:
        lines.append(f"- {x}")
    lines.append("")
    lines.append("### Rewrite Directions")
    lines.append(f"- Message: {lead['message_rewrite_direction']}")
    lines.append(f"- Format: {lead['format_rewrite_direction']}")
    lines.append(f"- Visual: {lead['visual_rewrite_direction']}")
    lines.append(f"- CTA: {lead['CTA_rewrite_direction']}")
    lines.append("")
    lines.append("### Revised Content Brief")
    for x in lead['revised_content_brief']:
        lines.append(f"- {x}")
    lines.append("")
    lines.append("## Persona Reviews")
    for r in persona_reviews:
        lines.append(f"### {r['persona_label']}")
        lines.append(f"- First impression: {r['first_impression']}")
        lines.append(f"- Emotional reaction: {r['emotional_reaction']}")
        lines.append(f"- Understood value: {r['understood_value']}")
        lines.append(f"- Trust level: {r['trust_level']}")
        lines.append(f"- Likelihood to continue: {r['likelihood_to_continue']}")
        lines.append("- What works:")
        for x in r['what_works']:
            lines.append(f"  - {x}")
        lines.append("- What loses attention:")
        for x in r['what_loses_attention']:
            lines.append(f"  - {x}")
        lines.append("- What to change for me:")
        for x in r['what_to_change_for_me']:
            lines.append(f"  - {x}")
        lines.append(f"- Suggested hook: {r['suggested_hook']}")
        lines.append("")
    lines.append("## Specialist Reviews")
    for s in specialist_reviews:
        lines.append(f"### {s['specialist_label']}")
        lines.append("- Strengths:")
        for x in s['strengths']:
            lines.append(f"  - {x}")
        lines.append("- Weaknesses:")
        for x in s['weaknesses']:
            lines.append(f"  - {x}")
        lines.append("- Risks:")
        for x in s['key_risks']:
            lines.append(f"  - {x}")
        lines.append("- Top changes:")
        for x in s['top_changes']:
            lines.append(f"  - {x}")
        lines.append(f"- Upgraded direction: {s['upgraded_direction']}")
        lines.append("")
    return '\n'.join(lines)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('content_file')
    parser.add_argument('--title', default='Untitled Content')
    parser.add_argument('--target', default='General Audience')
    parser.add_argument('--format', default=None)
    parser.add_argument('--output-dir', default='output')
    args = parser.parse_args()

    root = Path(__file__).parent
    cfg = load_personas(root / 'personas.yaml')
    text = read_content(args.content_file)

    persona_reviews = [
        persona_review(p, text, title=args.title, target=args.target, content_format=args.format)
        for p in cfg['personas']
    ]
    specialist_reviews = [
        specialist_review(s, text)
        for s in cfg['specialists']
    ]
    lead = lead_summary(persona_reviews, specialist_reviews, title=args.title, target=args.target)

    output_dir = root / args.output_dir
    output_dir.mkdir(parents=True, exist_ok=True)
    stem = Path(args.content_file).stem
    report = {
        'meta': {
            'title': args.title,
            'target': args.target,
            'format': args.format or 'auto',
            'source_file': str(args.content_file),
        },
        'persona_reviews': persona_reviews,
        'specialist_reviews': specialist_reviews,
        'lead_summary': lead,
    }

    json_path = output_dir / f'{stem}_review.json'
    md_path = output_dir / f'{stem}_review.md'
    json_path.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    md_path.write_text(render_markdown_report(report['meta'], persona_reviews, specialist_reviews, lead), encoding='utf-8')

    print(f'Wrote: {json_path}')
    print(f'Wrote: {md_path}')


if __name__ == '__main__':
    main()
