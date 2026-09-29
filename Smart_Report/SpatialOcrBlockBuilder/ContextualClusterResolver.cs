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
        // برای Merge شدن، حداقل دو Row مجاور باید پیوستگی همان بازه افقی را مستقل تأیید کنند.
        // Count به جای نسبت استفاده می‌شود تا Header یا Rowهای انتقالی نزدیک، شاهدهای درست را رقیق نکنند.
        public int MinSupportingRows = 2;

        // برای جلوگیری از Merge کردن جزیره‌های واقعی چپ/راست، حداقل یک همسایه باید
        // همان محدوده افقی دو Cluster را به صورت پیوسته پوشش دهد.
        public int NeighborRadius = 2;
    }

    /// <summary>
    /// Splitهای Horizontal را با شواهد Rowهای مجاور بازبینی می‌کند.
    /// این کلاس متن را نمی‌شناسد؛ Gap اولیه را حذف نمی‌کند، بلکه فقط Split مشکوک را
    /// وقتی Merge می‌کند که ساختار Rowهای همسایه پیوستگی همان بازه افقی را تأیید کنند.
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

                if (original.Count == 2)
                {
                    double support = CalculateNeighborSupport(rows, detector, i, original[0], original[1]);
                    r.NeighborSupport = support;

                    if (support >= options.MinSupportingRows)
                    {
                        r.ResolvedClusters.Add(Merge(original[0], original[1]));
                        r.WasMerged = true;
                    }
                }

                if (!r.WasMerged)
                    for (int j = 0; j < original.Count; j++) r.ResolvedClusters.Add(original[j]);

                result.Add(r);
            }
            return result;
        }

        private double CalculateNeighborSupport(IList<SpatialRow> rows,
            HorizontalClusterDetector detector, int rowIndex,
            SpatialCluster left, SpatialCluster right)
        {
            int supportingRows = 0;

            for (int distance = 1; distance <= options.NeighborRadius; distance++)
            {
                int before = rowIndex - distance;
                int after = rowIndex + distance;
                if (before >= 0)
                    EvaluateNeighbor(rows, detector, before, left, right, ref supportingRows);
                if (after < rows.Count)
                    EvaluateNeighbor(rows, detector, after, left, right, ref supportingRows);
            }

            return (double)supportingRows;
        }

        private static void EvaluateNeighbor(IList<SpatialRow> rows,
            HorizontalClusterDetector detector, int index,
            SpatialCluster left, SpatialCluster right,
            ref int supportingRows)
        {
            List<SpatialCluster> neighbor = detector.Detect(rows[index]);

            // یک Cluster همسایه باید از داخل Cluster چپ تا داخل Cluster راست امتداد داشته باشد.
            // این شرط Gap بزرگ واقعی مثل دو پنل مستقل را Merge نمی‌کند.
            for (int i = 0; i < neighbor.Count; i++)
            {
                SpatialBounds b = neighbor[i].Bounds;
                bool reachesLeft = b.Left <= left.Bounds.Right;
                bool reachesRight = b.Right >= right.Bounds.Left;
                if (reachesLeft && reachesRight)
                {
                    supportingRows++;
                    return;
                }
            }
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
