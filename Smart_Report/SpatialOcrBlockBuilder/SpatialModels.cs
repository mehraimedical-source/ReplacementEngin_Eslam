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