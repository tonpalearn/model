#!/bin/bash
set -e

if [ $# -lt 1 ]; then
  echo "Usage: $0 <media-file> [short|medium|long]"
  exit 1
fi

FILE="$1"
LENGTH="${2:-medium}"

summarize "$FILE" --length "$LENGTH"
