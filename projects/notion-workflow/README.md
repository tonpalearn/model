# Notion Workflow

Reusable local workflow for updating the connected OpenClaw Notion workspace.

## Current connection

This workspace already has a working Notion API setup via:
- `.env.notion`
- `scripts/notion_setup_test_requests.py`
- `scripts/notion_append_blocks_requests.py`

The currently connected page is the PA Inbox page from `NOTION_PAGE_URL`.

## What this means

The assistant can already:
- verify Notion connectivity
- append markdown-based notes into the connected Notion page
- reuse this path for future meeting notes, templates, and working docs

## Default workflow

1. Prepare markdown content locally
2. Use the append script to push it into the connected Notion page
3. Reuse the same integration for future pages/notes

## Main scripts
- `scripts/notion_setup_test_requests.py`
- `scripts/notion_append_blocks_requests.py`
