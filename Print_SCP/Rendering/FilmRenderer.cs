using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FellowOakDicom.Imaging;
using FellowOakDicom.Printing;
using Print_SCP.Models;

namespace Print_SCP.Rendering
{
    internal static class FilmRenderer
    {
        public static RenderedPrintInfo Render(
            FilmBox filmBox, PrintJobInfo job, int dpi, string outputRoot)
        {
            if (filmBox == null) throw new ArgumentNullException(nameof(filmBox));

            SizeF sizeMm = FilmSizeResolver.Resolve(filmBox.FilmSizeID, filmBox.FilmOrientation);
            int width = FilmSizeResolver.MmToPixels(sizeMm.Width, dpi);
            int height = FilmSizeResolver.MmToPixels(sizeMm.Height, dpi);

            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(dpi, dpi);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Color borderColor = DensityToColor(filmBox.BorderDensity, Color.Black);
                Color emptyColor = DensityToColor(filmBox.EmptyImageDensity, borderColor);
                g.Clear(borderColor);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                float margin = Math.Max(8, FilmSizeResolver.MmToPixels(3f, dpi));
                float gap = Math.Max(3, FilmSizeResolver.MmToPixels(1.5f, dpi));
                var area = new RectangleF(margin, margin,
                    bitmap.Width - 2 * margin, bitmap.Height - 2 * margin);

                int count = filmBox.BasicImageBoxes == null ? 0 : filmBox.BasicImageBoxes.Count;
                var boxes = FilmLayoutEngine.Calculate(
                    filmBox.ImageDisplayFormat, count, area, gap);

                foreach (var imageBox in filmBox.BasicImageBoxes
                    .OrderBy(x => x.ImageBoxPosition))
                {
                    int index = Math.Max(0, imageBox.ImageBoxPosition - 1);
                    if (index >= boxes.Count) continue;

                    RectangleF target = boxes[index];
                    using (var emptyBrush = new SolidBrush(emptyColor))
                        g.FillRectangle(emptyBrush, target);

                    using (Bitmap source = RenderImageBox(imageBox))
                    {
                        if (source != null)
                            DrawImage(g, source, target, imageBox, dpi);
                    }

                    if (string.Equals(filmBox.Trim, "YES", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var pen = new Pen(Color.White, Math.Max(1f, dpi / 150f)))
                            g.DrawRectangle(pen, target.X, target.Y, target.Width, target.Height);
                    }
                }
            }

            string dayFolder = Path.Combine(
                outputRoot,
                DateTime.Now.ToString("yyyy"),
                DateTime.Now.ToString("MM"),
                DateTime.Now.ToString("dd"));

            Directory.CreateDirectory(dayFolder);

            Guid id = Guid.NewGuid();
            string imagePath = Path.Combine(dayFolder, id.ToString("N") + ".png");
            string thumbPath = Path.Combine(dayFolder, id.ToString("N") + "_thumb.jpg");

            bitmap.Save(imagePath, ImageFormat.Png);
            Bitmap thumbnail = CreateThumbnail(bitmap, 220, 280);
            thumbnail.Save(thumbPath, ImageFormat.Jpeg);

            return new RenderedPrintInfo
            {
                Id = id,
                JobId = job.JobId,
                ReceivedAt = DateTime.Now,
                CallingAeTitle = job.CallingAeTitle,
                CalledAeTitle = job.CalledAeTitle,
                RemoteIp = job.RemoteIp,
                FilmSizeId = filmBox.FilmSizeID,
                Orientation = filmBox.FilmOrientation,
                Layout = filmBox.ImageDisplayFormat,
                ImageCount = filmBox.BasicImageBoxes.Count,
                WidthMm = sizeMm.Width,
                HeightMm = sizeMm.Height,
                Dpi = dpi,
                RenderedImagePath = imagePath,
                ThumbnailPath = thumbPath,
                Image = bitmap,
                Thumbnail = thumbnail
            };
        }

        private static Bitmap RenderImageBox(ImageBox imageBox)
        {
            if (imageBox == null || imageBox.ImageSequence == null)
                return null;

            var dicomImage = new DicomImage(imageBox.ImageSequence);
            using (IImage rendered = dicomImage.RenderImage())
            {
                var bitmap = new Bitmap(
                    rendered.Width,
                    rendered.Height,
                    PixelFormat.Format32bppArgb);

                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData data = bitmap.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    Marshal.Copy(
                        rendered.Pixels.Data,
                        0,
                        data.Scan0,
                        rendered.Width * rendered.Height);
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                if (string.Equals(
                    imageBox.Polarity,
                    "REVERSE",
                    StringComparison.OrdinalIgnoreCase))
                {
                    using (var reversed = new Bitmap(
                        bitmap.Width,
                        bitmap.Height,
                        PixelFormat.Format32bppArgb))
                    using (var g = Graphics.FromImage(reversed))
                    using (var attributes = new ImageAttributes())
                    {
                        var matrix = new ColorMatrix(new[]
                        {
                            new[] {-1f,  0f,  0f, 0f, 0f},
                            new[] { 0f, -1f,  0f, 0f, 0f},
                            new[] { 0f,  0f, -1f, 0f, 0f},
                            new[] { 0f,  0f,  0f, 1f, 0f},
                            new[] { 1f,  1f,  1f, 0f, 1f}
                        });
                        attributes.SetColorMatrix(matrix);
                        g.DrawImage(
                            bitmap,
                            rect,
                            0, 0,
                            bitmap.Width,
                            bitmap.Height,
                            GraphicsUnit.Pixel,
                            attributes);

                        bitmap.Dispose();
                        return (Bitmap)reversed.Clone();
                    }
                }

                return bitmap;
            }
        }

        private static void DrawImage(
            Graphics g,
            Bitmap source,
            RectangleF target,
            ImageBox imageBox,
            int dpi)
        {
            InterpolationMode previous = g.InterpolationMode;
            g.InterpolationMode = GetInterpolationMode(imageBox.MagnificationType);

            string behavior = (imageBox.RequestedDecimateCropBehavior ?? "DECIMATE")
                .Trim()
                .ToUpperInvariant();

            RectangleF effectiveTarget = target;

            if (imageBox.RequestedImageSize > 0)
            {
                float requestedWidth =
                    FilmSizeResolver.MmToPixels((float)imageBox.RequestedImageSize, dpi);

                if (requestedWidth > 0 && requestedWidth < effectiveTarget.Width)
                {
                    float ratio = requestedWidth / effectiveTarget.Width;
                    float requestedHeight = effectiveTarget.Height * ratio;
                    effectiveTarget = new RectangleF(
                        effectiveTarget.X + (effectiveTarget.Width - requestedWidth) / 2f,
                        effectiveTarget.Y + (effectiveTarget.Height - requestedHeight) / 2f,
                        requestedWidth,
                        requestedHeight);
                }
            }

            float sx = effectiveTarget.Width / source.Width;
            float sy = effectiveTarget.Height / source.Height;
            float scale = behavior == "CROP" ? Math.Max(sx, sy) : Math.Min(sx, sy);

            float w = source.Width * scale;
            float h = source.Height * scale;
            float x = effectiveTarget.X + (effectiveTarget.Width - w) / 2f;
            float y = effectiveTarget.Y + (effectiveTarget.Height - h) / 2f;

            GraphicsState state = g.Save();
            g.SetClip(effectiveTarget);
            g.DrawImage(source, new RectangleF(x, y, w, h));
            g.Restore(state);

            g.InterpolationMode = previous;
        }

        private static InterpolationMode GetInterpolationMode(string magnificationType)
        {
            switch ((magnificationType ?? "").Trim().ToUpperInvariant())
            {
                case "REPLICATE":
                case "NONE":
                    return InterpolationMode.NearestNeighbor;
                case "BILINEAR":
                    return InterpolationMode.HighQualityBilinear;
                case "CUBIC":
                    return InterpolationMode.HighQualityBicubic;
                default:
                    return InterpolationMode.HighQualityBicubic;
            }
        }

        private static Color DensityToColor(string density, Color fallback)
        {
            string value = (density ?? "").Trim().ToUpperInvariant();

            if (value == "BLACK") return Color.Black;
            if (value == "WHITE") return Color.White;

            int numeric;
            if (int.TryParse(value, out numeric))
            {
                // DICOM density is optical density x 100. Convert to a practical
                // screen/bitmap gray for preview and paper output.
                double od = Math.Max(0.0, numeric / 100.0);
                double transmission = Math.Pow(10.0, -od);
                int level = Math.Max(0, Math.Min(255,
                    (int)Math.Round(255.0 * transmission)));
                return Color.FromArgb(level, level, level);
            }

            return fallback;
        }

        private static Bitmap CreateThumbnail(Image source, int maxWidth, int maxHeight)
        {
            double scale = Math.Min(
                (double)maxWidth / source.Width,
                (double)maxHeight / source.Height);

            int w = Math.Max(1, (int)Math.Round(source.Width * scale));
            int h = Math.Max(1, (int)Math.Round(source.Height * scale));

            var result = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Black);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, new Rectangle(0, 0, w, h));
            }
            return result;
        }
    }
}