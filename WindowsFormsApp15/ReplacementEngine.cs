using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReplacementEngin_Eslam
{
    // نوع نتیجه: موقت در حین دریافت داده‌ها یا نهایی بعد از FinishTask
    public enum ResultType { Intermediate, Final }
    // وضعیت اطمینان هر پارامتر؛ فقط Confirmed وارد گزارش نهایی می‌شود
    public enum ResolutionStatus { Unresolved, Candidate, Confirmed, Conflict }
    // نوع ورودی که به موتور می‌رسد: قالب گزارش، متن صدا یا خروجی OCR
    public enum InputKind { NormalText, VoiceText, OcrText }

    // نتیجه‌ای که موتور بعد از هر ورودی به برنامه فراخواننده برمی‌گرداند
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

    // یک پارامتر پزشکی که موتور آن را حل کرده است؛ مانند BPD یا Presentation
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

    // نسخه خام هر ورودی را برای پردازش مجدد نگه می‌داریم تا ترتیب ورود مهم نباشد
    internal sealed class InputEvidence
    {
        public long Sequence;
        public InputKind Kind;
        public string ImageId;
        public string RawText;
        public DateTime ReceivedAt;
    }

    // اندازه‌گیری استخراج‌شده از OCR به همراه خصوصیات وابسته مثل GA و EDD
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

    // مفهوم استخراج‌شده از Voice؛ می‌تواند عددی یا متنی/دسته‌ای باشد
    internal sealed class VoiceHint
    {
        public long Sequence;
        public string Parameter;
        public double? ExpectedValue;
        public string TextValue;
        public string RawText;
    }

    // تمام وضعیت یک گزارش بیمار در طول یک Task
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
    /// هسته اصلی موتور جایگزینی.
    /// ابتدا برای بیمار CreateTask فراخوانی می‌شود.
    /// سپس Template، Voice و OCR می‌توانند با هر ترتیبی و چند بار وارد شوند.
    /// ورودی‌های یک Task با lock سریال می‌شوند ولی Taskهای بیماران مختلف می‌توانند همزمان اجرا شوند.
    /// </summary>
    public sealed class ReplacementEngine
    {
        // نگهداری Taskهای فعال به صورت Thread-Safe
        private readonly ConcurrentDictionary<string, PatientTask> _tasks =
            new ConcurrentDictionary<string, PatientTask>(StringComparer.OrdinalIgnoreCase);
        private readonly MedicalDictionary _dictionary = new MedicalDictionary();

        // بعد از هر پردازش، نتیجه جدید از طریق این Event نیز اعلام می‌شود
        public event EventHandler<ReplacementResultEventArgs> ResultReady;

        // یک Task مستقل برای بیمار می‌سازد و TaskId یکتا برمی‌گرداند
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

        // متن عادی در حال حاضر نقش Template گزارش را دارد
        public ReplacementResult SendNormalText(string taskId, string text)
        {
            return AddAndProcess(taskId, InputKind.NormalText, null, text, false);
        }

        // متن خروجی Vosk یا هر Speech-to-Text دیگر را وارد موتور می‌کند
        public ReplacementResult SendVoiceText(string taskId, string text)
        {
            return AddAndProcess(taskId, InputKind.VoiceText, null, text, false);
        }

        // خروجی OCR یک تصویر را همراه شناسه تصویر وارد موتور می‌کند
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

        // تمام Evidenceهای جمع‌شده را از ابتدا تحلیل می‌کند، Resolve می‌کند و گزارش را می‌سازد
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

    // فرهنگ واژه‌های پزشکی و شکل‌های مختلفی که ممکن است Vosk تولید کند
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

    // متن Voice را نرمال و به Hintهای قابل فهم برای Resolver تبدیل می‌کند
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

        // تشخیص Presentation جنین از عبارت‌های فارسی/انگلیسی
        private static VoiceHint ParsePresentation(InputEvidence e, string text)
        {
            // اگر پزشک صریحاً گفته «سفالیک نیست»، مقدار Cephalic را ثبت نمی‌کنیم.
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

        // تشخیص جنسیت فقط وقتی متن، زمینه مرتبط با جنین/جنسیت داشته باشد
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

        // تشخیص محل جفت؛ بدون وجود کلمه جفت/پلاسنتا چیزی حدس زده نمی‌شود
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

        // یکسان‌سازی حروف عربی/فارسی، اعداد و فاصله‌ها قبل از مقایسه متن
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

    // خروجی PaddleOCR/متن OCR را به Measurementهای ساختاریافته تبدیل می‌کند
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

            // اندازه‌گیری‌های دارای نام صریح مثل BPD/HC/FHR قوی‌تر از اعداد بدون برچسب هستند
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

            // اندازه‌گیری Generic مثل D بدون نام پارامتر، به تنهایی قابل انتساب نیست
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

    // Evidenceهای OCR و Voice را با هم تطبیق می‌دهد و فقط موارد قابل تأیید را Confirmed می‌کند
    internal static class Resolver
    {
        public static List<ResolvedParameter> Resolve(List<Measurement> measurements, List<VoiceHint> hints,
            string template, List<string> diagnostics)
        {
            var output = new List<ResolvedParameter>();

            // مقادیر دسته‌ای که Voice صریحاً گفته؛ مثل Presentation، Sex و PlacentaPosition
            foreach (var h in hints.Where(x => !String.IsNullOrEmpty(x.TextValue)))
            {
                AddOrReplace(output, new ResolvedParameter {
                    Name=h.Parameter, Value=h.TextValue, Unit=String.Empty,
                    GestationalAge=String.Empty, GestationalAgeTolerance=String.Empty,
                    EstimatedDueDate=String.Empty, Status=ResolutionStatus.Confirmed,
                    Source="Voice#" + h.Sequence
                });
            }

            // اندازه‌گیری OCR که نام پارامتر را صریح دارد مستقیماً به همان پارامتر نسبت داده می‌شود
            foreach (var m in measurements.Where(x => x.IsLabeled))
            {
                AddOrReplace(output, new ResolvedParameter {
                    Name=m.Parameter, Value=m.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    Unit=m.Unit, GestationalAge=m.GestationalAge, GestationalAgeTolerance=m.GestationalAgeTolerance, EstimatedDueDate=m.EstimatedDueDate, Status=ResolutionStatus.Confirmed, Source=m.Source
                });
            }

            // عدد OCR بدون برچسب فقط وقتی تأیید می‌شود که دقیقاً یک VoiceHint عددی نزدیک داشته باشد
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

        // تلرانس تطبیق عدد Voice و OCR: حداقل 0.6 یا 8 درصد مقدار
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

    // پارامترهای Confirmed را داخل Placeholderهای Template جایگزین می‌کند
    internal static class TemplateRenderer
    {
        public static string Render(string template, IEnumerable<ResolvedParameter> parameters)
        {
            if (String.IsNullOrEmpty(template)) return String.Empty;
            string result = template;
            foreach (var p in parameters)
            {
                // مثال: {BPD.Value} ، {BPD.Unit} ، {BPD.GA} ، {BPD.EDD}
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
