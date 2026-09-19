using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReplacementEngin_Eslam
{
    public enum ResultType { Intermediate, Final }
    public enum ResolutionStatus { Unresolved, Candidate, Confirmed, Conflict }
    public enum InputKind { NormalText, VoiceText, OcrText }

    public sealed class ReplacementResult
    {
        public string PatientId { get; internal set; }
        public string TaskId { get; internal set; }
        public string ReportText { get; internal set; }
        public ResultType Type { get; internal set; }
        public bool HasUnresolvedItems { get; internal set; }
        public IList<ResolvedParameter> ConfirmedParameters { get; internal set; }
        public IList<string> Diagnostics { get; internal set; }
    }

    public sealed class ReplacementResultEventArgs : EventArgs
    {
        public ReplacementResult Result { get; private set; }
        public ReplacementResultEventArgs(ReplacementResult result) { Result = result; }
    }

    public sealed class ResolvedParameter
    {
        public string Name { get; internal set; }
        public string Value { get; internal set; }
        public string Unit { get; internal set; }
        public string GestationalAge { get; internal set; }
        public string GestationalAgeTolerance { get; internal set; }
        public string EstimatedDueDate { get; internal set; }
        public ResolutionStatus Status { get; internal set; }
        public string Source { get; internal set; }
    }

    internal sealed class InputEvidence
    {
        public long Sequence;
        public InputKind Kind;
        public string ImageId;
        public string RawText;
        public DateTime ReceivedAt;
    }

    internal sealed class Measurement
    {
        public long Sequence;
        public string Parameter;
        public double Value;
        public string Unit;
        public string GestationalAge;
        public string GestationalAgeTolerance;
        public string EstimatedDueDate;
        public string Source;
        public bool IsLabeled;
    }

    internal sealed class VoiceHint
    {
        public long Sequence;
        public string Parameter;
        public double? ExpectedValue;
        public string TextValue;
        public string RawText;
    }

    internal sealed class PatientTask
    {
        public readonly object Sync = new object();
        public string TaskId;
        public string PatientId;
        public string TemplateText;
        public bool Completed;
        public long Sequence;
        public readonly List<InputEvidence> Evidence = new List<InputEvidence>();
        public readonly List<ResolvedParameter> Confirmed = new List<ResolvedParameter>();
        public readonly List<string> Diagnostics = new List<string>();
        public string CurrentReport = String.Empty;
    }

    /// <summary>
    /// Public facade. CreateTask must be called first. Normal/Voice/OCR inputs may then arrive in any order.
    /// Each input is processed atomically per task. Different tasks may be processed concurrently.
    /// </summary>
    public sealed class ReplacementEngine
    {
        private readonly ConcurrentDictionary<string, PatientTask> _tasks =
            new ConcurrentDictionary<string, PatientTask>(StringComparer.OrdinalIgnoreCase);
        private readonly MedicalDictionary _dictionary = new MedicalDictionary();

        public event EventHandler<ReplacementResultEventArgs> ResultReady;

        public string CreateTask(string patientId)
        {
            if (String.IsNullOrWhiteSpace(patientId)) throw new ArgumentException("patientId is required.", "patientId");
            var task = new PatientTask
            {
                TaskId = Guid.NewGuid().ToString("N"),
                PatientId = patientId.Trim()
            };
            if (!_tasks.TryAdd(task.TaskId, task)) throw new InvalidOperationException("Could not create task.");
            return task.TaskId;
        }

        public ReplacementResult SendNormalText(string taskId, string text)
        {
            return AddAndProcess(taskId, InputKind.NormalText, null, text, false);
        }

        public ReplacementResult SendVoiceText(string taskId, string text)
        {
            return AddAndProcess(taskId, InputKind.VoiceText, null, text, false);
        }

        public ReplacementResult SendOcrText(string taskId, string imageId, string text)
        {
            return AddAndProcess(taskId, InputKind.OcrText, imageId, text, false);
        }

        public ReplacementResult GetCurrentResult(string taskId)
        {
            PatientTask task = GetTask(taskId);
            lock (task.Sync) return ProcessLocked(task, false);
        }

        public ReplacementResult FinishTask(string taskId)
        {
            PatientTask task = GetTask(taskId);
            ReplacementResult result;
            lock (task.Sync)
            {
                if (task.Completed) return BuildResult(task, ResultType.Final);
                result = ProcessLocked(task, true);
                task.Completed = true;
            }
            RaiseResult(result);
            return result;
        }

        public bool RemoveTask(string taskId)
        {
            PatientTask ignored;
            return _tasks.TryRemove(taskId, out ignored);
        }

        private ReplacementResult AddAndProcess(string taskId, InputKind kind, string imageId, string text, bool final)
        {
            if (text == null) throw new ArgumentNullException("text");
            PatientTask task = GetTask(taskId);
            ReplacementResult result;
            lock (task.Sync)
            {
                if (task.Completed) throw new InvalidOperationException("Task is completed.");
                task.Sequence++;
                task.Evidence.Add(new InputEvidence
                {
                    Sequence = task.Sequence, Kind = kind, ImageId = imageId,
                    RawText = text, ReceivedAt = DateTime.UtcNow
                });
                if (kind == InputKind.NormalText) task.TemplateText = text;
                result = ProcessLocked(task, final);
            }
            RaiseResult(result);
            return result;
        }

        private ReplacementResult ProcessLocked(PatientTask task, bool final)
        {
            task.Diagnostics.Clear();
            var measurements = new List<Measurement>();
            var hints = new List<VoiceHint>();

            foreach (var e in task.Evidence.OrderBy(x => x.Sequence))
            {
                if (e.Kind == InputKind.OcrText)
                    measurements.AddRange(OcrParser.Parse(e));
                else if (e.Kind == InputKind.VoiceText)
                    hints.AddRange(VoiceParser.Parse(e, _dictionary));
            }

            var resolved = Resolver.Resolve(measurements, hints, task.TemplateText, task.Diagnostics);
            task.Confirmed.Clear();
            task.Confirmed.AddRange(resolved.Where(x => x.Status == ResolutionStatus.Confirmed));
            task.CurrentReport = TemplateRenderer.Render(task.TemplateText, task.Confirmed);

            return BuildResult(task, final ? ResultType.Final : ResultType.Intermediate);
        }

        private static ReplacementResult BuildResult(PatientTask task, ResultType type)
        {
            return new ReplacementResult
            {
                PatientId = task.PatientId,
                TaskId = task.TaskId,
                ReportText = task.CurrentReport ?? String.Empty,
                Type = type,
                HasUnresolvedItems = task.Diagnostics.Any(x => x.StartsWith("UNRESOLVED:", StringComparison.Ordinal)),
                ConfirmedParameters = task.Confirmed.ToList().AsReadOnly(),
                Diagnostics = task.Diagnostics.ToList().AsReadOnly()
            };
        }

        private PatientTask GetTask(string taskId)
        {
            if (String.IsNullOrWhiteSpace(taskId)) throw new ArgumentException("taskId is required.", "taskId");
            PatientTask task;
            if (!_tasks.TryGetValue(taskId, out task)) throw new KeyNotFoundException("Task not found: " + taskId);
            return task;
        }

        private void RaiseResult(ReplacementResult result)
        {
            var handler = ResultReady;
            if (handler != null) handler(this, new ReplacementResultEventArgs(result));
        }
    }

    internal sealed class MedicalDictionary
    {
        private readonly Dictionary<string, string[]> _aliases =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "Endometrium", new[] { "آندومتر", "اندومتر", "اندومتریوم" } },
                { "RightKidney", new[] { "کلیه راست", "کلیه سمت راست" } },
                { "LeftKidney", new[] { "کلیه چپ", "کلیه سمت چپ" } },
                { "Spleen", new[] { "طحال" } },
                { "BPD", new[] { "bpd", "بی پی دی" } },
                { "FL", new[] { "fl", "اف ال" } },
                { "AC", new[] { "ac", "ای سی" } },
                { "HC", new[] { "hc", "اچ سی" } },
                { "AFI", new[] { "afi", "ای اف آی" } },
                { "FHR", new[] { "fhr", "ضربان قلب جنین" } },
                { "EFW", new[] { "efw", "وزن جنین", "وزن تقریبی جنین" } },
                { "Grade", new[] { "گرید", "گریت", "گریه" } }
            };

        public IEnumerable<KeyValuePair<string, string[]>> Entries { get { return _aliases; } }
    }

    internal static class VoiceParser
    {
        private static readonly Regex Number = new Regex(@"(?<!\d)(\d+(?:[\.,]\d+)?)", RegexOptions.Compiled);

        public static IEnumerable<VoiceHint> Parse(InputEvidence e, MedicalDictionary dictionary)
        {
            string text = NormalizeText(e.RawText ?? String.Empty);

            VoiceHint presentation = ParsePresentation(e, text);
            if (presentation != null) yield return presentation;

            VoiceHint fetalSex = ParseFetalSex(e, text);
            if (fetalSex != null) yield return fetalSex;

            VoiceHint placenta = ParsePlacentaPosition(e, text);
            if (placenta != null) yield return placenta;

            foreach (var entry in dictionary.Entries)
            {
                if (!entry.Value.Any(a => text.Contains(NormalizeText(a)))) continue;
                double? value = null;
                var m = Number.Match(text);
                double parsed;
                if (m.Success && Double.TryParse(m.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out parsed)) value = parsed;
                yield return new VoiceHint {
                    Sequence = e.Sequence, Parameter = entry.Key,
                    ExpectedValue = value, RawText = e.RawText
                };
            }
        }

        private static VoiceHint ParsePresentation(InputEvidence e, string text)
        {
            // Do not confirm explicit negation.
            if (ContainsAny(text, "سفالیک نیست", "سفاليک نیست", "cephalic نیست"))
                return null;

            if (ContainsAny(text, "سفالیک", "سفاليک", "cephalic"))
                return TextHint(e, "Presentation", "Cephalic");

            if (ContainsAny(text, "بریچ", "بريچ", "breech"))
                return TextHint(e, "Presentation", "Breech");

            if (ContainsAny(text, "ترانسورس", "عرضی", "عرضي", "transverse"))
                return TextHint(e, "Presentation", "Transverse");

            return null;
        }

        private static VoiceHint ParseFetalSex(InputEvidence e, string text)
        {
            if (ContainsAny(text, "جنسیت مشخص نیست", "جنسیت نامشخص", "جنسیت دیده نشد"))
                return null;

            bool sexContext = ContainsAny(text, "جنسیت", "جنس جنین", "جنین", "بچه");
            if (!sexContext) return null;

            if (ContainsAny(text, "دختر", "مونث", "مؤنث", "female"))
                return TextHint(e, "Sex", "Female");

            if (ContainsAny(text, "پسر", "مذکر", "male"))
                return TextHint(e, "Sex", "Male");

            return null;
        }

        private static VoiceHint ParsePlacentaPosition(InputEvidence e, string text)
        {
            if (!ContainsAny(text, "جفت", "پلاسنتا", "placenta")) return null;
            if (ContainsAny(text, "محل جفت مشخص نیست", "جفت مشخص نیست")) return null;

            if (ContainsAny(text, "قدامی", "قدامي", "انتریور", "anterior"))
                return TextHint(e, "PlacentaPosition", "Anterior");

            if (ContainsAny(text, "خلفی", "خلفي", "پوستریور", "posterior"))
                return TextHint(e, "PlacentaPosition", "Posterior");

            if (ContainsAny(text, "فوندال", "فوندوس", "fundal"))
                return TextHint(e, "PlacentaPosition", "Fundal");

            if (ContainsAny(text, "لترال", "جانبی", "جانبي", "lateral"))
                return TextHint(e, "PlacentaPosition", "Lateral");

            return null;
        }

        private static VoiceHint TextHint(InputEvidence e, string parameter, string value)
        {
            return new VoiceHint {
                Sequence = e.Sequence, Parameter = parameter,
                TextValue = value, RawText = e.RawText
            };
        }

        private static bool ContainsAny(string text, params string[] values)
        {
            foreach (string value in values)
                if (text.Contains(NormalizeText(value))) return true;
            return false;
        }

        private static string NormalizeText(string s)
        {
            s = NormalizeDigits(s).ToLowerInvariant();
            s = s.Replace('ي', 'ی').Replace('ك', 'ک');
            return Regex.Replace(s, @"\s+", " ").Trim();
        }

        private static string NormalizeDigits(string s)
        {
            const string fa = "۰۱۲۳۴۵۶۷۸۹";
            const string ar = "٠١٢٣٤٥٦٧٨٩";
            for (int i = 0; i < 10; i++)
                s = s.Replace(fa[i], (char)('0' + i)).Replace(ar[i], (char)('0' + i));
            return s;
        }
    }

    internal static class OcrParser
    {
        private static readonly Regex Labeled = new Regex(
            @"\b(BPD|FL|AC|HC|AFI|FHR|EFW)\b[^0-9]{0,40}(\d+(?:[\.,]\d+)?)\s*(mm|cm|g|bpm)?(?<tail>[^\r\n\]]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Ga = new Regex(
            @"\bGA\s*[:=]?\s*(\d{1,2}w\d{1,2}d)(?:\s*[±+/-]+\s*(\d{1,3}d))?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Edd = new Regex(
            @"\bEDD\s*[:=]?\s*(\d{4}[-/]\d{1,2}[-/]\d{1,2})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Generic = new Regex(
            @"(?:\b\d+\s*)?\bD\b[^0-9]{0,15}(\d+(?:[\.,]\d+)?)\s*(mm|cm)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static IEnumerable<Measurement> Parse(InputEvidence e)
        {
            var list = new List<Measurement>();
            string raw = NormalizeDigits(e.RawText ?? String.Empty);

            foreach (Match m in Labeled.Matches(raw))
            {
                double value;
                if (!Double.TryParse(m.Groups[2].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out value)) continue;

                string tail = m.Groups["tail"].Value;
                Match ga = Ga.Match(tail);
                Match edd = Edd.Match(tail);

                list.Add(new Measurement {
                    Sequence=e.Sequence, Parameter=m.Groups[1].Value.ToUpperInvariant(), Value=value,
                    Unit=m.Groups[3].Success ? m.Groups[3].Value : String.Empty,
                    GestationalAge=ga.Success ? ga.Groups[1].Value : String.Empty,
                    GestationalAgeTolerance=ga.Success && ga.Groups[2].Success ? "±" + ga.Groups[2].Value : String.Empty,
                    EstimatedDueDate=edd.Success ? edd.Groups[1].Value.Replace('/', '-') : String.Empty,
                    Source="OCR:" + (e.ImageId ?? e.Sequence.ToString()), IsLabeled=true
                });
            }

            foreach (Match m in Generic.Matches(raw))
            {
                double value;
                if (!Double.TryParse(m.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out value)) continue;
                list.Add(new Measurement {
                    Sequence=e.Sequence, Parameter="Unknown", Value=value,
                    Unit=m.Groups[2].Success ? m.Groups[2].Value : "mm",
                    Source="OCR:" + (e.ImageId ?? e.Sequence.ToString()), IsLabeled=false
                });
            }
            return list;
        }

        private static string NormalizeDigits(string s)
        {
            const string fa = "۰۱۲۳۴۵۶۷۸۹";
            const string ar = "٠١٢٣٤٥٦٧٨٩";
            for (int i = 0; i < 10; i++)
                s = s.Replace(fa[i], (char)('0' + i)).Replace(ar[i], (char)('0' + i));
            return s;
        }
    }

    internal static class Resolver
    {
        public static List<ResolvedParameter> Resolve(List<Measurement> measurements, List<VoiceHint> hints,
            string template, List<string> diagnostics)
        {
            var output = new List<ResolvedParameter>();

            // Categorical values explicitly stated by voice (for example fetal presentation).
            foreach (var h in hints.Where(x => !String.IsNullOrEmpty(x.TextValue)))
            {
                AddOrReplace(output, new ResolvedParameter {
                    Name=h.Parameter, Value=h.TextValue, Unit=String.Empty,
                    GestationalAge=String.Empty, GestationalAgeTolerance=String.Empty,
                    EstimatedDueDate=String.Empty, Status=ResolutionStatus.Confirmed,
                    Source="Voice#" + h.Sequence
                });
            }

            // Explicitly labeled machine measurements are strong evidence.
            foreach (var m in measurements.Where(x => x.IsLabeled))
            {
                AddOrReplace(output, new ResolvedParameter {
                    Name=m.Parameter, Value=m.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    Unit=m.Unit, GestationalAge=m.GestationalAge, GestationalAgeTolerance=m.GestationalAgeTolerance, EstimatedDueDate=m.EstimatedDueDate, Status=ResolutionStatus.Confirmed, Source=m.Source
                });
            }

            // Generic measurements require a unique voice target with a close numeric value.
            foreach (var m in measurements.Where(x => !x.IsLabeled))
            {
                var candidates = hints.Where(h => h.ExpectedValue.HasValue &&
                    NumericClose(h.ExpectedValue.Value, m.Value)).ToList();

                var distinctTargets = candidates.Select(x => x.Parameter).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (distinctTargets.Count == 1)
                {
                    var h = candidates.OrderBy(x => Math.Abs(x.ExpectedValue.Value - m.Value))
                                      .ThenBy(x => Math.Abs(x.Sequence - m.Sequence)).First();
                    AddOrReplace(output, new ResolvedParameter {
                        Name=h.Parameter, Value=m.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                        Unit=m.Unit, GestationalAge=m.GestationalAge, GestationalAgeTolerance=m.GestationalAgeTolerance, EstimatedDueDate=m.EstimatedDueDate, Status=ResolutionStatus.Confirmed,
                        Source=m.Source + " + Voice#" + h.Sequence
                    });
                }
                else
                {
                    diagnostics.Add("UNRESOLVED: " + m.Value.ToString("0.##",
                        System.Globalization.CultureInfo.InvariantCulture) + " " + m.Unit + " from " + m.Source);
                }
            }

            foreach (var h in hints.Where(x => x.ExpectedValue.HasValue))
            {
                if (!output.Any(x => x.Name.Equals(h.Parameter, StringComparison.OrdinalIgnoreCase)))
                    diagnostics.Add("UNRESOLVED: voice hint " + h.Parameter + " ~= " + h.ExpectedValue.Value);
            }
            return output;
        }

        private static bool NumericClose(double expected, double actual)
        {
            double tolerance = Math.Max(0.6, Math.Abs(expected) * 0.08);
            return Math.Abs(expected - actual) <= tolerance;
        }

        private static void AddOrReplace(List<ResolvedParameter> list, ResolvedParameter value)
        {
            int i = list.FindIndex(x => x.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase));
            if (i >= 0) list[i] = value; else list.Add(value);
        }
    }

    internal static class TemplateRenderer
    {
        public static string Render(string template, IEnumerable<ResolvedParameter> parameters)
        {
            if (String.IsNullOrEmpty(template)) return String.Empty;
            string result = template;
            foreach (var p in parameters)
            {
                result = Replace(result, p.Name + ".Value", p.Value);
                result = Replace(result, p.Name + ".Unit", p.Unit);
                result = Replace(result, p.Name + ".GA", p.GestationalAge);
                result = Replace(result, p.Name + ".GATolerance", p.GestationalAgeTolerance);
                result = Replace(result, p.Name + ".EDD", p.EstimatedDueDate);
                result = Replace(result, p.Name, p.Value);
            }
            return result;
        }

        private static string Replace(string text, string key, string value)
        {
            return text.Replace("{" + key + "}", value ?? String.Empty);
        }
    }
}
