---
name: news-seeker
description: Research a requested topic and turn scattered news into a clear, decision-useful synthesis. Use when the user wants you to investigate a topic, follow a developing story, compare coverage, extract the important facts, identify what matters, or produce a concise news brief or insight memo from multiple sources based on a topic prompt.
---

# News Seeker

Investigate a user-provided topic and produce a reliable, structured synthesis from multiple sources.

## Core workflow

1. Clarify the topic only if the request is too broad to search effectively.
2. Search for multiple relevant sources around the requested topic.
3. Prefer original reporting, official statements, and reputable outlets over commentary roundups.
4. Compare claims across sources before concluding anything important.
5. Separate facts, interpretations, uncertainty, and implications.
6. Deliver a compact synthesis that answers: what happened, why it matters, what is still unclear, and what to watch next.

## Output structure

Default to this structure unless the user asks otherwise:

- **Topic**
- **What happened**
- **Key facts**
- **Why it matters**
- **What is still unclear**
- **What to watch next**
- **Sources**

If the user wants a shorter answer, compress to:
- 3 to 5 bullet summary
- 1 short implication paragraph
- source list

## Search strategy

Use a multi-source approach:

1. Search broad news coverage first.
2. Search for official or primary-source confirmation second.
3. Search for context or background third.
4. If the topic is fast-moving, note that the situation may change and prefer the latest timestamped reporting.

## Quality rules

- Do not rely on a single source for major conclusions.
- Flag disagreement between sources explicitly.
- Avoid overstating certainty.
- Distinguish reporting from opinion.
- If evidence is weak, say so plainly.
- If the topic involves numbers, dates, or named actors, verify them across sources.

## Helpful modes

### Mode A: quick brief
Use when the user wants a fast summary.
Deliver the default structure in a compact form.

### Mode B: insight memo
Use when the user wants meaning, not just facts.
Add:
- strategic implications
- possible second-order effects
- risks and opportunities

### Mode C: compare coverage
Use when the user wants to know how different outlets are framing the same story.
Add:
- outlet-by-outlet framing notes
- agreement vs disagreement
- missing angles

## If the user gives only a topic
Assume they want:
- a current-state summary
- the most important facts
- practical significance
- source links

## References

- Read `references/output-templates.md` when you need sample output shapes.
- Use `scripts/topic_to_brief.py` if you want a deterministic local formatter for turning gathered notes into a structured brief.
