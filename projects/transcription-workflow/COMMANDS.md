# Commands

## 1. Direct transcription with Whisper

```bash
whisper "/path/to/file.ogg" --model turbo --language th --task transcribe --output_format txt --output_dir ./out
```

## 2. Helper script

```bash
bash scripts/transcribe_local.sh "/path/to/file.ogg" medium
```

## 3. Quick summary with summarize

```bash
summarize "/path/to/file.ogg" --length medium
```

## 4. Save Whisper output into a folder

```bash
mkdir -p out && whisper "/path/to/file.ogg" --model turbo --language th --task transcribe --output_format txt --output_dir ./out
```

## 5. Rework extracted text into cleaner notes

After extraction, use the assistant to transform it into:
- cleaned transcript
- summary bullets
- meeting notes
- article draft
- talking points
