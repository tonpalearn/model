# Usage Guide

## 1. Put your content in a file
Examples:
- `sample_input/article_example.md`
- your own `.md` or `.txt` file

## 2. Run the review
```bash
python3 main.py sample_input/article_example.md --title "AI for SME" --target "Thai SME owners" --format article
```

## 3. Outputs
The system generates:
- `output/<file>_review.json`
- `output/<file>_review.md`

## 4. Recommended workflow
1. Paste draft content into a text file
2. Run review
3. Read the lead summary first
4. Then inspect persona reactions
5. Revise content
6. Run it again

## 5. Best use cases
- Article review
- Social post review
- Ad copy review
- Landing page copy review
- Creative brief review

## 6. Presets available
- article
- social_post
- ad_copy
- landing_page
- script
- image_brief

## 7. Scores included
Each run now includes score signals across:
- hook
- clarity
- trust
- attention
- conversion

## 8. What to improve later
If desired, this project can later be upgraded to:
- true LLM-backed persona agents
- image review support
- multi-round debate between agents
- weighted persona boards
- target-specific board presets (clinic, SME, B2B, wellness, AI page, etc.)
