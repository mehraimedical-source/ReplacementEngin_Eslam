using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Smart_Report
{
    public class OcrBlock
    {
        public string Text;
        public string StartKeyword;
        public int StartIndex;
        public int EndIndex;

        // یک بلوک خالی می‌سازد.
        // دلیل: مقدار -1 برای Indexها یعنی هنوز محدوده واقعی از OCR به این بلوک داده نشده است؛
        // در نتیجه با بلوکی که واقعاً از کاراکتر صفر شروع شده اشتباه نمی‌شود.
        // مثال: StartIndex=0 یعنی متن واقعاً از ابتدای OCR آمده، ولی -1 یعنی هنوز مقداردهی نشده است.
        public OcrBlock()
        {
            Text = "";
            StartKeyword = "";
            StartIndex = -1;
            EndIndex = -1;
        }

        // نمایش Block برای Debug است. Raw Text تغییر نمی‌کند و فقط هنگام نمایش [] اضافه می‌شود.
        public override string ToString()
        {
            return "[" + Text + "]";
        }
    }

    public class OcrBlockBuilder
    {
        private OcrBlockRules _rules;

        private class BlockStart
        {
            public int Index;
            public string Keyword;
        }

        // Builder را با Ruleهای پیش‌فرض و قابل ویرایش برنامه می‌سازد.
        public OcrBlockBuilder()
            : this(OcrBlockRules.CreateDefault())
        {
        }

        // Builder را با Ruleهای داده‌شده می‌سازد. Ruleها Clone می‌شوند تا تغییر بیرونی رفتار Engine را ناخواسته عوض نکند.
        public OcrBlockBuilder(OcrBlockRules rules)
        {
            SetRules(rules);
        }

        // Ruleهای فعال Block Starter را عوض می‌کند. اگر null باشد Default Rules استفاده می‌شود.
        public void SetRules(OcrBlockRules rules)
        {
            _rules = (rules == null) ? OcrBlockRules.CreateDefault() : rules.Clone();
        }

        // تابع اصلی ساخت بلوک‌هاست.
        // مراحل: 1) یکدست کردن فاصله‌های OCR، 2) پیدا کردن تمام Startهای معتبر،
        // 3) حفظ متن ناشناخته قبل و بین Startها، 4) بریدن متن به بلوک‌ها.
        // دلیل این طراحی: در این مرحله فقط Boundary مهم است، نه اینکه متن برای Report مفید است یا نه.
        // مثال: متن Header یا تنظیمات دستگاه ممکن است هنوز معنی بالینی نداشته باشد، ولی حذف نمی‌شود.
        // اصل مهم Zero Data Loss: هیچ قسمت Raw OCR فقط به دلیل ناشناخته بودن دور ریخته نمی‌شود.
        public List<OcrBlock> Build(string rawOcrText)
        {
            List<OcrBlock> blocks = new List<OcrBlock>();

            if (String.IsNullOrEmpty(rawOcrText))
                return blocks;

            string text = Normalize(rawOcrText);
            List<BlockStart> starts = FindBlockStarts(text);

            // اصل Zero Data Loss: اگر Block Starter پیدا نشد، کل Raw OCR حفظ می‌شود.
            if (starts.Count == 0)
            {
                AddBlock(blocks, text, "", 0, text.Length);
                return blocks;
            }

            // متن قبل از اولین Block Starter هم حفظ می‌شود؛ Header و Device Text نباید حذف شوند.
            if (starts[0].Index > 0)
                AddBlock(blocks, text, "", 0, starts[0].Index);

            for (int i = 0; i < starts.Count; i++)
            {
                int startIndex = starts[i].Index;
                int endExclusive = (i + 1 < starts.Count)
                    ? starts[i + 1].Index
                    : text.Length;

                if (endExclusive <= startIndex)
                    continue;

                string blockText = text.Substring(
                    startIndex,
                    endExclusive - startIndex).Trim();

                if (blockText.Length == 0)
                    continue;

                AddBlock(
                    blocks,
                    text,
                    starts[i].Keyword,
                    startIndex,
                    endExclusive);
            }

            return blocks;
        }

        // یک Range دقیق را به OcrBlock تبدیل می‌کند؛ Range خالی رد می‌شود ولی Raw Text غیرخالی حفظ می‌شود.
        private void AddBlock(
            List<OcrBlock> blocks,
            string text,
            string startKeyword,
            int startIndex,
            int endExclusive)
        {
            if (endExclusive <= startIndex)
                return;

            string blockText = text.Substring(
                startIndex,
                endExclusive - startIndex).Trim();

            if (blockText.Length == 0)
                return;

            OcrBlock block = new OcrBlock();
            block.Text = blockText;
            block.StartKeyword = startKeyword;
            block.StartIndex = startIndex;
            block.EndIndex = endExclusive - 1;
            blocks.Add(block);
        }

        // خروجی UI و Regression Test را می‌سازد؛ [] فقط نمایش Boundary است و Raw OCR را اصلاح نمی‌کند.
        public string BuildBracketText(string rawOcrText)
        {
            List<OcrBlock> blocks = Build(rawOcrText);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < blocks.Count; i++)
            {
                if (i > 0)
                    result.Append(Environment.NewLine);

                result.Append("[");
                result.Append(blocks[i].Text);
                result.Append("]");
            }

            return result.ToString();
        }

        // فقط فاصله‌ها، Tab و Enterهای OCR را یکدست می‌کند تا Regexها روی متن پایدار باشند.
        // دلیل: Block Builder نباید خودش OCR را "تصحیح" کند؛ وظیفه‌اش تشخیص مرزهاست.
        // مثال: "Fetaļ" به "Fetal" تبدیل نمی‌شود؛ همان Raw باقی می‌ماند و Rule مخصوص آن را می‌شناسد.
        private string Normalize(string text)
        {
            text = text.Replace("\r", " ");
            text = text.Replace("\n", " ");
            text = text.Replace("\t", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        // Boundaryهای احتمالی را از Editable Rules و Patternهای OCR مثل FHR، NT، NB و D جمع می‌کند؛ سپس Duplicate/Overlap را حذف می‌کند.
        private List<BlockStart> FindBlockStarts(string text)
        {
            List<BlockStart> found = new List<BlockStart>();

            for (int i = 0; i < _rules.Sections.Count; i++)
                FindKeyword(text, _rules.Sections[i], found, true);

            // OCR/report variants that cannot be represented safely as plain
            // unconditional keywords in the editable starter lists.
            FindBracketedOb(text, found);
            FindReportSectionVariant(text, "Fetal Biom", "Fetal Biometry", found);
            FindReportSectionVariant(text, "Fetal Long", "Fetal Long Bones", found);
            FindAuaValue(text, found);
            FindCompactRatio(text, "FL/AC", found);

            for (int i = 0; i < _rules.Parameters.Count; i++)
                FindKeyword(text, _rules.Parameters[i], found, true);

            for (int i = 0; i < _rules.Measurements.Count; i++)
            {
                // Fetal HR ممکن است مثل Fetal HR158-bpm بدون فاصله OCR شود؛ بنابراین Pattern اختصاصی دارد.
                if (String.Compare(_rules.Measurements[i], "Fetal HR", true) == 0)
                    FindFetalHeartRate(text, found);
                else
                    FindKeyword(text, _rules.Measurements[i], found, false);
            }

            // Conditional measurements: these labels are also used as presets
            // or anatomy text, so they start a block only when a numeric value
            // and a measurement unit immediately follow them.
            FindConditionalMeasurement(text, "NT", found);
            FindConditionalMeasurement(text, "NB", found);
            FindConditionalMeasurement(text, "D", found);
            FindConditionalHeartRate(text, found);

            found.Sort(delegate(BlockStart a, BlockStart b)
            {
                int byIndex = a.Index.CompareTo(b.Index);
                if (byIndex != 0)
                    return byIndex;

                // در Index یکسان، Keyword اختصاصی‌تر و طولانی‌تر اولویت دارد.
                return b.Keyword.Length.CompareTo(a.Keyword.Length);
            });

            List<BlockStart> unique = new List<BlockStart>();

            for (int i = 0; i < found.Count; i++)
            {
                if (unique.Count == 0)
                {
                    unique.Add(found[i]);
                    continue;
                }

                BlockStart previous = unique[unique.Count - 1];

                // در Start Position یکسان فقط Match اختصاصی‌تر نگه داشته می‌شود تا Duplicate Block نسازیم.
                if (previous.Index == found[i].Index)
                    continue;

                // Keyword کوتاه داخل Match بلندتر نباید False Block ایجاد کند.
                int previousEnd = previous.Index + previous.Keyword.Length;
                if (found[i].Index < previousEnd)
                    continue;

                unique.Add(found[i]);
            }

            return unique;
        }

        // Section گزارش را در شکل [OB] پیدا می‌کند و مرز را از خود '[' شروع می‌کند.
        // دلیل: می‌خواهیم Raw عبارت [OB] کامل داخل یک بلوک بماند.
        // مثال: "... T [OB] AUA 17w5d" -> [OB] باید Section مستقل باشد.
        // اما هر OB یک Section نیست؛ مثال "3 Trim./OB" بخشی از Preset دستگاه است.
        // همچنین Match معمولی OB داخل [OB] جداگانه غیرفعال می‌شود تا دو مرز روی یک عبارت نسازیم.
        private void FindBracketedOb(string text, List<BlockStart> result)
        {
            MatchCollection matches = Regex.Matches(text, @"\[\s*OB\s*\]", RegexOptions.IgnoreCase);
            for (int i = 0; i < matches.Count; i++)
            {
                BlockStart start = new BlockStart();
                start.Index = matches[i].Index;
                start.Keyword = matches[i].Value;
                result.Add(start);
            }
        }

        // عنوان Sectionهایی را پیدا می‌کند که OCR انتهای آنها را ناقص خوانده است.
        // مثال: "Fetal Biom..." به جای Fetal Biometry و "Fetal Long..." برای Fetal Long Bones.
        // دلیل استفاده از Pattern مخصوص: اگر فقط "Fetal Biom" را Keyword عمومی کنیم، ممکن است
        // هر متن دیگری که با این Prefix شروع شده اشتباهاً Section جدید ایجاد کند.
        private void FindReportSectionVariant(string text, string prefix, string keyword, List<BlockStart> result)
        {
            string pattern = @"(?<![A-Z0-9])" + Regex.Escape(prefix) + @"(?:etry|etry\.\.\.|\.\.\.|[A-Za-z]*\.\.\.)";
            MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            for (int i = 0; i < matches.Count; i++)
            {
                BlockStart start = new BlockStart();
                start.Index = matches[i].Index;
                start.Keyword = keyword;
                result.Add(start);
            }
        }

        // AUA را فقط وقتی مرز گزارش می‌داند که بلافاصله مقدار سن بارداری داشته باشد.
        // مثال: "AUA 17w5d" -> شروع بلوک معتبر است.
        // دلیل: AUA تنها را به Keyword عمومی تبدیل نمی‌کنیم تا هر حضور اتفاقی آن Split ایجاد نکند.
        private void FindAuaValue(string text, List<BlockStart> result)
        {
            MatchCollection matches = Regex.Matches(
                text,
                @"(?<![A-Z0-9])AUA\s+[0-9]+w[0-9]+d(?![A-Z0-9])",
                RegexOptions.IgnoreCase);

            for (int i = 0; i < matches.Count; i++)
            {
                BlockStart start = new BlockStart();
                start.Index = matches[i].Index;
                start.Keyword = "AUA";
                result.Add(start);
            }
        }

        // Ratio را وقتی پیدا می‌کند که OCR متن قبلی جدول را بدون فاصله به آن چسبانده باشد.
        // دلیل: Rule معمولی FL/AC وقتی قبلش متن چسبیده باشد ممکن است مرز درست را نبیند.
        // مثال واقعی: "5,255FL/AC 21.16 %" -> مرز باید دقیقاً از FL/AC شروع شود و
        // مقدار "5,255" حذف نشود؛ آن مقدار در بلوک قبلی باقی می‌ماند (Zero Data Loss).
        // اگر FL/AC معمولی و با فاصله باشد، همان Rule قابل ویرایش Measurements کافی است.
        private void FindCompactRatio(string text, string keyword, List<BlockStart> result)
        {
            MatchCollection matches = Regex.Matches(
                text,
                Regex.Escape(keyword) + @"\s+[+-]?[0-9]+(?:[\.,][0-9]+)?\s*%",
                RegexOptions.IgnoreCase);

            for (int i = 0; i < matches.Count; i++)
            {
                int index = matches[i].Index;

                // Normal separated ratio is already handled by the editable
                // measurement rule. This helper is only for OCR-attached text
                // such as "5,255FL/AC 21.16 %".
                if (index == 0 || Char.IsWhiteSpace(text[index - 1]))
                    continue;

                BlockStart start = new BlockStart();
                start.Index = index;
                start.Keyword = keyword;
                result.Add(start);
            }
        }

        // کل اندازه‌گیری Fetal HR را پیدا می‌کند، نه فقط کلمه HR را.
        // دلیل: OCR ممکن است خود عبارت Fetal را خراب کند، ولی ساختار "HR + عدد + bpm"
        // مدرک قوی‌ای است که این عبارت واقعاً ضربان قلب جنین است.
        // مثال‌های واقعی که باید یک بلوک کامل شوند:
        //   Fetal HR158-bpm       -> حالت بدون فاصله و با خط تیره
        //   Fetal HR 143 143 bpm  -> جدول گزارش عدد را دوبار تکرار کرده است
        //   Fetaļ HR158-bpm       -> OCR حرف l را به ļ تبدیل کرده است
        //   Feta HR158-bpm        -> OCR حرف آخر l را حذف کرده است
        // نکته مهم: متن Raw اصلاح نمی‌شود؛ مثلاً Fetaļ همان Fetaļ داخل خروجی باقی می‌ماند.
        // فقط برای تشخیص مرز بلوک، این شکل‌ها معادل ساختاری Fetal HR در نظر گرفته می‌شوند.
        private void FindFetalHeartRate(string text, List<BlockStart> result)
        {
            string pattern =
                @"(?<![A-Z0-9])Feta(?:l|ļ)?\s+HR\s*[+-]?[0-9]+(?:[\.,][0-9]+)?(?:\s+[+-]?[0-9]+(?:[\.,][0-9]+)?)?\s*-?\s*(?:bpm)(?![A-Z])";

            MatchCollection matches = Regex.Matches(
                text,
                pattern,
                RegexOptions.IgnoreCase);

            for (int i = 0; i < matches.Count; i++)
            {
                BlockStart start = new BlockStart();
                start.Index = matches[i].Index;
                start.Keyword = "Fetal HR";
                result.Add(start);
            }
        }

        // HR عمومی را فقط وقتی Measurement می‌داند که بعد از آن "عدد + bpm" وجود داشته باشد.
        // دلیل: کلمه HR به تنهایی قابل اعتماد نیست و در تنظیمات دستگاه هم دیده شده است.
        // مثال صحیح: "HR 147 bpm" -> باید بلوک جدید بسازد.
        // مثال غلط: "32Hz HR TIs 0.1" -> نباید از HR جدا شود، چون bpm و مقدار HR ندارد.
        // اگر HR داخل "Fetal/Fetaļ/Feta HR ... bpm" باشد نیز این تابع آن را رد می‌کند؛
        // چون FindFetalHeartRate باید کل عبارت را از ابتدای Fetal/Fetaļ/Feta جدا کند.
        // وگرنه خروجی غلطی مثل [Fetaļ] [HR158-bpm] ساخته می‌شود.
        private void FindConditionalHeartRate(string text, List<BlockStart> result)
        {
            MatchCollection matches = Regex.Matches(
                text,
                @"(?<![A-Z0-9])HR\s*[+-]?[0-9]+(?:[\.,][0-9]+)?\s*-?\s*bpm(?![A-Z])",
                RegexOptions.IgnoreCase);

            for (int i = 0; i < matches.Count; i++)
            {
                int index = matches[i].Index;
                string before = text.Substring(Math.Max(0, index - 8), Math.Min(8, index));
                if (Regex.IsMatch(before, @"Feta(?:l|ļ)?\s+$", RegexOptions.IgnoreCase))
                    continue;

                BlockStart start = new BlockStart();
                start.Index = index;
                start.Keyword = "HR";
                result.Add(start);
            }
        }

        // Measurementهای کوتاه و مبهم مثل NT، NB و D را به صورت شرطی پیدا می‌کند.
        // دلیل: خود Keyword به تنهایی کافی نیست؛ همان حروف ممکن است در Preset یا متن دستگاه باشند.
        // شرط فعلی: بعد از Keyword باید مقدار عددی و واحد mm یا cm بیاید.
        // مثال: "NT 2.32 mm" -> Measurement واقعی و شروع بلوک است.
        // مثال: "NT/CA1-7S/14.0cm" -> نام Preset دستگاه است و نباید از NT جدا شود.
        // مثال: "NB 2.66 mm" -> Measurement واقعی است.
        // برای D یک خطای OCR واقعی هم دیده‌ایم: "D.4.10 mm"؛ بنابراین فقط D اجازه دارد
        // به جای فاصله، نقطه بین D و مقدار داشته باشد. Raw OCR همچنان دست‌نخورده می‌ماند.
        // این منطق را نباید به هر "کلمه + عدد + واحد" تعمیم داد؛ چون مواردی مثل
        // SV 2.0mm، SVD 6.4cm، PRF و WF تنظیمات Doppler هستند و لزوماً Block Starter نیستند.
        private void FindConditionalMeasurement(string text, string keyword, List<BlockStart> result)
        {
            string pattern =
                @"(?<![A-Z0-9])" +
                Regex.Escape(keyword) +
                ((keyword == "D") ? @"(?:\s+|\.)" : @"\s+") +
                @"[+-]?[0-9]+(?:[\.,][0-9]+)?\s*(?:mm|cm)(?![A-Z])";

            MatchCollection matches = Regex.Matches(
                text,
                pattern,
                RegexOptions.IgnoreCase);

            for (int i = 0; i < matches.Count; i++)
            {
                BlockStart start = new BlockStart();
                start.Index = matches[i].Index;
                start.Keyword = keyword;
                result.Add(start);
            }
        }

        // Generic matcher for the centralized editable Section/Parameter/Measurement
        // lists. Word boundaries prevent matching inside larger alphanumeric words;
        // IsRealBlockStart then applies context guards for known ambiguous cases.
        private void FindKeyword(
            string text,
            string keyword,
            List<BlockStart> result,
            bool independent)
        {
            string pattern =
                @"(?<![A-Z0-9])" +
                Regex.Escape(keyword) +
                @"(?![A-Z0-9])";

            MatchCollection matches = Regex.Matches(
                text,
                pattern,
                RegexOptions.IgnoreCase);

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];

                if (!IsRealBlockStart(
                    text,
                    match.Index,
                    keyword,
                    independent))
                {
                    continue;
                }

                BlockStart start = new BlockStart();
                start.Index = match.Index;
                start.Keyword = keyword;
                result.Add(start);
            }
        }

        // Final guard for ordinary keyword matches. "independent" is used for
        // section/parameter-like starters; measurement starters receive additional
        // protections against EFW formulas, ratios and parenthesized labels.
        // Returning false means "this occurrence is text inside another block",
        // not that the OCR text should be deleted.
        private bool IsRealBlockStart(
            string text,
            int index,
            string keyword,
            bool independent)
        {
            if (independent)
            {
                // OB is a real section label on report pages, but it can also
                // appear in machine presets such as "3 Trim./OB".
                if (keyword == "OB" && index > 0 && text[index - 1] == '/')
                    return false;

                if (keyword == "OB" && index > 0 && text[index - 1] == '[')
                    return false;

                return true;
            }

            // BPD/HC/AC/FL can be formula references inside an EFW block.
            // OCR may render separators as comma, dot, slash, or spaces, so
            // determine this from the local EFW context instead of punctuation.
            if (keyword == "BPD" || keyword == "HC" || keyword == "AC" || keyword == "FL")
            {
                int previousEfw = LastEfwIndex(text, index);
                if (previousEfw >= 0)
                {
                    int distance = index - previousEfw;
                    if (distance <= 45)
                    {
                        string between = text.Substring(previousEfw, distance);

                        // A report section header ends the EFW formula-reference
                        // context even when OCR truncates "Fetal Biometry".
                        if (between.IndexOf("Fetal Biom", StringComparison.OrdinalIgnoreCase) < 0 &&
                            between.IndexOf("g ", StringComparison.OrdinalIgnoreCase) < 0 &&
                            between.IndexOf("mm ", StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }
                }
            }

            // Ratio names can also appear inside a longer EFW formula,
            // for example: AC/BPD/FL/HC. In that context FL/HC is not a
            // separate block starter.
            if (keyword == "FL/HC" && index > 0 && text[index - 1] == '/')
                return false;

            // A generic keyword must not split a known ratio/calculation.
            // Examples: FL/AC, FL/BPD, FL/HC and HC/AC.
            // Without this guard, FL/AC could incorrectly become [FL/] [AC ...].
            if (keyword == "FL" || keyword == "HC" || keyword == "AC" || keyword == "BPD")
            {
                int keywordEnd = index + keyword.Length;

                if (keywordEnd < text.Length && text[keywordEnd] == '/')
                    return false;

                if (index > 0 && text[index - 1] == '/')
                    return false;
            }

            // مثال OFD (HC) 97.37mm: این HC بخشی از Title است و Block Starter جدید نیست.
            // به طور معمول Keyword داخل Parentheses نباید Block جدید بسازد.
            // مثال‌ها: "OFD (HC) 97.37mm"، "CI (BPD/OFD) 83%" و "GA(EFW)".
            // یک Exception محدود داریم: اگر Keyword یک Ratio کامل باشد و پرانتز قبلی به علت
            // OCR ناقص با "..." باز مانده باشد، Ratio بعدی باید مستقل شناخته شود.
            // مثال واقعی: "FL/HC ... (13.30~23.90%, 15... HC/AC 1.24".
            if (IsInsideParentheses(text, index))
            {
                bool ratioAfterTruncatedRange =
                    keyword.IndexOf("/") >= 0 &&
                    HasTruncatedOpenParenthesis(text, index);

                if (!ratioAfterTruncatedRange)
                    return false;
            }

            // GA و EDD ساده عمداً Starter نیستند؛ مثال BPD ... GA ... EDD ... باید یک Block بماند و نقش آنها از Context مشخص شود.
            return true;
        }

        // نزدیک‌ترین EFW / EFW1 / EFW2 قبل از موقعیت فعلی را پیدا می‌کند.
        // دلیل: BPD، HC، AC و FL گاهی Measurement مستقل نیستند و فقط نام پارامترهای فرمول EFW هستند.
        // مثال: "EFW1 Hadlock2 BPD,AC,FL" -> BPD/AC/FL نباید سه بلوک جدید بسازند.
        // اما در Section بعدی Fetal Biometry، "BPD 3.96 cm" باید دوباره Measurement مستقل شود؛
        // به همین علت Context فرمول EFW محدود و محلی نگه داشته شده است.
        private int LastEfwIndex(string text, int beforeIndex)
        {
            int a = text.LastIndexOf("EFW1", beforeIndex, StringComparison.OrdinalIgnoreCase);
            int b = text.LastIndexOf("EFW2", beforeIndex, StringComparison.OrdinalIgnoreCase);
            int c = text.LastIndexOf("EFW", beforeIndex, StringComparison.OrdinalIgnoreCase);
            int best = a;
            if (b > best) best = b;
            if (c > best) best = c;
            return best;
        }

        // بررسی می‌کند آیا Parentheses باز قبلی به علت OCR ناقص و وجود "..." بسته نشده است.
        // این Helper فقط برای Exception محدود Ratioها استفاده می‌شود و Guard عمومی Parentheses را ضعیف نمی‌کند.
        private bool HasTruncatedOpenParenthesis(string text, int index)
        {
            int open = text.LastIndexOf('(', index);
            int close = text.LastIndexOf(')', index);

            if (open < 0 || open <= close)
                return false;

            string inside = text.Substring(open, index - open);
            return inside.IndexOf("...", StringComparison.Ordinal) >= 0;
        }

        // بررسی می‌کند Keyword فعلی داخل پرانتز باز قرار دارد یا نه.
        // دلیل: نام Measurement ممکن است داخل عنوان Measurement دیگری آمده باشد.
        // مثال: "OFD (HC) 97.37mm" -> HC اینجا توضیح OFD است، نه شروع Measurement جدید؛
        // بنابراین نباید خروجی به [OFD (] و [HC) ...] شکسته شود.
        private bool IsInsideParentheses(string text, int index)
        {
            int open = text.LastIndexOf('(', index);
            int close = text.LastIndexOf(')', index);

            return open > close;
        }
    }
}
