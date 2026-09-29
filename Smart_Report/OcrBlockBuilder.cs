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

        public OcrBlock()
        {
            Text = "";
            StartKeyword = "";
            StartIndex = -1;
            EndIndex = -1;
        }

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

        public OcrBlockBuilder()
            : this(OcrBlockRules.CreateDefault())
        {
        }

        public OcrBlockBuilder(OcrBlockRules rules)
        {
            SetRules(rules);
        }

        public void SetRules(OcrBlockRules rules)
        {
            _rules = (rules == null) ? OcrBlockRules.CreateDefault() : rules.Clone();
        }

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

        private string Normalize(string text)
        {
            text = text.Replace("\r", " ");
            text = text.Replace("\n", " ");
            text = text.Replace("\t", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        private List<BlockStart> FindBlockStarts(string text)
        {
            List<BlockStart> found = new List<BlockStart>();

            for (int i = 0; i < _rules.Sections.Count; i++)
                FindKeyword(text, _rules.Sections[i], found, true);

            for (int i = 0; i < _rules.Parameters.Count; i++)
                FindKeyword(text, _rules.Parameters[i], found, true);

            for (int i = 0; i < _rules.Measurements.Count; i++)
                FindKeyword(text, _rules.Measurements[i], found, false);

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

        private bool IsRealBlockStart(
            string text,
            int index,
            string keyword,
            bool independent)
        {
            if (independent)
                return true;

            // Measurement names separated by commas are references inside
            // formulas such as EFW1 Hadlock2 BPD,AC,FL and must not split.
            if (keyword == "BPD" || keyword == "HC" || keyword == "AC" || keyword == "FL")
            {
                int keywordEnd = index + keyword.Length;
                if ((keywordEnd < text.Length && text[keywordEnd] == ',') ||
                    (index > 0 && text[index - 1] == ','))
                    return false;
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

        private bool IsInsideParentheses(string text, int index)
        {
            int open = text.LastIndexOf('(', index);
            int close = text.LastIndexOf(')', index);

            return open > close;
        }
    }
}
