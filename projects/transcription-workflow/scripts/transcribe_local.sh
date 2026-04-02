#!/bin/bash
set -e

if [ $# -lt 1 ]; then
  echo "Usage: $0 <media-file> [short|medium|long]"
  echo "Runs Whisper transcription first; summarize remains optional for follow-up summaries."
  exit 1
fi

FILE="$1"
LENGTH="${2:-medium}"
OUTDIR="$(pwd)/projects/transcription-workflow/test-output"
mkdir -p "$OUTDIR"

whisper "$FILE" --model turbo --language th --task transcribe --output_format txt --output_dir "$OUTDIR"

echo "\n--- Optional follow-up summary (${LENGTH}) ---\n"
summarize "$FILE" --length "$LENGTH" || true
