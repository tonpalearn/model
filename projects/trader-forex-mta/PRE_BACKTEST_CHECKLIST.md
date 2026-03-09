# PRE_BACKTEST_CHECKLIST

## ก่อนเชื่อผล backtest ต้องเช็ก
- [ ] bot compile ได้ใน cTrader จริง
- [ ] volume sizing ไม่เพี้ยนกับ broker
- [ ] timezone / trading day offset ชัดเจน
- [ ] symbol spec ของ XAUUSD ถูกต้อง
- [ ] log อ่านแล้วสอดคล้องกับ logic
- [ ] ไม่มี runtime error เงียบๆ
- [ ] trade เกิดจาก setup logic จริง ไม่ใช่ bug
- [ ] stop-loss / take-profit หน่วยถูกต้อง
- [ ] break-even ทำงานอย่างที่ตั้งใจ
- [ ] daily loss cap อิงต้นวันจริง

## ก่อน optimization
- [ ] baseline run ก่อน
- [ ] แยก in-sample / out-of-sample
- [ ] ระวัง overfitting
- [ ] อย่าปรับ parameter จากช่วงเดียว

## ก่อนเชื่อว่าระบบดี
- [ ] ดูจำนวนเทรดพอหรือยัง
- [ ] ดู expectancy ไม่ใช่แค่กำไรรวม
- [ ] ดู drawdown
- [ ] ดู performance แยก session
- [ ] ดูหลายช่วงเวลา ไม่ใช่แค่ช่วงเดียว
