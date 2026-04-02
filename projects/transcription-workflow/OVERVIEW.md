# Transcription Workflow

## Purpose

A simple workflow for turning audio/video/local media into transcript-like text or summaries that can then be cleaned, structured, and reused.

## Primary method

Use the `summarize` CLI as the first-line tool for local files and media when transcript extraction or summarization is needed.

## Default workflow

1. Put the media file path into the command
2. Run `summarize` on the local file
3. Start with a short or medium output first
4. If needed, expand or re-run with longer output
5. Clean the result into one of these formats:
   - raw transcript-like notes
   - structured summary
   - cleaned meeting notes
   - content draft

## Use cases

- voice notes
- meeting recordings
- lecture clips
- podcast/audio summaries
- turning spoken ideas into usable text

## Limitation

This workflow may produce a transcript, summary, or transcript-like extraction depending on the media and tool behavior. For exact word-perfect transcription, another dedicated ASR pipeline may still be better.
