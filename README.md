# ReplacementEngin_Eslam

Real-time, task-based replacement engine for medical imaging reports (.NET Framework 4.6).

## Public API

The application normally uses only `ReplacementEngine` and `ReplacementResult`.

```csharp
var engine = new ReplacementEngine();

engine.ResultReady += (s, e) =>
{
    Console.WriteLine(e.Result.PatientId);
    Console.WriteLine(e.Result.TaskId);
    Console.WriteLine(e.Result.Type);
    Console.WriteLine(e.Result.ReportText);
};

string taskId = engine.CreateTask("57161");

// These three calls may arrive in ANY order and may be repeated:
engine.SendVoiceText(taskId, "مقدار آندومتر فرستادم 5 بود");
engine.SendOcrText(taskId, "IMG-1", "D 5.12 mm");
engine.SendNormalText(taskId, "ضخامت آندومتر {Endometrium.Value} {Endometrium.Unit} است.");

// Explicit finalization:
ReplacementResult finalResult = engine.FinishTask(taskId);
```

## Contract

- `CreateTask(patientId)` must be first.
- Normal text, voice text and OCR text may arrive in any order.
- Every input triggers an intermediate re-process and `ResultReady`.
- `FinishTask(taskId)` is the explicit request for the final result.
- Completed tasks reject further inputs.
- Each task is locked independently; different patient tasks can process concurrently.
- Raw evidence is retained for the lifetime of the task.
- Labeled OCR (BPD/FL/AC/HC/AFI/FHR/EFW) can be confirmed directly.
- Generic OCR distance (for example `D 5.12 mm`) is not assigned by number alone. It requires a unique compatible voice hint.
- Ambiguous evidence stays unresolved and is exposed through Diagnostics.
- Template placeholders support `{BPD.Value}`, `{BPD.Unit}`, `{BPD.GA}` and equivalent parameter names.

## Safety principle

The engine is conservative by design: unresolved/ambiguous evidence is not inserted into the report. Integration software should still provide clinical review before committing a medical report.

## Extending vocabulary

`MedicalDictionary` is currently internal and seeded with common aliases. It is intentionally isolated so it can later be replaced by a JSON/database-backed center/doctor-specific dictionary without changing the public API.
