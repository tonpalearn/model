# Commands

## 1. Short summary from a local file

```bash
summarize "/path/to/file.ogg" --length short
```

## 2. Medium summary from a local file

```bash
summarize "/path/to/file.ogg" --length medium
```

## 3. JSON output

```bash
summarize "/path/to/file.ogg" --length medium --json
```

## 4. Save output to a file

```bash
summarize "/path/to/file.ogg" --length medium > output.txt
```

## 5. Rework extracted text into cleaner notes

After extraction, use the assistant to transform it into:
- cleaned transcript
- summary bullets
- meeting notes
- article draft
- talking points
