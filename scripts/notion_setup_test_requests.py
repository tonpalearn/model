#!/usr/bin/env python3
import json, os, re, sys, certifi, requests
TOKEN = os.environ.get('NOTION_TOKEN')
PAGE_URL = os.environ.get('NOTION_PAGE_URL', '')
NOTION_VERSION = '2022-06-28'
if not TOKEN:
    print('Missing NOTION_TOKEN', file=sys.stderr); sys.exit(2)
m = re.search(r'([0-9a-fA-F]{32})', PAGE_URL.replace('-', ''))
if not m:
    print('Could not parse page id from NOTION_PAGE_URL', file=sys.stderr); sys.exit(2)
raw = m.group(1).lower()
page_id = f"{raw[0:8]}-{raw[8:12]}-{raw[12:16]}-{raw[16:20]}-{raw[20:32]}"
r = requests.get(f'https://api.notion.com/v1/pages/{page_id}', headers={
    'Authorization': f'Bearer {TOKEN}',
    'Notion-Version': NOTION_VERSION,
    'Content-Type': 'application/json',
}, timeout=30, verify=certifi.where())
print(r.status_code)
print(r.text[:2000])
