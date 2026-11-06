# ApiRedactor

A production-ready C# CLI tool built with .NET 8 in Visual Studio Code. The tool reads network traffic log files in NDJSON format, identifies sensitive information (such as email addresses, JWT tokens, and credit card numbers) based on predefined logic in a `rules.yaml` file, and outputs sanitized data while tracking redaction counts.

---

## Project Location & Overview

- **Location:** `C:\ApiRedactor`
- **Goal:** Mask sensitive PII and confidential keys in streaming NDJSON logs using YAML-driven rules.

---

## Project Structure & Roles

- **`rules.yaml`** – Contains the configurable rules for key matching and regex value masking.
- **`input.ndjson`** – Input file containing raw log entries (one JSON object per line).
- **`RulesConfig.cs`** – process and loads the `rules.yaml` file into C# models.
- **`LogProcessor.cs`** – Handles memory-efficient line-by-line streaming of `input.ndjson`.
- **`RedactionEngine.cs`** – Core engine containing the recursive JSON traversal and rule-matching logic.
- **`Program.cs`** – Entry point that ties all components together, routing redacted output to `stdout` and execution summaries to `stderr`.

---

## Key Functions Architecture

### `RulesConfig.cs`
* **`LoadFromFile`** – Loads and deserializes the YAML configuration into usable C# objects.

### `LogProcessor.cs`
* **`ProcessFile`** – Streams and processes the input log file line-by-line, forwarding each line to the redaction engine and outputting the sanitized line.

### `RedactionEngine.cs`
* **`RedactLine`** – Receives a raw JSON string line and returns the redacted string version.
* **`ProcessJsonNode`** – Recursively traverses JSON objects and arrays to locate target fields and replace their values with masking characters (`****`).
* **`ApplyValueRules`** – Applies regex patterns (`ValueRules`) to string values to mask embedded sensitive text.
* **`GetMatchedKeyRuleName`** – Matches key names against rules and tracks rule hit counts for summary reporting.

### `Program.cs`
* **`Main`** – Initializes configurations, triggers the streaming process, and manages output streams.
