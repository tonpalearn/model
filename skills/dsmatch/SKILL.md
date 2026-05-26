---
name: dsmatch
description: Find public demand signals from marketplaces, forums, directories, or web pages, match them against supply from other public web sources, and estimate whether an intermediary can profitably broker the match. Use when the user wants demand-supply matching, broker/arbitrage opportunities, marketplace scanning, or a ranked list of matches with margin estimates and outreach details.
---

# DSMatch

Create or update demand-supply datasets, score cross-source matches, and estimate brokerability.

## Workflow

1. Gather demand records from public sources into a structured file.
2. Gather supply records from separate public sources into a structured file.
3. Run the matcher script to score pairs and estimate brokerability and gross margin.
4. Export CSV and XLSX outputs for review.

## Files

- Main script: `scripts/dsmatch.py`
- Reference schema: `references/schema.md`
- Default output folder: `projects/demand-supply-matcher/output/`

## Run

```bash
python3 skills/dsmatch/scripts/dsmatch.py
```

## Notes

- Keep demand and supply sourced from different pages or domains where possible.
- Use only source-grounded public records. Do not invent contacts, prices, or margins.
- Margin estimates are heuristic unless both sides expose pricing or budget ranges.
