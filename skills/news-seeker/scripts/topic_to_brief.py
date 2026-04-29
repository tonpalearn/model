#!/usr/bin/env python3
"""Turn gathered notes into a structured brief.

Usage:
  python3 topic_to_brief.py notes.txt
"""

from pathlib import Path
import sys


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 topic_to_brief.py <notes-file>")
        sys.exit(1)

    path = Path(sys.argv[1])
    text = path.read_text(encoding="utf-8")
    lines = [line.strip() for line in text.splitlines() if line.strip()]

    print("# Structured Brief")
    print()
    print("## Topic")
    print(lines[0] if lines else "Unknown topic")
    print()
    print("## Raw notes")
    for line in lines[1:]:
        print(f"- {line}")
    print()
    print("## What happened")
    print("Summarize the central event from the notes above.")
    print()
    print("## Key facts")
    print("- Extract the most important verifiable facts.")
    print()
    print("## Why it matters")
    print("Explain significance for the intended audience.")
    print()
    print("## What is still unclear")
    print("List open questions, conflicts, or missing information.")
    print()
    print("## What to watch next")
    print("List next developments to monitor.")


if __name__ == "__main__":
    main()
