using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    /// <summary>
    /// یک جزیره افقی داخل یک Row فیزیکی است.
    /// Cluster هنوز معنی پزشکی یا Table Cell ندارد؛ فقط نزدیکی هندسی Itemها را نگه می‌دارد.
    /// </summary>
    public sealed class SpatialCluster
    {
        public readonly List<SpatialOcrItem> Items = new List<SpatialOcrItem>();
        public SpatialBounds Bounds;
        public double GapBefore;

        public string GetText()
        {
            List<SpatialOcrItem> ordered = new List<SpatialOcrItem>(Items);
            ordered.Sort(delegate(SpatialOcrItem a, SpatialOcrItem b)
            {
                return a.Bounds.Left.CompareTo(b.Bounds.Left);
            });

            string result = "";
            for (int i = 0; i < ordered.Count; i++)
            {
                if (i > 0) result += " | ";
                result += ordered[i].Text;
            }
            return result;
        }
    }

    /// <summary>
    /// تنظیمات تشخیص Cluster افقی.
    /// Gap بر حسب ارتفاع Median همان Row نرمال می‌شود تا الگوریتم به Resolution وابسته نباشد.
    /// </summary>
    public sealed class HorizontalClusteringOptions
    {
        public double MinGapHeightFactor = 6.0;
    }

    /// <summary>
    /// Row فیزیکی را بدون استفاده از متن یا قواعد پزشکی به جزیره‌های افقی تقسیم می‌کند.
    /// هدف این مرحله جدا کردن گروه‌هایی است که از نظر هندسی فاصله غیرعادی دارند.
    /// </summary>
    public sealed class HorizontalClusterDetector
    {
        private readonly HorizontalClusteringOptions options;

        public HorizontalClusterDetector(HorizontalClusteringOptions options)
        {
            this.options = options == null ? new HorizontalClusteringOptions() : options;
        }

        public List<SpatialCluster> Detect(SpatialRow row)
        {
            List<SpatialCluster> result = new List<SpatialCluster>();
            if (row == null || row.Items.Count == 0)
                return result;

            List<SpatialOcrItem> ordered = new List<SpatialOcrItem>(row.Items);
            ordered.Sort(delegate(SpatialOcrItem a, SpatialOcrItem b)
            {
                return a.Bounds.Left.CompareTo(b.Bounds.Left);
            });

            double scale = row.MedianHeight;
            if (scale <= 0.0)
                scale = CalculateMedianHeight(ordered);
            if (scale <= 0.0)
                scale = 1.0;

            SpatialCluster current = null;
            SpatialOcrItem previous = null;

            for (int i = 0; i < ordered.Count; i++)
            {
                SpatialOcrItem item = ordered[i];
                double gap = previous == null ? 0.0 : item.Bounds.Left - previous.Bounds.Right;
                double normalizedGap = gap > 0.0 ? gap / scale : 0.0;

                // فقط Gap مثبت و بزرگ باعث Split می‌شود؛ Boxهای همپوشان یا نزدیک همیشه در یک Cluster می‌مانند.
                if (current == null || (gap > 0.0 && normalizedGap >= options.MinGapHeightFactor))
                {
                    current = new SpatialCluster();
                    current.GapBefore = gap > 0.0 ? gap : 0.0;
                    result.Add(current);
                }

                current.Items.Add(item);
                ExpandBounds(current, item.Bounds);
                previous = item;
            }

            return result;
        }

        public List<List<SpatialCluster>> Detect(IList<SpatialRow> rows)
        {
            List<List<SpatialCluster>> result = new List<List<SpatialCluster>>();
            if (rows == null) return result;

            for (int i = 0; i < rows.Count; i++)
                result.Add(Detect(rows[i]));

            return result;
        }

        private static void ExpandBounds(SpatialCluster cluster, SpatialBounds bounds)
        {
            if (cluster.Bounds == null)
            {
                cluster.Bounds = new SpatialBounds();
                cluster.Bounds.Left = bounds.Left;
                cluster.Bounds.Top = bounds.Top;
                cluster.Bounds.Right = bounds.Right;
                cluster.Bounds.Bottom = bounds.Bottom;
                return;
            }

            cluster.Bounds.Left = Math.Min(cluster.Bounds.Left, bounds.Left);
            cluster.Bounds.Top = Math.Min(cluster.Bounds.Top, bounds.Top);
            cluster.Bounds.Right = Math.Max(cluster.Bounds.Right, bounds.Right);
            cluster.Bounds.Bottom = Math.Max(cluster.Bounds.Bottom, bounds.Bottom);
        }

        private static double CalculateMedianHeight(IList<SpatialOcrItem> items)
        {
            List<double> heights = new List<double>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Bounds != null && items[i].Bounds.Height > 0.0)
                    heights.Add(items[i].Bounds.Height);
            }

            heights.Sort();
            if (heights.Count == 0) return 0.0;
            if (heights.Count % 2 == 1) return heights[heights.Count / 2];
            return (heights[heights.Count / 2 - 1] + heights[heights.Count / 2]) / 2.0;
        }
    }
}
