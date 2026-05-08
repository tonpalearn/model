# Demand Supply Matcher

Overnight prototype for collecting public buyer-demand and supplier-supply signals from accessible internet sources, normalizing them, and producing candidate matches with deal details.

## Current assumptions
- Domain: generic B2B products/services
- Geography: global + SEA
- Sources: public web search results, public pages, and curated accessible public records
- Output target: candidate matches with confidence scores, reasons, and next actions

## Run

```bash
python3 scripts/ingest_real_sources.py
python3 scripts/run_matcher.py
```

Outputs are written to `output/`.

## Notes
- `data/demand.csv` and `data/supply.csv` are seed records for initial testing.
- `scripts/ingest_real_sources.py` writes source-grounded public-web records into `data/real_demand.csv` and `data/real_supply.csv`.
- The matcher combines both seed and real public records, then ranks candidate matches.
