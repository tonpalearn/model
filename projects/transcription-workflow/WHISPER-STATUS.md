# Whisper Status

## Result

Whisper is installed and working on this machine.

Detected:
- `whisper` CLI available
- `ffmpeg` available
- Homebrew package `openai-whisper` installed

## Test

Tested successfully on a real inbound `.ogg` voice message.

### Example transcript result

`เดี๋ยวผมจะมีประชุมเอเจ้น แอมเวช่วยคิดให้หน่อยว่าจะคุยหัวข้ออะไรดีวันนี้`

## Notes

- Running on CPU, so performance may be slower than GPU setups.
- `--model turbo` is a good default for practical use.
- For Thai audio, specify `--language th` when appropriate.
