#!/usr/bin/env python3
import json
import subprocess
from datetime import datetime, timedelta
from pathlib import Path
from zoneinfo import ZoneInfo

TZ = ZoneInfo('Asia/Bangkok')
ACCOUNT = 'ckawin1184@gmail.com'
GOG = '/opt/homebrew/bin/gog'
OPENCLAW = '/usr/local/bin/openclaw'
STATE_PATH = Path('/Users/ckawin/.openclaw/workspace/memory/heartbeat-state.json')
TASKS_PATH = Path('/Users/ckawin/.openclaw/workspace/memory/open-tasks.md')
TARGET = '6271498929'


def run(cmd):
    return subprocess.check_output(cmd, text=True)


def load_state():
    if not STATE_PATH.exists():
        return {"lastChecks": {}, "preferences": {"taskFollowupHours": 4}}
    return json.loads(STATE_PATH.read_text())


def save_state(state):
    STATE_PATH.write_text(json.dumps(state, ensure_ascii=False, indent=2) + "\n")


def fetch_events(hours_ahead=24):
    now = datetime.now(TZ)
    end = now + timedelta(hours=hours_ahead)
    out = run([GOG, 'calendar', 'events', 'primary', '--account', ACCOUNT, '--from', now.isoformat(), '--to', end.isoformat(), '--json'])
    return json.loads(out).get('events', [])


def parse_dt(ev, key):
    val = ev.get(key, {}).get('dateTime')
    return datetime.fromisoformat(val) if val else None


def fmt(dt):
    return dt.astimezone(TZ).strftime('%H:%M')


def load_now_tasks():
    if not TASKS_PATH.exists():
        return []
    lines = TASKS_PATH.read_text().splitlines()
    tasks, in_now = [], False
    for line in lines:
        if line.strip() == '## Now':
            in_now = True
            continue
        if in_now and line.startswith('## '):
            break
        if in_now and line.strip().startswith('- '):
            task = line.strip()[2:].strip()
            if task and task != '_ยังไม่มี_':
                tasks.append(task)
    return tasks


def build_alert():
    now = datetime.now(TZ)
    state = load_state()
    pref_hours = state.get('preferences', {}).get('taskFollowupHours', 4)
    last_task_review = state.get('lastChecks', {}).get('taskReview')
    due_for_task_review = True
    if last_task_review:
        due_for_task_review = (now.timestamp() - last_task_review) >= pref_hours * 3600

    soon_events = []
    for ev in fetch_events(24):
        start = parse_dt(ev, 'start')
        if not start:
            continue
        delta = start - now
        mins = delta.total_seconds() / 60
        if 0 <= mins <= 180:
            soon_events.append((start, ev))
    soon_events.sort(key=lambda x: x[0])

    tasks = load_now_tasks()
    lines = []
    if soon_events:
        lines.append('⏰ เตือนงาน/นัดใกล้ถึง')
        for start, ev in soon_events[:3]:
            summary = ev.get('summary', '(ไม่มีชื่อ)')
            loc = ev.get('location')
            lines.append(f"- {fmt(start)} {summary}" + (f" @ {loc}" if loc else ''))
    if due_for_task_review and tasks:
        if lines:
            lines.append('')
        lines.append('📌 งานที่ยังค้างในโฟกัสตอนนี้')
        for t in tasks[:5]:
            lines.append(f'- {t}')

    if not lines:
        return None, state

    state.setdefault('lastChecks', {})['taskReview'] = int(now.timestamp())
    state['lastChecks']['calendar'] = int(now.timestamp())
    return '\n'.join(lines), state


def send(msg):
    subprocess.check_call([OPENCLAW, 'message', 'send', '--channel', 'telegram', '--target', TARGET, '--message', msg])


if __name__ == '__main__':
    msg, state = build_alert()
    if msg:
        send(msg)
        save_state(state)
