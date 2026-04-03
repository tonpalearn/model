#!/bin/bash
set -euo pipefail
export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:$PATH"
BRIEF="$(python3 /Users/ckawin/.openclaw/workspace/scripts/morning_brief.py)"
/usr/local/bin/openclaw message send --channel telegram --target 6271498929 --message "$BRIEF"
