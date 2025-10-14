using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DrumBuddy.Core.Models;
using DrumBuddy.Views.HelperViews;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace DrumBuddy.Services;

public class PdfGenerator
{
    private readonly string _saveDirectory;

    public PdfGenerator()
    {
        _saveDirectory = Path.Combine(FilePathProvider.GetPathForSavedFiles(), "pdfs");
        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }

    //TODO: scaling feels off (tested on mac)
    public async Task ExportSheetToPdf(IEnumerable<MeasureView> measureViews,
        string sheetName,
        string sheetDescription,
        Bpm tempo)
    {
        var pageWidth = XUnit.FromMillimeter(210).Point; // A4 width
        var pageHeight = XUnit.FromMillimeter(297).Point; // A4 height
        double margin = 20;

        var doc = new PdfDocument();
        var page = doc.AddPage();
        page.Size = PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);

        var currentY = margin;

        var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
        var subtitleFont = new XFont("Arial", 12, XFontStyle.Regular);

        gfx.DrawString(sheetName, titleFont, XBrushes.Black,
            new XRect(0, currentY, pageWidth, 30), XStringFormats.TopCenter);
        currentY += 40;

        gfx.DrawString($"Tempo: {tempo.Value} BPM", subtitleFont, XBrushes.Black,
            new XRect(margin, currentY, pageWidth - 2 * margin, 20), XStringFormats.TopLeft);
        currentY += 25;

        if (!string.IsNullOrWhiteSpace(sheetDescription))
        {
            gfx.DrawString("Description: " + sheetDescription, subtitleFont, XBrushes.Black,
                new XRect(margin, currentY, pageWidth - 2 * margin, 40), XStringFormats.TopLeft);
            currentY += 50; // Leave space before measures
        }

        foreach (var measureView in measureViews)
        {
            var bmp = await RenderControlToBitmap(measureView, new Size(1200, 200));
            using var ms = new MemoryStream();
            bmp.Save(ms);
            ms.Position = 0;

            var img = XImage.FromStream(() => ms);

            var imgWidthInPoints = img.PixelWidth * 72.0 / img.HorizontalResolution;
            var imgHeightInPoints = img.PixelHeight * 72.0 / img.VerticalResolution;

            var scaledWidth = pageWidth - 2 * margin;
            var scaleFactor = scaledWidth / imgWidthInPoints;

            var finalWidth = imgWidthInPoints * scaleFactor;
            var finalHeight = imgHeightInPoints * scaleFactor;

            if (currentY + finalHeight > pageHeight - margin)
            {
                page = doc.AddPage();
                page.Size = PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
                currentY = margin;
            }

            var xPos = margin + (scaledWidth - finalWidth) / 2;

            gfx.DrawImage(img, xPos, currentY, finalWidth, finalHeight);
            currentY += finalHeight;
        }

        var footerFont = new XFont("Arial", 10, XFontStyle.Italic);
        var footerText = "Made in DrumBuddy";
        var textSize = gfx.MeasureString(footerText, footerFont);

        gfx.DrawString(
            footerText,
            footerFont,
            XBrushes.Gray,
            new XPoint(pageWidth - margin - textSize.Width, pageHeight - margin),
            XStringFormats.TopLeft);

        // --- Save PDF ---
        try
        {
            var fileName = $"{sheetName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var fullPath = Path.Combine(_saveDirectory, fileName);
            doc.Save(fullPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }


    private async Task<Bitmap> RenderControlToBitmap(Control control, Size logicalSize)
    {
        // First measure and arrange the control
        control.Measure(logicalSize);
        control.Arrange(new Rect(logicalSize));
        control.UpdateLayout();

        // Convert logical size (DIPs) to pixel size
        var scaling = control.GetVisualRoot()?.RenderScaling ?? 1.0;
        var pixelSize = new PixelSize(
            (int)(logicalSize.Width * scaling),
            (int)(logicalSize.Height * scaling)
        );

        var rtb = new RenderTargetBitmap(pixelSize, new Vector(96, 96)); // 96 DPI
        await Dispatcher.UIThread.InvokeAsync(() => { rtb.Render(control); });

        return rtb;
    }
}