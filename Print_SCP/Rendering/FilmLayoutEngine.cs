using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Print_SCP.Rendering
{
    internal static class FilmLayoutEngine
    {
        public static IReadOnlyList<RectangleF> Calculate(
            string format, int imageCount, RectangleF area, float gap)
        {
            string value = (format ?? "STANDARD\\1,1").Trim().ToUpperInvariant();
            string[] parts = value.Split(new[] { '\\' }, 2);
            string type = parts[0];
            string args = parts.Length > 1 ? parts[1] : "";

            if (type == "STANDARD")
            {
                var nums = ParseNumbers(args);
                int columns = nums.Count > 0 ? nums[0] : 1;
                int rows = nums.Count > 1 ? nums[1] : 1;
                return UniformGrid(columns, rows, area, gap);
            }

            if (type == "ROW")
            {
                var counts = ParseNumbers(args);
                if (counts.Count > 0) return Rows(counts, area, gap);
            }

            if (type == "COL")
            {
                var counts = ParseNumbers(args);
                if (counts.Count > 0) return Columns(counts, area, gap);
            }

            // SLIDE, SUPERSLIDE and vendor CUSTOM layouts are configuration-dependent.
            // Use a deterministic best-fit grid as a safe generic fallback.
            return BestFitGrid(Math.Max(1, imageCount), area, gap);
        }

        private static List<int> ParseNumbers(string text)
        {
            return (text ?? "").Split(',')
                .Select(x =>
                {
                    int n;
                    return int.TryParse(x.Trim(), out n) ? Math.Max(1, n) : 0;
                })
                .Where(x => x > 0)
                .ToList();
        }

        private static IReadOnlyList<RectangleF> UniformGrid(
            int columns, int rows, RectangleF area, float gap)
        {
            var result = new List<RectangleF>();
            float w = (area.Width - gap * (columns - 1)) / columns;
            float h = (area.Height - gap * (rows - 1)) / rows;

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    result.Add(new RectangleF(
                        area.X + c * (w + gap),
                        area.Y + r * (h + gap), w, h));

            return result;
        }

        private static IReadOnlyList<RectangleF> Rows(
            IList<int> counts, RectangleF area, float gap)
        {
            var result = new List<RectangleF>();
            int rows = counts.Count;
            float h = (area.Height - gap * (rows - 1)) / rows;

            for (int r = 0; r < rows; r++)
            {
                int columns = counts[r];
                float w = (area.Width - gap * (columns - 1)) / columns;
                for (int c = 0; c < columns; c++)
                    result.Add(new RectangleF(
                        area.X + c * (w + gap),
                        area.Y + r * (h + gap), w, h));
            }

            return result;
        }

        private static IReadOnlyList<RectangleF> Columns(
            IList<int> counts, RectangleF area, float gap)
        {
            var result = new List<RectangleF>();
            int columns = counts.Count;
            float w = (area.Width - gap * (columns - 1)) / columns;

            // COL is column-major by DICOM definition.
            for (int c = 0; c < columns; c++)
            {
                int rows = counts[c];
                float h = (area.Height - gap * (rows - 1)) / rows;
                for (int r = 0; r < rows; r++)
                    result.Add(new RectangleF(
                        area.X + c * (w + gap),
                        area.Y + r * (h + gap), w, h));
            }

            return result;
        }

        private static IReadOnlyList<RectangleF> BestFitGrid(
            int count, RectangleF area, float gap)
        {
            double aspect = area.Width / Math.Max(1.0, area.Height);
            int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count * aspect)));
            int rows = Math.Max(1, (int)Math.Ceiling((double)count / columns));
            return UniformGrid(columns, rows, area, gap).Take(count).ToList();
        }
    }
}