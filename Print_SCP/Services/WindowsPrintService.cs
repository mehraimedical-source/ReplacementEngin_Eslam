using System;
using System.Drawing;
using System.Drawing.Printing;
using Print_SCP.Models;

namespace Print_SCP.Services
{
    internal static class WindowsPrintService
    {
        public static void Print(RenderedPrintInfo item, string printerName)
        {
            if (item == null || item.Image == null)
                throw new InvalidOperationException("Rendered film image is not available.");

            using (var document = new PrintDocument())
            {
                if (!string.IsNullOrWhiteSpace(printerName))
                    document.PrinterSettings.PrinterName = printerName;

                if (!document.PrinterSettings.IsValid)
                    throw new InvalidOperationException("Windows printer is not valid: " + printerName);

                int widthHundredthsInch =
                    Math.Max(1, (int)Math.Round(item.WidthMm / 25.4 * 100.0));
                int heightHundredthsInch =
                    Math.Max(1, (int)Math.Round(item.HeightMm / 25.4 * 100.0));

                document.DefaultPageSettings.PaperSize =
                    new PaperSize(item.FilmSizeId, widthHundredthsInch, heightHundredthsInch);

                document.DefaultPageSettings.Landscape =
                    string.Equals(item.Orientation, "LANDSCAPE",
                        StringComparison.OrdinalIgnoreCase);

                document.PrintPage += (s, e) =>
                {
                    e.Graphics.DrawImage(
                        item.Image,
                        e.PageBounds,
                        new Rectangle(0, 0, item.Image.Width, item.Image.Height),
                        GraphicsUnit.Pixel);
                    e.HasMorePages = false;
                };

                document.Print();
            }
        }
    }
}