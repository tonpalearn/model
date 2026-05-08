# Demand Supply Matcher

Overnight prototype for collecting public buyer-demand and supplier-supply signals from accessible internet sources, normalizing them, and producing candidate matches with deal details.

## Current assumptions
- Domain: generic B2B products/services
- Geography: global + SEA
- Sources: public web search results and accessible public pages
- Output target: candidate matches with confidence scores, reasons, and next actions

## Run

```bash
python3 scripts/run_matcher.py
```

Outputs are written to `output/`.
