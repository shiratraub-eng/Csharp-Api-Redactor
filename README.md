# Csharp-Api-Redactor
A C# CLI tool and lightweight service built with .NET 8 to securely redact PII and secrets from NDJSON logs using YAML rules.

---

```markdown
# C# NDJSON Redactor CLI

A C# CLI tool built with .NET 8 to redact sensitive data (PII/secrets) from NDJSON files based on rules defined in a YAML configuration file.

---

## 1. How to Build, Run, and Test

### Prerequisites
- .NET 8.0 SDK installed.

### Build
```bash
dotnet build

```

### Run (CLI Mode)

Place your `rules.yaml` and `input.ndjson` in the project directory, then run:

```bash
dotnet run

```

*The output redacted logs will be written to stdout/file, and the summary of redaction hits per rule will be printed to stderr.*

### Run (Service Mode)

If running as a web service:

```bash
dotnet run -- --serve --addr :8080 --rules rules.yaml

```

### Test

To run unit tests:

```bash
dotnet test

```

---

## 2. Design Decisions and Trade-offs

* **Recursion:** using a recursive function (`ProcessJsonNode`) to go deep into nested JSON objects and arrays. This ensures we don't miss sensitive fields inside sub-objects.
* **Streaming:** Instead of loading huge files into memory at once, It reads and processed the file line-by-line using `StreamReader`. This keeps memory usage low even for large log files.
* **Rule Matching:** splits rules into two types: `KeyRules` (checking field names like `password`) and `ValueRules` (checking string contents using Regex).
* **Failure Handling:** If a log line is malformed or bad JSON, it catches the error gracefully using `try-catch` so the app doesn't crash, and it makes sure sensitive data is never written to error logs.

---

## 3. JWT Rule Fix

* **The Issue:** Some JWT tokens were slipping through unredacted.
* **The Fix:** The original Regex pattern in `rules.yaml` was too strict about JWT structure and didn't catch tokens inside complex strings or headers. we updated the Regex pattern to properly match standard three-part base64 JWT tokens (`header.payload.signature`).

---

## 4. Deployment Files

* **Dockerfile:** Uses a multi-stage build (`build` stage to compile, `runtime` stage to run) to keep the final Docker container small and fast.
* **k8s-deployment.yaml:** Defines a Kubernetes Deployment and uses a `ConfigMap` for the `rules.yaml` file, so rules can be changed easily without rebuilding the app.

---

## 5. Known Limitations & Future Improvements

* Running multiple Regex patterns on every string can be slow on very large datasets.
* With more time, I would improve credit card validation with a full Luhn algorithm implementation to prevent false positives.

---

## 6. High-Performance Scaling (Sub-Millisecond Budget)

If this tool needed to process live traffic in under a millisecond per request, I would avoid building heavy JSON trees in memory for every line. Instead, I would use faster low-level parsers (like `Utf8JsonReader`) to scan bytes directly without allocating objects, reducing memory cleanup pauses (Garbage Collection) and boosting speed.

```

---

