using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    public sealed class AlignmentPattern
    {
        public int RowIndex;
        public readonly List<double> Positions = new List<double>();
    }

    public sealed class AlignmentPatternTransition
    {
        public int PreviousRowIndex;
        public int CurrentRowIndex;
        public double Similarity;
        public double ChangeScore { get { return 1.0 - Similarity; } }
        public bool IsBoundary;
    }

    public sealed class AlignmentPatternOptions
    {
        // فاصله X نسبت به Median ارتفاع متن نرمال می‌شود تا Resolution تعیین‌کننده نباشد.
        public double MaxHorizontalDistanceHeightFactor = 2.0;

        // Boundary فقط شاهد هندسی تغییر Topology است و هنوز Section قطعی محسوب نمی‌شود.
        public double BoundaryChangeThreshold = 0.55;
    }

    /// <summary>
    /// الگوی Alignment هر Row و میزان تغییر ساختار بین Rowهای متوالی را بدون شناخت متن محاسبه می‌کند.
    /// </summary>
    public sealed class AlignmentPatternDetector
    {
        private readonly AlignmentPatternOptions options;

        public AlignmentPatternDetector(AlignmentPatternOptions options)
        {
            this.options = options == null ? new AlignmentPatternOptions() : options;
        }

        public List<AlignmentPattern> BuildPatterns(IList<SpatialRow> rows)
        {
            List<AlignmentPattern> result = new List<AlignmentPattern>();
            if (rows == null) return result;
            double scale = CalculateDocumentMedianHeight(rows);
            if (scale <= 0.0) scale = 1.0;

            for (int i = 0; i < rows.Count; i++)
            {
                AlignmentPattern p = new AlignmentPattern();
                p.RowIndex = i;
                List<SpatialOcrItem> ordered = new List<SpatialOcrItem>(rows[i].Items);
                ordered.Sort(delegate(SpatialOcrItem a, SpatialOcrItem b)
                {
                    return a.Bounds.Left.CompareTo(b.Bounds.Left);
                });
                for (int j = 0; j < ordered.Count; j++)
                    p.Positions.Add(ordered[j].Bounds.Left / scale);
                result.Add(p);
            }
            return result;
        }

        public List<AlignmentPatternTransition> DetectTransitions(IList<SpatialRow> rows)
        {
            List<AlignmentPatternTransition> result = new List<AlignmentPatternTransition>();
            List<AlignmentPattern> patterns = BuildPatterns(rows);
            if (patterns.Count < 2) return result;

            for (int i = 1; i < patterns.Count; i++)
            {
                AlignmentPatternTransition t = new AlignmentPatternTransition();
                t.PreviousRowIndex = patterns[i - 1].RowIndex;
                t.CurrentRowIndex = patterns[i].RowIndex;
                t.Similarity = CalculateSimilarity(patterns[i - 1].Positions,
                    patterns[i].Positions, options.MaxHorizontalDistanceHeightFactor);
                t.IsBoundary = t.ChangeScore >= options.BoundaryChangeThreshold;
                result.Add(t);
            }
            return result;
        }

        private static double CalculateSimilarity(IList<double> a, IList<double> b, double tolerance)
        {
            if (a.Count == 0 && b.Count == 0) return 1.0;
            if (a.Count == 0 || b.Count == 0) return 0.0;
            bool[] used = new bool[b.Count];
            int matches = 0;

            // Matching یک‌به‌یک مانع می‌شود چند Item نزدیک به یک ستون رأی تکراری بدهند.
            for (int i = 0; i < a.Count; i++)
            {
                int best = -1;
                double bestDistance = double.MaxValue;
                for (int j = 0; j < b.Count; j++)
                {
                    if (used[j]) continue;
                    double d = Math.Abs(a[i] - b[j]);
                    if (d <= tolerance && d < bestDistance)
                    {
                        best = j;
                        bestDistance = d;
                    }
                }
                if (best >= 0)
                {
                    used[best] = true;
                    matches++;
                }
            }

            // Dice similarity هم Alignment و هم تغییر تعداد ستون‌ها را منعکس می‌کند.
            return (2.0 * matches) / (a.Count + b.Count);
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
