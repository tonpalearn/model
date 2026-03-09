# Trader Forex MTA Project

เวอร์ชันพัฒนาปัจจุบันมี baseline implementation ของ cTrader/cAlgo cBot สำหรับ `XAUUSD` แล้ว โดยอิง setup หลักจากเอกสารวิเคราะห์:

- **Primary setup:** Breakout + Retest + Confirmation
- **Bias timeframe:** H1
- **Execution timeframe:** M15
- **Risk posture:** conservative / test-first

## โครงสร้างล่าสุด

```text
src/
  MtaGoldBreakoutRetestBot/
    MtaGoldBreakoutRetestBot.cs
SETUP.md
IMPLEMENTATION_MAP.md
DEV_NOTES.md
STRATEGY_SPEC.md
```

## เอกสารสำคัญ
- `STRATEGY_SPEC.md` — สเปกกลยุทธ์
- `SETUP.md` — วิธีนำเข้า/ตั้งค่าใน cTrader
- `IMPLEMENTATION_MAP.md` — mapping ระหว่าง spec กับโค้ด
- `DEV_NOTES.md` — assumptions, simplifications, จุดที่ควรถูก challenge

## ข้อจำกัดสำคัญ
- ยังไม่ได้ยืนยัน compile ใน cTrader environment ของเครื่องนี้
- ไม่อ้างผลกำไรหรือความสามารถทำกำไร
- ต้องให้ Test Agent ตรวจ logic, units, timezone, broker specifics และ backtest behavior ต่อ
