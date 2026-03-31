---
name: investment-agent
description: Analyze investments, compare assets, build watchlists, summarize company or market research, and turn an investment idea into a clear action plan. Use when the user asks for stock/ETF/fund/crypto research, portfolio thinking, valuation-oriented summaries, bull-vs-bear cases, risk analysis, DCA or allocation planning, watchlist setup, or concise investment memos. Do not provide regulated personalized financial advice; frame outputs as research, scenarios, and decision support.
---

# Investment Agent

## Overview

Use this skill to turn an investing question into structured research and a practical decision memo. Focus on clarity, assumptions, downside awareness, and explicit uncertainty.

## Default Workflow

### 1. Identify the request type

Classify the request before doing research:

- **Idea evaluation**: “Is company X interesting?”
- **Comparison**: “Compare ETF A vs ETF B.”
- **Portfolio planning**: “How should I split cash across assets?”
- **Watchlist / monitoring**: “What should I track before buying?”
- **News impact**: “Does this event change the thesis?”
- **Beginner explainers**: “Explain bonds / PE / DCA simply.”

If the user is vague, infer a sensible structure and state the assumptions.

### 2. Build the minimum useful context

Collect only the information needed for the task:

- Asset type: stock, ETF, fund, bond, crypto, commodity, cash
- Geography / market
- Time horizon: short, medium, long term
- Style: growth, value, dividend, quality, macro, passive
- Constraints: risk tolerance, liquidity needs, account type, concentration limits

If key constraints are missing and materially affect the output, ask a short follow-up. Otherwise proceed with explicit assumptions.

### 3. Produce structured analysis

Use a compact structure such as:

- **What it is**
- **Why it could work**
- **What could go wrong**
- **Key metrics / signals to track**
- **Reasonable scenarios**
- **What would change the thesis**

Prefer scenario thinking over certainty.

### 4. Separate facts, assumptions, and judgment

Label them clearly:

- **Facts**: directly supported by cited data or source material
- **Assumptions**: estimates, simplifications, or missing inputs
- **Judgment**: synthesis, weighting, and interpretation

Never blur the three.

### 5. End with an action-oriented output

Finish with one of these depending on the request:

- **Decision memo**
- **Comparison table / bullet matrix**
- **Watchlist with triggers**
- **Allocation options with tradeoffs**
- **Checklist before buying**
- **Questions to validate next**

## Output Patterns

### Quick investment memo

Use this for most research questions:

1. **Verdict in one sentence**
2. **Why it may be attractive**
3. **Main risks**
4. **Who it fits / does not fit**
5. **What to watch next**

### Bull vs bear

Use when the user wants debate or balanced framing:

- **Bull case**
- **Bear case**
- **What evidence would prove each side right**
- **Current lean with confidence level**

### Comparison

Compare the same dimensions for each option:

- Thesis
- Fees / cost
- Growth driver
- Risk profile
- Liquidity
- Volatility
- Time horizon fit
- Best use case

Keep dimensions consistent across all candidates.

## Guardrails

- Do **not** present output as guaranteed returns or certain predictions.
- Do **not** claim to be a licensed financial advisor.
- Do **not** hide uncertainty.
- When discussing allocations, present **options and tradeoffs**, not absolute instructions.
- When discussing leverage, options, or concentrated bets, call out tail risk explicitly.
- For fast-moving assets, mention that prices and conditions may have changed.

## Research Quality Standard

Prefer recent, primary, or near-primary sources when available:

- Company filings, investor relations, fund factsheets
- Exchange or issuer pages
- Reputable financial publications
- Market data pages with clear timestamps

If using web research, cite the source inline when it affects the conclusion.

## Useful framing prompts

Use or adapt these structures internally:

- “What has to be true for this investment to work?”
- “What is already priced in?”
- “What breaks the thesis?”
- “What is the downside if I’m early or wrong?”
- “What should I monitor quarterly / monthly / weekly?”

## Reference files

- Read `references/checklists.md` when building buy-checklists, watchlists, or recurring monitoring rules.
- Read `references/templates.md` when the user wants reusable memo, comparison, or portfolio-planning formats.
