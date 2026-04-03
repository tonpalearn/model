#!/usr/bin/env python3
import json, os, re, sys, certifi, requests
TOKEN = os.environ.get('NOTION_TOKEN')
PAGE_URL = os.environ.get('NOTION_PAGE_URL', '')
NOTION_VERSION = '2022-06-28'
if len(sys.argv) < 2:
    print('Usage: notion_append_blocks_requests.py <markdown-file>', file=sys.stderr); sys.exit(2)
md_path = sys.argv[1]
if not TOKEN:
    print('Missing NOTION_TOKEN', file=sys.stderr); sys.exit(2)
matches = re.findall(r'([0-9a-fA-F]{32})', PAGE_URL)
if not matches:
    print('Could not parse page id from NOTION_PAGE_URL', file=sys.stderr); sys.exit(2)
raw = matches[-1].lower()
page_id = f"{raw[0:8]}-{raw[8:12]}-{raw[12:16]}-{raw[16:20]}-{raw[20:32]}"
text = open(md_path, 'r', encoding='utf-8').read().splitlines()
children=[]
for line in text:
    s=line.rstrip()
    if not s:
        continue
    if s.startswith('# '):
        children.append({"object":"block","type":"heading_1","heading_1":{"rich_text":[{"type":"text","text":{"content":s[2:]}}]}})
    elif s.startswith('## '):
        children.append({"object":"block","type":"heading_2","heading_2":{"rich_text":[{"type":"text","text":{"content":s[3:]}}]}})
    elif s.startswith('- '):
        children.append({"object":"block","type":"bulleted_list_item","bulleted_list_item":{"rich_text":[{"type":"text","text":{"content":s[2:]}}]}})
    else:
        children.append({"object":"block","type":"paragraph","paragraph":{"rich_text":[{"type":"text","text":{"content":s[:1900]}}]}})
for chunk_i in range(0, len(children), 100):
    chunk = children[chunk_i:chunk_i+100]
    r = requests.patch(f'https://api.notion.com/v1/blocks/{page_id}/children',
        headers={
            'Authorization': f'Bearer {TOKEN}',
            'Notion-Version': NOTION_VERSION,
            'Content-Type': 'application/json',
        },
        json={"children": chunk}, timeout=30, verify=certifi.where())
    if r.status_code >= 300:
        print(r.text, file=sys.stderr)
        sys.exit(1)
print(json.dumps({'ok': True, 'appended_blocks': len(children)}, ensure_ascii=False))
