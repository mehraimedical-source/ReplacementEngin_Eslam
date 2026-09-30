using System;
using System.Collections.Generic;
using System.Drawing;

namespace Print_SCP.Rendering
{
    internal static class FilmSizeResolver
    {
        private static readonly Dictionary<string, SizeF> Sizes =
            new Dictionary<string, SizeF>(StringComparer.OrdinalIgnoreCase)
            {
                ["8INX10IN"] = new SizeF(203.2f, 254.0f),
                ["8_5INX11IN"] = new SizeF(215.9f, 279.4f),
                ["10INX12IN"] = new SizeF(254.0f, 304.8f),
                ["10INX14IN"] = new SizeF(257.0f, 364.0f),
                ["11INX14IN"] = new SizeF(279.4f, 355.6f),
                ["11INX17IN"] = new SizeF(279.4f, 431.8f),
                ["14INX14IN"] = new SizeF(355.6f, 355.6f),
                ["14INX17IN"] = new SizeF(355.6f, 431.8f),
                ["24CMX24CM"] = new SizeF(240.0f, 240.0f),
                ["24CMX30CM"] = new SizeF(240.0f, 300.0f),
                ["A4"] = new SizeF(210.0f, 297.0f),
                ["A3"] = new SizeF(297.0f, 420.0f)
            };

        public static SizeF Resolve(string filmSizeId, string orientation)
        {
            SizeF size;
            if (!Sizes.TryGetValue((filmSizeId ?? "").Trim(), out size))
                size = Sizes["A4"];

            if (string.Equals(orientation, "LANDSCAPE", StringComparison.OrdinalIgnoreCase)
                && size.Height > size.Width)
                return new SizeF(size.Height, size.Width);

            if (string.Equals(orientation, "PORTRAIT", StringComparison.OrdinalIgnoreCase)
                && size.Width > size.Height)
                return new SizeF(size.Height, size.Width);

            return size;
        }

        public static int MmToPixels(float mm, int dpi)
        {
            return Math.Max(1, (int)Math.Round(mm / 25.4f * dpi));
        }
    }
}