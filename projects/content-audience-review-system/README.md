# Content Audience Review System

Audience simulation and multi-agent content review workflow for evaluating how different personas react to a piece of content, then producing a lead-editor summary with actionable recommendations.

## What it does
- Simulates a diverse review panel of persona agents
- Adds specialist reviewers for copy, visual, and conversion feedback
- Produces a lead summary with clear recommendations on message, structure, visual direction, and CTA
- Works on article copy, social posts, ad copy, landing pages, and image/video creative briefs

## Default review board
### Persona reviewers
1. Young Professional Woman (28, office worker)
2. Founder / Business Owner Man (35)
3. Experienced Working Mother (42)
4. Gen Z Fast-Scroller (23)
5. Skeptical Professional (31)
6. Practical Budget-Conscious Consumer (37)
7. Aspirational Lifestyle Seeker (29)
8. Expert / Analytical Reviewer (40)

### Specialist reviewers
9. Copy Strategist
10. Visual Strategist
11. Conversion Strategist

### Lead reviewer
12. Lead Editor / Head of Insight

## Output
- First impression by persona
- Emotional reaction by persona
- What works / what loses attention
- Persona-specific recommendations
- Specialist recommendations
- Final merged summary from lead reviewer
- Suggested revision direction for text, format, image, and CTA

## Quick start
```bash
python3 main.py sample_input/article_example.md
```

## Files
- `personas.yaml` - persona and specialist definitions
- `main.py` - main runner
- `review_engine.py` - review orchestration and summarization logic
- `prompts.py` - prompt builders and output format rules
- `sample_input/` - example content
- `output/` - generated review reports

## Run options
```bash
python3 main.py <content_file>
python3 main.py <content_file> --title "Your content title"
python3 main.py <content_file> --target "Thai SME founders"
python3 main.py <content_file> --format article
```

## Current mode
This version is built to be ready-to-use out of the box with a deterministic heuristic review engine and structured personas. You can later swap the reviewer layer with LLM-backed agents if desired.
