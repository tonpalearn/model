# Commands

## 1. Verify Notion connection

```bash
cd /Users/ckawin/.openclaw/workspace
set -a && source .env.notion && set +a
python3 scripts/notion_setup_test_requests.py
```

## 2. Append a markdown file into the connected Notion page

```bash
cd /Users/ckawin/.openclaw/workspace
set -a && source .env.notion && set +a
python3 scripts/notion_append_blocks_requests.py /path/to/file.md
```

## 3. Typical usage flow

- write content into a local markdown file
- run append script
- confirm blocks appended successfully
