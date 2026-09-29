using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    /// <summary>
    /// یک X تکرارشونده در چند Row است. Anchor ستون یا Cell قطعی نیست؛
    /// فقط شاهد هندسی است که چند OCR Item در امتداد عمودی مشابه قرار گرفته‌اند.
    /// </summary>
    public sealed class AlignmentAnchor
    {
        public double X;
        public readonly List<SpatialOcrItem> Items = new List<SpatialOcrItem>();
        public readonly List<int> RowIndexes = new List<int>();

        public int RowCount { get { return RowIndexes.Count; } }
        public int StartRow { get { return RowIndexes.Count == 0 ? -1 : RowIndexes[0]; } }
        public int EndRow { get { return RowIndexes.Count == 0 ? -1 : RowIndexes[RowIndexes.Count - 1]; } }
    }

    public sealed class AlignmentAnchorOptions
    {
        // تلرانس X نسبت به Median ارتفاع متن نرمال می‌شود تا Resolution تصویر تعیین‌کننده نباشد.
        public double MaxHorizontalDistanceHeightFactor = 1.5;

        // Anchor تک‌ردیفی برای تحلیل ساختار صفحه قابل اتکا نیست؛ حداقل در دو Row باید تکرار شود.
        public int MinRows = 2;

        // اگر Anchor برای چند Row متوالی شاهد نداشته باشد، ادامه دادن آن باعث اتصال
        // بخش‌های مستقل صفحه فقط به دلیل X مشابه می‌شود. یک Row خالی را تحمل می‌کنیم.
        public int MaxMissingRows = 1;
    }

    /// <summary>
    /// Alignment عمودی OCR Itemها را فقط از روی مختصات پیدا می‌کند.
    /// این Detector هیچ واژه پزشکی، نام دستگاه یا Regex محتوایی نمی‌شناسد.
    /// </summary>
    public sealed class AlignmentAnchorDetector
    {
        private readonly AlignmentAnchorOptions options;

        public AlignmentAnchorDetector(AlignmentAnchorOptions options)
        {
            this.options = options == null ? new AlignmentAnchorOptions() : options;
        }

        public List<AlignmentAnchor> Detect(IList<SpatialRow> rows)
        {
            List<AlignmentAnchor> candidates = new List<AlignmentAnchor>();
            List<AlignmentAnchor> result = new List<AlignmentAnchor>();
            if (rows == null || rows.Count == 0) return result;

            double scale = CalculateDocumentMedianHeight(rows);
            if (scale <= 0.0) scale = 1.0;
            double tolerance = scale * options.MaxHorizontalDistanceHeightFactor;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                SpatialRow row = rows[rowIndex];
                for (int itemIndex = 0; itemIndex < row.Items.Count; itemIndex++)
                {
                    SpatialOcrItem item = row.Items[itemIndex];
                    AlignmentAnchor best = null;
                    double bestDistance = double.MaxValue;

                    for (int a = 0; a < candidates.Count; a++)
                    {
                        // یک Anchor از یک Row فقط یک رأی می‌گیرد؛ وگرنه چند Cell همان Row
                        // می‌توانند به اشتباه یک Alignment عمودی مصنوعی بسازند.
                        if (ContainsRow(candidates[a], rowIndex)) continue;

                        // Anchor محلی است: اگر آخرین شاهد آن بیش از حد از Row جاری دور باشد،
                        // حتی با X یکسان نباید Header و Tableهای پایین صفحه را به هم متصل کند.
                        int missingRows = rowIndex - candidates[a].EndRow - 1;
                        if (missingRows > options.MaxMissingRows) continue;

                        double distance = Math.Abs(candidates[a].X - item.Bounds.Left);
                        if (distance <= tolerance && distance < bestDistance)
                        {
                            best = candidates[a];
                            bestDistance = distance;
                        }
                    }

                    if (best == null)
                    {
                        best = new AlignmentAnchor();
                        best.X = item.Bounds.Left;
                        candidates.Add(best);
                    }

                    best.Items.Add(item);
                    best.RowIndexes.Add(rowIndex);
                    RecalculateX(best);
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].RowCount >= options.MinRows)
                    result.Add(candidates[i]);
            }

            result.Sort(delegate(AlignmentAnchor a, AlignmentAnchor b)
            {
                int row = a.StartRow.CompareTo(b.StartRow);
                return row != 0 ? row : a.X.CompareTo(b.X);
            });
            return result;
        }

        private static bool ContainsRow(AlignmentAnchor anchor, int rowIndex)
        {
            for (int i = 0; i < anchor.RowIndexes.Count; i++)
                if (anchor.RowIndexes[i] == rowIndex) return true;
            return false;
        }

        private static void RecalculateX(AlignmentAnchor anchor)
        {
            List<double> values = new List<double>();
            for (int i = 0; i < anchor.Items.Count; i++)
                values.Add(anchor.Items[i].Bounds.Left);

            values.Sort();
            if (values.Count % 2 == 1)
                anchor.X = values[values.Count / 2];
            else
                anchor.X = (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2.0;
        }

        private static double CalculateDocumentMedianHeight(IList<SpatialRow> rows)
        {
            List<double> heights = new List<double>();
            for (int i = 0; i < rows.Count; i++)
                for (int j = 0; j < rows[i].Items.Count; j++)
                    if (rows[i].Items[j].Bounds != null && rows[i].Items[j].Bounds.Height > 0.0)
                        heights.Add(rows[i].Items[j].Bounds.Height);

            heights.Sort();
            if (heights.Count == 0) return 0.0;
            if (heights.Count % 2 == 1) return heights[heights.Count / 2];
            return (heights[heights.Count / 2 - 1] + heights[heights.Count / 2]) / 2.0;
        }
    }
}
