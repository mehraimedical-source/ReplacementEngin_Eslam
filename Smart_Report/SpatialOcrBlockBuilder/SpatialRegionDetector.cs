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

        // اگر Median عرض Clusterها بخش بزرگی از عرض Document باشد، Layout از نوع Row-Structured است
        // و باید به جای Chain کردن Region.Bounds، مرزهای عمودی Sectionها را پیدا کنیم.
        public double WideClusterDocumentRatio = 0.45;

        // Gap بزرگ بین Rowها یک Boundary قوی Section است.
        public double SectionGapHeightFactor = 0.45;

        // افت محسوس تعداد ستون‌ها همراه با تغییر Topology می‌تواند Header Section جدید باشد.
        public double SectionTopologyChangeThreshold = 0.35;
        public int SectionColumnDrop = 2;
        public double SectionForwardSimilarityThreshold = 0.60;

        // Headerهای تکرارشونده از روی Pattern هندسی قبلی شناخته می‌شوند، نه متن.
        public double RepeatedPatternSimilarityThreshold = 0.90;
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

            // Region باید همان Clusterهای Contextual را مصرف کند که در Grid دیده می‌شوند.
            // در غیر این صورت UI یک BPD Merge شده نشان می‌دهد ولی Region دوباره Split خام را می‌سازد.
            List<ContextualClusterResolution> resolved =
                new ContextualClusterResolver(new ContextualClusterResolverOptions())
                .Resolve(rows, clusterDetector);
            return DetectResolved(resolved);
        }

        public List<SpatialRegion> DetectResolved(IList<ContextualClusterResolution> resolved)
        {
            List<SpatialRegion> regions = new List<SpatialRegion>();
            if (resolved == null || resolved.Count == 0) return regions;

            // دو نوع Layout داریم:
            // 1) Document/Table که بیشتر Rowها یک Cluster عریض دارند.
            // 2) Overlay/Sparse مثل تصویر دستگاه که چند ستون مستقل عمودی دارد.
            // استفاده از یک الگوریتم برای هر دو نوع باعث Chain Effect می‌شد و کل صفحه را یک Region می‌کرد.
            if (IsRowStructuredDocument(resolved))
                return DetectRowStructuredRegions(resolved);

            return DetectSparseRegions(resolved);
        }

        /// <summary>
        /// Layoutهای Document/Table را Row-by-Row بخش‌بندی می‌کند.
        /// مرز Section فقط از Gap عمودی و تغییر Pattern ستون‌ها ساخته می‌شود؛ متن هیچ نقشی ندارد.
        /// </summary>
        private List<SpatialRegion> DetectRowStructuredRegions(
            IList<ContextualClusterResolution> resolved)
        {
            List<SpatialRegion> result = new List<SpatialRegion>();
            List<RowRegionInfo> rows = BuildRowInfos(resolved);
            if (rows.Count == 0) return result;

            double scale = CalculateMedianClusterHeight(resolved);
            if (scale <= 0.0) scale = 1.0;

            SpatialRegion current = new SpatialRegion();

            for (int i = 0; i < rows.Count; i++)
            {
                bool boundary = i > 0 && IsSectionBoundary(rows, i, scale);
                if (boundary && current.Clusters.Count > 0)
                {
                    result.Add(current);
                    current = new SpatialRegion();
                }

                for (int j = 0; j < rows[i].Clusters.Count; j++)
                {
                    current.Clusters.Add(rows[i].Clusters[j]);
                    ExpandBounds(current, rows[i].Clusters[j].Bounds);
                }
            }

            if (current.Clusters.Count > 0)
                result.Add(current);

            return result;
        }

        /// <summary>
        /// Layoutهای Sparse همان رفتار Column/Island قبلی را نگه می‌دارند؛
        /// این مسیر برای Sampleهایی مثل Overlay دستگاه مناسب است.
        /// </summary>
        private List<SpatialRegion> DetectSparseRegions(
            IList<ContextualClusterResolution> resolved)
        {
            List<SpatialRegion> regions = new List<SpatialRegion>();
            List<SpatialCluster> clusters = new List<SpatialCluster>();

            for (int i = 0; i < resolved.Count; i++)
            {
                List<SpatialCluster> rowClusters = resolved[i].ResolvedClusters;
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

        private sealed class RowRegionInfo
        {
            public readonly List<SpatialCluster> Clusters = new List<SpatialCluster>();
            public readonly List<double> Positions = new List<double>();
            public SpatialBounds Bounds;
            public int ItemCount;
        }

        private static List<RowRegionInfo> BuildRowInfos(
            IList<ContextualClusterResolution> resolved)
        {
            List<RowRegionInfo> result = new List<RowRegionInfo>();

            for (int i = 0; i < resolved.Count; i++)
            {
                if (resolved[i].ResolvedClusters.Count == 0) continue;

                RowRegionInfo info = new RowRegionInfo();
                double left = double.MaxValue, top = double.MaxValue;
                double right = double.MinValue, bottom = double.MinValue;

                for (int j = 0; j < resolved[i].ResolvedClusters.Count; j++)
                {
                    SpatialCluster cluster = resolved[i].ResolvedClusters[j];
                    info.Clusters.Add(cluster);

                    for (int k = 0; k < cluster.Items.Count; k++)
                    {
                        SpatialOcrItem item = cluster.Items[k];
                        info.Positions.Add(item.Bounds.Left);
                        info.ItemCount++;
                    }

                    if (cluster.Bounds.Left < left) left = cluster.Bounds.Left;
                    if (cluster.Bounds.Top < top) top = cluster.Bounds.Top;
                    if (cluster.Bounds.Right > right) right = cluster.Bounds.Right;
                    if (cluster.Bounds.Bottom > bottom) bottom = cluster.Bounds.Bottom;
                }

                info.Positions.Sort();
                info.Bounds = new SpatialBounds();
                info.Bounds.Left = left; info.Bounds.Top = top;
                info.Bounds.Right = right; info.Bounds.Bottom = bottom;
                result.Add(info);
            }

            return result;
        }

        private bool IsSectionBoundary(IList<RowRegionInfo> rows, int index, double scale)
        {
            RowRegionInfo previous = rows[index - 1];
            RowRegionInfo current = rows[index];

            double gap = current.Bounds.Top - previous.Bounds.Bottom;
            if (gap < 0.0) gap = 0.0;
            if ((gap / scale) >= options.SectionGapHeightFactor)
                return true;

            double previousSimilarity = CalculatePatternSimilarity(
                previous.Positions, current.Positions, scale);

            // Header جدید معمولاً ستون‌های کمتری از Row داده قبلی دارد،
            // ولی Row بعدی دوباره با آن Section هم‌خانواده می‌شود.
            if (index + 1 < rows.Count)
            {
                double forwardSimilarity = CalculatePatternSimilarity(
                    current.Positions, rows[index + 1].Positions, scale);

                int drop = previous.ItemCount - current.ItemCount;
                double change = 1.0 - previousSimilarity;

                if (drop >= options.SectionColumnDrop &&
                    change >= options.SectionTopologyChangeThreshold &&
                    forwardSimilarity >= options.SectionForwardSimilarityThreshold)
                    return true;
            }

            // Headerهای تکرارشونده مثل جدول‌های پشت‌سرهم با Pattern قبلی تشخیص داده می‌شوند.
            // Row مجاور قبلی عمداً از جستجو حذف می‌شود چون هدف پیدا کردن بازگشت یک Pattern قدیمی است.
            for (int i = 0; i < index - 1; i++)
            {
                double repeated = CalculatePatternSimilarity(
                    rows[i].Positions, current.Positions, scale);

                if (repeated >= options.RepeatedPatternSimilarityThreshold &&
                    previousSimilarity < options.RepeatedPatternSimilarityThreshold)
                    return true;
            }

            return false;
        }

        private bool IsRowStructuredDocument(
            IList<ContextualClusterResolution> resolved)
        {
            List<double> widths = new List<double>();
            double documentLeft = double.MaxValue;
            double documentRight = double.MinValue;

            for (int i = 0; i < resolved.Count; i++)
            {
                for (int j = 0; j < resolved[i].ResolvedClusters.Count; j++)
                {
                    SpatialCluster cluster = resolved[i].ResolvedClusters[j];
                    widths.Add(cluster.Bounds.Width);
                    if (cluster.Bounds.Left < documentLeft) documentLeft = cluster.Bounds.Left;
                    if (cluster.Bounds.Right > documentRight) documentRight = cluster.Bounds.Right;
                }
            }

            if (widths.Count == 0) return false;
            double documentWidth = documentRight - documentLeft;
            if (documentWidth <= 0.0) return false;

            widths.Sort();
            double medianWidth;
            if (widths.Count % 2 == 1)
                medianWidth = widths[widths.Count / 2];
            else
                medianWidth = (widths[widths.Count / 2 - 1] + widths[widths.Count / 2]) / 2.0;

            return (medianWidth / documentWidth) >= options.WideClusterDocumentRatio;
        }

        private static double CalculateMedianClusterHeight(
            IList<ContextualClusterResolution> resolved)
        {
            List<double> heights = new List<double>();
            for (int i = 0; i < resolved.Count; i++)
                for (int j = 0; j < resolved[i].ResolvedClusters.Count; j++)
                    if (resolved[i].ResolvedClusters[j].Bounds.Height > 0.0)
                        heights.Add(resolved[i].ResolvedClusters[j].Bounds.Height);

            heights.Sort();
            if (heights.Count == 0) return 0.0;
            if (heights.Count % 2 == 1) return heights[heights.Count / 2];
            return (heights[heights.Count / 2 - 1] + heights[heights.Count / 2]) / 2.0;
        }

        private static double CalculatePatternSimilarity(
            IList<double> a, IList<double> b, double scale)
        {
            if (a.Count == 0 && b.Count == 0) return 1.0;
            if (a.Count == 0 || b.Count == 0) return 0.0;

            double tolerance = Math.Max(1.0, scale * 2.0);
            bool[] used = new bool[b.Count];
            int matches = 0;

            for (int i = 0; i < a.Count; i++)
            {
                int best = -1;
                double bestDistance = double.MaxValue;

                for (int j = 0; j < b.Count; j++)
                {
                    if (used[j]) continue;
                    double distance = Math.Abs(a[i] - b[j]);
                    if (distance <= tolerance && distance < bestDistance)
                    {
                        best = j;
                        bestDistance = distance;
                    }
                }

                if (best >= 0)
                {
                    used[best] = true;
                    matches++;
                }
            }

            return (2.0 * matches) / (a.Count + b.Count);
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
