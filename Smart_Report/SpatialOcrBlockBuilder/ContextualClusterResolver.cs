using System;
using System.Collections.Generic;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    public sealed class ContextualClusterResolution
    {
        public int RowIndex;
        public readonly List<SpatialCluster> OriginalClusters = new List<SpatialCluster>();
        public readonly List<SpatialCluster> ResolvedClusters = new List<SpatialCluster>();
        public bool WasMerged;
        public double NeighborSupport;
    }

    public sealed class ContextualClusterResolverOptions
    {
        // حداقل دو Row هم‌خانواده باید نشان دهند که Split فعلی فقط حاصل یک Gap غیرعادی است.
        public int MinSupportingRows = 2;

        // همسایه‌های نزدیک برای پیدا کردن تکرار ساختار بررسی می‌شوند؛ متن هیچ نقشی در تصمیم ندارد.
        public int NeighborRadius = 2;

        // موقعیت ستون‌ها نسبت به ارتفاع متن نرمال می‌شود تا Resolution تصویر روی تصمیم اثر نگذارد.
        public double MaxColumnDriftHeightFactor = 2.0;

        // اگر فقط یک Row همسایه ساختار را تأیید کند، Merge فقط برای Gapهای متوسط مجاز است.
        // این شرط Rowهای جدولی کوتاه مثل CEREB/Fetal HR را پوشش می‌دهد ولی پنل‌های دور از هم
        // مثل DR/HR در Sample 2 را به علت Gap بسیار بزرگ به هم نمی‌چسباند.
        public double MaxSingleSupportGapHeightFactor = 16.0;
    }

    /// <summary>
    /// Splitهای افقی را با تکرار هندسی Rowهای مجاور بازبینی می‌کند.
    /// نکته مهم: شاهد لازم نیست خودش یک Cluster پیوسته باشد؛ کافی است Itemهایش
    /// همان دو سوی Split را اشغال کنند. این تفاوت برای جدول‌هایی مهم است که همه Rowها
    /// به علت فاصله ستون‌ها توسط HorizontalClusterDetector چند تکه شده‌اند.
    /// </summary>
    public sealed class ContextualClusterResolver
    {
        private readonly ContextualClusterResolverOptions options;

        public ContextualClusterResolver(ContextualClusterResolverOptions options)
        {
            this.options = options == null ? new ContextualClusterResolverOptions() : options;
        }

        public List<ContextualClusterResolution> Resolve(
            IList<SpatialRow> rows, HorizontalClusterDetector detector)
        {
            List<ContextualClusterResolution> result = new List<ContextualClusterResolution>();
            if (rows == null || detector == null) return result;

            for (int i = 0; i < rows.Count; i++)
            {
                ContextualClusterResolution r = new ContextualClusterResolution();
                r.RowIndex = i;
                List<SpatialCluster> original = detector.Detect(rows[i]);
                for (int j = 0; j < original.Count; j++) r.OriginalClusters.Add(original[j]);

                // هر Boundary بین دو Cluster جداگانه ارزیابی می‌شود.
                // بنابراین الگوریتم فقط به حالت خاص «دقیقاً دو Cluster» محدود نیست.
                if (original.Count > 1)
                {
                    List<bool> mergeBoundary = new List<bool>();
                    for (int b = 0; b < original.Count - 1; b++)
                    {
                        double support = CalculateNeighborSupport(
                            rows, i, original[b], original[b + 1]);
                        if (support > r.NeighborSupport) r.NeighborSupport = support;

                        double normalizedGap = GetNormalizedGap(
                            rows[i], original[b], original[b + 1]);

                        bool strongSupport = support >= options.MinSupportingRows;
                        bool singleSupportWithModerateGap =
                            support >= 1.0 &&
                            normalizedGap <= options.MaxSingleSupportGapHeightFactor;

                        mergeBoundary.Add(strongSupport || singleSupportWithModerateGap);
                    }

                    BuildResolvedClusters(original, mergeBoundary, r.ResolvedClusters);
                    r.WasMerged = r.ResolvedClusters.Count < original.Count;
                }

                if (r.ResolvedClusters.Count == 0)
                    for (int j = 0; j < original.Count; j++) r.ResolvedClusters.Add(original[j]);

                result.Add(r);
            }
            return result;
        }

        private double CalculateNeighborSupport(IList<SpatialRow> rows, int rowIndex,
            SpatialCluster left, SpatialCluster right)
        {
            int supportingRows = 0;
            double scale = GetRowScale(rows[rowIndex]);
            double tolerance = Math.Max(1.0, scale * options.MaxColumnDriftHeightFactor);

            // مرز Split با دو Anchor واقعی تعریف می‌شود: آخرین Item سمت چپ و اولین Item سمت راست.
            // Row همسایه فقط وقتی شاهد است که در نزدیکی هر دو Anchor داده داشته باشد.
            // Anchor سمت چپ را از لبه راست Cluster می‌گیریم، نه Left آخرین Item.
            // دلیل: OCR ممکن است دو مقدار مجاور را در یک Row جدا و در Row همسایه
            // به صورت یک Box پهن برگرداند. در آن حالت Left دو Box برابر نیست ولی
            // Box همسایه همان مرز هندسی را پوشش می‌دهد (نمونه BPD/HC در Sample 1).
            double leftAnchor = left.Bounds.Right;
            double rightAnchor = right.Bounds.Left;

            for (int distance = 1; distance <= options.NeighborRadius; distance++)
            {
                int before = rowIndex - distance;
                int after = rowIndex + distance;
                if (before >= 0 && SupportsBoundary(rows[before], leftAnchor, rightAnchor, tolerance))
                    supportingRows++;
                if (after < rows.Count && SupportsBoundary(rows[after], leftAnchor, rightAnchor, tolerance))
                    supportingRows++;
            }

            return (double)supportingRows;
        }

        private static bool SupportsBoundary(SpatialRow row, double leftAnchor,
            double rightAnchor, double tolerance)
        {
            bool hasLeft = false;
            bool hasRight = false;

            for (int i = 0; i < row.Items.Count; i++)
            {
                SpatialBounds bounds = row.Items[i].Bounds;

                // برای Anchor چپ علاوه بر نزدیکی Left، پوشاندن خود Anchor توسط Box
                // نیز شاهد معتبر است. این کار اختلاف Segmentation OCR بین Rowهای
                // هم‌ساختار را تحمل می‌کند، بدون استفاده از متن یا قواعد پزشکی.
                if (Math.Abs(bounds.Left - leftAnchor) <= tolerance ||
                    (bounds.Left <= leftAnchor + tolerance &&
                     bounds.Right >= leftAnchor - tolerance))
                    hasLeft = true;

                // Anchor راست آغاز Cluster بعدی است؛ نزدیکی Left همان ستون را
                // تشخیص می‌دهد و اجازه نمی‌دهد صرفاً یک Box بسیار پهن دو سمت را تأیید کند.
                if (Math.Abs(bounds.Left - rightAnchor) <= tolerance)
                    hasRight = true;
            }

            return hasLeft && hasRight;
        }

        private static double GetRightMostItemLeft(SpatialCluster cluster)
        {
            double x = double.MinValue;
            for (int i = 0; i < cluster.Items.Count; i++)
                if (cluster.Items[i].Bounds.Left > x) x = cluster.Items[i].Bounds.Left;
            return x;
        }

        private static double GetLeftMostItemLeft(SpatialCluster cluster)
        {
            double x = double.MaxValue;
            for (int i = 0; i < cluster.Items.Count; i++)
                if (cluster.Items[i].Bounds.Left < x) x = cluster.Items[i].Bounds.Left;
            return x;
        }

        private static double GetNormalizedGap(SpatialRow row,
            SpatialCluster left, SpatialCluster right)
        {
            double gap = right.Bounds.Left - left.Bounds.Right;
            if (gap <= 0.0) return 0.0;

            double scale = GetRowScale(row);
            if (scale <= 0.0) scale = 1.0;
            return gap / scale;
        }

        private static double GetRowScale(SpatialRow row)
        {
            if (row != null && row.MedianHeight > 0.0) return row.MedianHeight;
            return 1.0;
        }

        private static void BuildResolvedClusters(IList<SpatialCluster> original,
            IList<bool> mergeBoundary, IList<SpatialCluster> output)
        {
            SpatialCluster current = Clone(original[0]);

            for (int i = 0; i < mergeBoundary.Count; i++)
            {
                if (mergeBoundary[i])
                    current = Merge(current, original[i + 1]);
                else
                {
                    output.Add(current);
                    current = Clone(original[i + 1]);
                }
            }

            output.Add(current);
        }

        private static SpatialCluster Clone(SpatialCluster source)
        {
            SpatialCluster clone = new SpatialCluster();
            clone.GapBefore = source.GapBefore;
            for (int i = 0; i < source.Items.Count; i++) clone.Items.Add(source.Items[i]);
            clone.Bounds = CopyBounds(source.Bounds);
            return clone;
        }

        private static SpatialBounds CopyBounds(SpatialBounds source)
        {
            SpatialBounds b = new SpatialBounds();
            b.Left = source.Left; b.Top = source.Top;
            b.Right = source.Right; b.Bottom = source.Bottom;
            return b;
        }

        private static SpatialCluster Merge(SpatialCluster first, SpatialCluster second)
        {
            SpatialCluster merged = new SpatialCluster();
            merged.GapBefore = first.GapBefore;
            for (int i = 0; i < first.Items.Count; i++) merged.Items.Add(first.Items[i]);
            for (int i = 0; i < second.Items.Count; i++) merged.Items.Add(second.Items[i]);

            merged.Bounds = new SpatialBounds();
            merged.Bounds.Left = Math.Min(first.Bounds.Left, second.Bounds.Left);
            merged.Bounds.Top = Math.Min(first.Bounds.Top, second.Bounds.Top);
            merged.Bounds.Right = Math.Max(first.Bounds.Right, second.Bounds.Right);
            merged.Bounds.Bottom = Math.Max(first.Bounds.Bottom, second.Bounds.Bottom);
            return merged;
        }
    }
}
