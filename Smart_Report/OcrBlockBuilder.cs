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

        // Creates an empty OCR block. Indices start at -1 so an uninitialized
        // block cannot be confused with a real block beginning at character 0.
        public OcrBlock()
        {
            Text = "";
            StartKeyword = "";
            StartIndex = -1;
            EndIndex = -1;
        }

        // Debug/display representation of a block. The raw Text itself is not
        // modified; brackets are added only when the object is converted to text.
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

        // Creates a builder with the application's default editable rules.
        public OcrBlockBuilder()
            : this(OcrBlockRules.CreateDefault())
        {
        }

        // Creates a builder from caller-supplied rules. SetRules clones them so
        // later edits outside the builder cannot silently change current behavior.
        public OcrBlockBuilder(OcrBlockRules rules)
        {
            SetRules(rules);
        }

        // Replaces the active starter rules. A null value intentionally falls
        // back to defaults instead of leaving the builder without rules.
        public void SetRules(OcrBlockRules rules)
        {
            _rules = (rules == null) ? OcrBlockRules.CreateDefault() : rules.Clone();
        }

        // Main blocking pipeline:
        // 1) normalize OCR whitespace, 2) find every supported block start,
        // 3) preserve unknown text before/between starts, and 4) cut blocks.
        // IMPORTANT: this stage decides boundaries only; it does not decide
        // whether a block is clinically useful and it must preserve raw OCR data.
        public List<OcrBlock> Build(string rawOcrText)
        {
            List<OcrBlock> blocks = new List<OcrBlock>();

            if (String.IsNullOrEmpty(rawOcrText))
                return blocks;

            string text = Normalize(rawOcrText);
            List<BlockStart> starts = FindBlockStarts(text);

            // Zero Data Loss:
            // If no known block starter exists, keep the whole OCR text.
            if (starts.Count == 0)
            {
                AddBlock(blocks, text, "", 0, text.Length);
                return blocks;
            }

            // Keep everything before the first recognized starter as a block.
            // Unknown/header/device text must never disappear.
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

        // Adds one exact character range as an OcrBlock. Empty ranges are ignored,
        // but non-empty OCR text is preserved (Zero Data Loss principle).
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

        // Convenience output used by the test/UI: builds blocks and prints one
        // [block] per line. Brackets are presentation markers, not OCR correction.
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

        // Normalizes only whitespace so matching is stable across OCR engines.
        // It deliberately does NOT repair spelling/glyph errors such as Fetaļ.
        private string Normalize(string text)
        {
            text = text.Replace("\r", " ");
            text = text.Replace("\n", " ");
            text = text.Replace("\t", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        // Collects all candidate boundaries from editable rules plus narrowly
        // defined OCR-aware patterns (FHR, NT, NB, D, etc.). Candidates are sorted
        // and overlapping/duplicate starts are removed before Build() cuts text.
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
                // Fetal HR is often OCR'd without a separator before the value,
                // e.g. "Fetal HR158-bpm". Handle it with its own value pattern.
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

                // At the same location, prefer the more specific keyword.
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

                // Same start position: keep only the most specific match.
                if (previous.Index == found[i].Index)
                    continue;

                // Prevent a shorter keyword inside a longer already detected
                // keyword from creating a false block.
                int previousEnd = previous.Index + previous.Keyword.Length;
                if (found[i].Index < previousEnd)
                    continue;

                unique.Add(found[i]);
            }

            return unique;
        }

        // Finds report section labels written as [OB]. We start at '[' so the
        // complete raw label stays together. Plain OB matching is separately
        // suppressed inside brackets and in machine presets such as 3 Trim./OB.
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

        // Finds truncated OCR section headings such as "Fetal Biom..." and
        // "Fetal Long...". These cannot safely be unconditional short keywords,
        // because the same prefixes may occur in unrelated text.
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

        // Recognizes report-style AUA only when followed by a gestational-age
        // value (for example AUA 17w5d). This avoids making bare AUA too broad.
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

        // Recovers a ratio boundary when OCR attaches preceding chart/table text
        // to it, e.g. "5,255FL/AC 21.16 %". Normal spaced FL/AC is already handled
        // by editable measurement rules, so this helper acts only on attached text.
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

        // Finds the COMPLETE Fetal HR measurement, not just the token "HR".
        // Supported real OCR forms include:
        //   Fetal HR158-bpm      (missing space / hyphen before bpm)
        //   Fetal HR 143 143 bpm (report table repeats the numeric value)
        //   Fetaļ HR158-bpm      (OCR substitutes ļ for final l)
        //   Feta HR158-bpm       (OCR drops final l)
        // Requiring numeric value + bpm keeps this tolerant spelling rule narrow.
        // Raw OCR is never corrected: Fetaļ/Feta remain exactly as recognized.
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

        // Finds generic HR only when syntax proves it is a measurement:
        // HR + numeric value + bpm, e.g. "HR 147 bpm".
        // This is intentionally conditional because machine text such as
        // "32Hz HR TIs 0.1" must NOT start an HR block. If HR belongs to a
        // Fetal/Fetaļ/Feta HR phrase, FindFetalHeartRate owns the whole phrase
        // and this helper skips the inner HR to prevent a split in the middle.
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

        // Conditional numeric measurement matcher used for ambiguous short labels.
        // Current callers: NT, NB and D. A label becomes a boundary only when a
        // number and mm/cm follow it. Thus NT/CA1-7S (a preset) is not NT data,
        // while "NT 2.32 mm" is. D additionally accepts OCR "D.4.10 mm" where
        // a dot was inserted between the label and value. Do not generalize this
        // to arbitrary word+number+unit patterns; Doppler/device settings contain
        // many similar forms (SV, SVD, PRF, WF) that are not block starters here.
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

            // Example: OFD (HC) 97.37mm
            // HC is part of the OFD title, not a new block.
            if (IsInsideParentheses(text, index))
                return false;

            // Plain GA and EDD never reach here because they are deliberately
            // not registered as starters. Their role will be determined from
            // the surrounding block.
            return true;
        }

        // Returns the nearest preceding EFW/EFW1/EFW2 occurrence. It supports the
        // local EFW-formula guard so BPD/HC/AC/FL references inside a formula do not
        // incorrectly create new blocks. The guard is deliberately local in distance.
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

        // True when index lies after the most recent '(' with no closing ')' yet.
        // Example: in "OFD (HC) 97.37mm", HC describes OFD and must not split.
        private bool IsInsideParentheses(string text, int index)
        {
            int open = text.LastIndexOf('(', index);
            int close = text.LastIndexOf(')', index);

            return open > close;
        }
    }
}
