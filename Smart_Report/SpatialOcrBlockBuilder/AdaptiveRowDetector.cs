using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    public sealed class AdaptiveRowDetector
    {
        private readonly RowDetectionOptions options;

        public AdaptiveRowDetector(RowDetectionOptions options)
        {
            this.options = options ?? new RowDetectionOptions();
        }

        public List<SpatialRow> Detect(IList<SpatialOcrItem> source)
        {
            List<SpatialOcrItem> items = new List<SpatialOcrItem>();
            for (int i = 0; i < source.Count; i++) items.Add(source[i]);
            items.Sort(CompareReadingSeed);

            List<SpatialRow> rows = new List<SpatialRow>();
            for (int i = 0; i < items.Count; i++)
            {
                SpatialOcrItem item = items[i];
                SpatialRow best = null;
                double bestScore = double.MinValue;

                for (int r = rows.Count - 1; r >= 0; r--)
                {
                    SpatialRow row = rows[r];
                    if (item.Bounds.Top - row.Bounds.Bottom > Math.Max(item.Bounds.Height, row.MedianHeight))
                        break;

                    double score;
                    if (CanJoin(row, item, out score) && score > bestScore)
                    {
                        best = row;
                        bestScore = score;
                    }
                }

                if (best == null)
                {
                    best = new SpatialRow();
                    best.Items.Add(item);
                    Recalculate(best);
                    rows.Add(best);
                }
                else
                {
                    best.Items.Add(item);
                    Recalculate(best);
                }
            }

            rows.Sort(delegate(SpatialRow a, SpatialRow b)
            {
                int y = a.Bounds.CenterY.CompareTo(b.Bounds.CenterY);
                return y != 0 ? y : a.Bounds.Left.CompareTo(b.Bounds.Left);
            });

            for (int i = 0; i < rows.Count; i++)
                rows[i].Items.Sort(delegate(SpatialOcrItem a, SpatialOcrItem b) { return a.Bounds.Left.CompareTo(b.Bounds.Left); });

            return rows;
        }

        private bool CanJoin(SpatialRow row, SpatialOcrItem item, out double score)
        {
            double overlap = Math.Max(0.0, Math.Min(row.Bounds.Bottom, item.Bounds.Bottom) - Math.Max(row.Bounds.Top, item.Bounds.Top));
            double baseHeight = Math.Min(Math.Max(1.0, row.MedianHeight), Math.Max(1.0, item.Bounds.Height));
            double overlapRatio = overlap / baseHeight;

            double centerDistance = Math.Abs(row.Bounds.CenterY - item.Bounds.CenterY);
            double adaptiveHeight = Math.Max(row.MedianHeight, item.Bounds.Height);
            double centerRatio = centerDistance / Math.Max(1.0, adaptiveHeight);

            bool accepted = overlapRatio >= options.MinVerticalOverlapRatio ||
                            centerRatio <= options.MaxCenterDistanceFactor;

            score = overlapRatio - centerRatio * 0.35;
            return accepted;
        }

        private static int CompareReadingSeed(SpatialOcrItem a, SpatialOcrItem b)
        {
            int y = a.Bounds.CenterY.CompareTo(b.Bounds.CenterY);
            return y != 0 ? y : a.Bounds.Left.CompareTo(b.Bounds.Left);
        }

        private static void Recalculate(SpatialRow row)
        {
            double left = double.MaxValue, top = double.MaxValue, right = double.MinValue, bottom = double.MinValue;
            List<double> heights = new List<double>();
            for (int i = 0; i < row.Items.Count; i++)
            {
                SpatialBounds b = row.Items[i].Bounds;
                if (b.Left < left) left = b.Left; if (b.Top < top) top = b.Top;
                if (b.Right > right) right = b.Right; if (b.Bottom > bottom) bottom = b.Bottom;
                heights.Add(b.Height);
            }
            heights.Sort();
            row.MedianHeight = heights.Count % 2 == 1 ? heights[heights.Count / 2] :
                (heights[heights.Count / 2 - 1] + heights[heights.Count / 2]) / 2.0;
            row.Bounds = new SpatialBounds();
            row.Bounds.Left = left; row.Bounds.Top = top; row.Bounds.Right = right; row.Bounds.Bottom = bottom;
        }
    }
}