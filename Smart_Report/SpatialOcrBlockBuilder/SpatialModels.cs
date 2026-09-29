using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    public sealed class SpatialBounds
    {
        public double Left, Top, Right, Bottom;
        public double Width { get { return Right - Left; } }
        public double Height { get { return Bottom - Top; } }
        public double CenterX { get { return (Left + Right) / 2.0; } }
        public double CenterY { get { return (Top + Bottom) / 2.0; } }
    }

    public sealed class SpatialOcrItem
    {
        public int SourceIndex;
        public string Text;
        public double Score;
        public SpatialBounds Bounds;
        public bool IsLowConfidence;

        // ارتفاع واقعی Box متن؛ این مقدار FontSize نیست و فقط اندازه بصری OCR در تصویر است.
        public double VisualTextHeight;

        // عرض واقعی Box متن که بعداً برای تحلیل Scale، AspectRatio و الگوهای Spatial مفید است.
        public double VisualTextWidth;

        // نسبت عرض به ارتفاع Box؛ بدون وابستگی به رزولوشن تصویر قابل مقایسه‌تر است.
        public double AspectRatio;

        // ارتفاع متن نسبت به Median ارتفاع تمام OCR Itemهای همان تصویر.
        // مقدار نزدیک 1 یعنی اندازه معمول تصویر؛ کوچک‌تر/بزرگ‌تر بودن برای تشخیص نقش Spatial مفید است.
        public double RelativeTextHeight;

        // فعلاً هیچ Itemی بر اساس اندازه حذف نمی‌شود؛ این فیلد برای امتیازدهی مراحل بعدی رزرو شده است.
        public double NoiseScore;
    }

    /// <summary>
    /// ویژگی‌های آماری سطح تصویر که Scale نسبی OCR Itemها را محاسبه می‌کند.
    /// استفاده از Median باعث می‌شود تغییر Resolution و چند متن خیلی بزرگ/کوچک اثر کمتری داشته باشند.
    /// </summary>
    public sealed class SpatialDocumentMetrics
    {
        public double MedianTextHeight;

        public static SpatialDocumentMetrics Apply(IList<SpatialOcrItem> items)
        {
            SpatialDocumentMetrics metrics = new SpatialDocumentMetrics();
            List<double> heights = new List<double>();

            for (int i = 0; i < items.Count; i++)
            {
                SpatialOcrItem item = items[i];
                item.VisualTextWidth = item.Bounds == null ? 0.0 : Math.Max(0.0, item.Bounds.Width);
                item.VisualTextHeight = item.Bounds == null ? 0.0 : Math.Max(0.0, item.Bounds.Height);
                item.AspectRatio = item.VisualTextHeight > 0.0
                    ? item.VisualTextWidth / item.VisualTextHeight
                    : 0.0;

                if (item.VisualTextHeight > 0.0)
                    heights.Add(item.VisualTextHeight);
            }

            heights.Sort();
            if (heights.Count > 0)
            {
                metrics.MedianTextHeight = heights.Count % 2 == 1
                    ? heights[heights.Count / 2]
                    : (heights[heights.Count / 2 - 1] + heights[heights.Count / 2]) / 2.0;
            }

            for (int i = 0; i < items.Count; i++)
            {
                items[i].RelativeTextHeight = metrics.MedianTextHeight > 0.0
                    ? items[i].VisualTextHeight / metrics.MedianTextHeight
                    : 0.0;
            }

            return metrics;
        }
    }

    public sealed class SpatialRow
    {
        public readonly List<SpatialOcrItem> Items = new List<SpatialOcrItem>();
        public SpatialBounds Bounds;
        public double MedianHeight;

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

    public sealed class RowDetectionOptions
    {
        public double MinVerticalOverlapRatio = 0.45;
        public double MaxCenterDistanceFactor = 0.55;
        public double LowConfidenceThreshold = 0.65;
    }
}