using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    /// <summary>
    /// Region مجموعه‌ای از Clusterهای نزدیک و هم‌تراز در چند Row مجاور است.
    /// این لایه هنوز هیچ معنی پزشکی ندارد و فقط ساختار دوبعدی صفحه را بازسازی می‌کند.
    /// </summary>
    public sealed class SpatialRegion
    {
        public readonly List<SpatialCluster> Clusters = new List<SpatialCluster>();
        public SpatialBounds Bounds;

        public string GetText()
        {
            List<SpatialCluster> ordered = new List<SpatialCluster>(Clusters);
            ordered.Sort(delegate(SpatialCluster a, SpatialCluster b)
            {
                int y = a.Bounds.Top.CompareTo(b.Bounds.Top);
                return y != 0 ? y : a.Bounds.Left.CompareTo(b.Bounds.Left);
            });

            string result = "";
            for (int i = 0; i < ordered.Count; i++)
            {
                if (i > 0) result += " / ";
                result += ordered[i].GetText();
            }
            return result;
        }
    }

    public sealed class SpatialRegionOptions
    {
        // Clusterهای یک Region باید از نظر افقی بخش قابل توجهی از عرض یکدیگر را پوشش دهند
        // یا لبه چپ بسیار نزدیک داشته باشند؛ این کار ستون‌های دور از هم را جدا نگه می‌دارد.
        public double MinHorizontalOverlapRatio = 0.25;
        public double MaxLeftAlignmentHeightFactor = 2.5;

        // فاصله عمودی نیز نسبت به ارتفاع متن نرمال می‌شود تا Resolution تصویر روی نتیجه اثر مستقیم نداشته باشد.
        public double MaxVerticalGapHeightFactor = 2.0;
    }

    /// <summary>
    /// Clusterهای Rowهای مختلف را بر اساس نزدیکی عمودی و Alignment افقی به Region تبدیل می‌کند.
    /// مثال مهم: HR و Time در سمت راست تصویر با هم Region می‌شوند،
    /// در حالی که DR و FA سمت چپ به Region دیگری تعلق می‌گیرند.
    /// </summary>
    public sealed class SpatialRegionDetector
    {
        private readonly SpatialRegionOptions options;

        public SpatialRegionDetector(SpatialRegionOptions options)
        {
            this.options = options == null ? new SpatialRegionOptions() : options;
        }

        public List<SpatialRegion> Detect(IList<SpatialRow> rows, HorizontalClusterDetector clusterDetector)
        {
            List<SpatialRegion> regions = new List<SpatialRegion>();
            if (rows == null || clusterDetector == null) return regions;

            List<SpatialCluster> clusters = new List<SpatialCluster>();
            for (int i = 0; i < rows.Count; i++)
            {
                List<SpatialCluster> rowClusters = clusterDetector.Detect(rows[i]);
                for (int j = 0; j < rowClusters.Count; j++)
                    clusters.Add(rowClusters[j]);
            }

            clusters.Sort(delegate(SpatialCluster a, SpatialCluster b)
            {
                int y = a.Bounds.Top.CompareTo(b.Bounds.Top);
                return y != 0 ? y : a.Bounds.Left.CompareTo(b.Bounds.Left);
            });

            for (int i = 0; i < clusters.Count; i++)
            {
                SpatialCluster cluster = clusters[i];
                SpatialRegion best = null;
                double bestScore = double.MinValue;

                for (int r = 0; r < regions.Count; r++)
                {
                    double score;
                    if (CanJoin(regions[r], cluster, out score) && score > bestScore)
                    {
                        best = regions[r];
                        bestScore = score;
                    }
                }

                if (best == null)
                {
                    best = new SpatialRegion();
                    regions.Add(best);
                }

                best.Clusters.Add(cluster);
                ExpandBounds(best, cluster.Bounds);
            }

            regions.Sort(delegate(SpatialRegion a, SpatialRegion b)
            {
                int x = a.Bounds.Left.CompareTo(b.Bounds.Left);
                return x != 0 ? x : a.Bounds.Top.CompareTo(b.Bounds.Top);
            });

            return regions;
        }

        private bool CanJoin(SpatialRegion region, SpatialCluster cluster, out double score)
        {
            double verticalGap = cluster.Bounds.Top - region.Bounds.Bottom;
            if (verticalGap < 0.0) verticalGap = 0.0;

            double scale = Math.Max(1.0, Math.Min(region.Bounds.Height, cluster.Bounds.Height));
            double verticalFactor = verticalGap / scale;
            if (verticalFactor > options.MaxVerticalGapHeightFactor)
            {
                score = double.MinValue;
                return false;
            }

            double overlap = Math.Max(0.0,
                Math.Min(region.Bounds.Right, cluster.Bounds.Right) -
                Math.Max(region.Bounds.Left, cluster.Bounds.Left));
            double minWidth = Math.Max(1.0, Math.Min(region.Bounds.Width, cluster.Bounds.Width));
            double overlapRatio = overlap / minWidth;

            double leftDistance = Math.Abs(region.Bounds.Left - cluster.Bounds.Left);
            double leftFactor = leftDistance / scale;

            bool aligned = overlapRatio >= options.MinHorizontalOverlapRatio ||
                           leftFactor <= options.MaxLeftAlignmentHeightFactor;

            score = overlapRatio - verticalFactor * 0.25 - leftFactor * 0.05;
            return aligned;
        }

        private static void ExpandBounds(SpatialRegion region, SpatialBounds bounds)
        {
            if (region.Bounds == null)
            {
                region.Bounds = new SpatialBounds();
                region.Bounds.Left = bounds.Left;
                region.Bounds.Top = bounds.Top;
                region.Bounds.Right = bounds.Right;
                region.Bounds.Bottom = bounds.Bottom;
                return;
            }

            region.Bounds.Left = Math.Min(region.Bounds.Left, bounds.Left);
            region.Bounds.Top = Math.Min(region.Bounds.Top, bounds.Top);
            region.Bounds.Right = Math.Max(region.Bounds.Right, bounds.Right);
            region.Bounds.Bottom = Math.Max(region.Bounds.Bottom, bounds.Bottom);
        }
    }
}
