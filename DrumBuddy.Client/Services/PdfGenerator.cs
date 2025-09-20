using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DrumBuddy.Client.Views.HelperViews;
using DrumBuddy.Core.Models;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace DrumBuddy.Client.Services;

public class PdfGenerator
{
    private const string Path = @"C:\Users\SBB3BP\Documents\DrumBuddy\SavedFiles";

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

        foreach (var measureView in measureViews)
        {
            var bmp = await RenderControlToBitmap(measureView, new Size(1200, 190)); // Adjust size
            using var ms = new MemoryStream();
            bmp.Save(ms);
            ms.Position = 0;

            var img = XImage.FromStream(() => ms);
            var scaledWidth = pageWidth - 2 * margin;
            var scaleFactor = scaledWidth / img.PixelWidth * 72.0 / img.HorizontalResolution;
            var scaledHeight = img.PixelHeight * scaleFactor;

            if (currentY + scaledHeight > pageHeight - margin)
            {
                // New page
                page = doc.AddPage();
                page.Width = pageWidth;
                page.Height = pageHeight;
                gfx = XGraphics.FromPdfPage(page);
                currentY = margin;
            }

            gfx.DrawImage(img, margin, currentY, scaledWidth, scaledHeight);
            currentY += scaledHeight + 10; // 10pt spacing between measures
        }

        try
        {
            var fileName = $"{sheetName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var fullPath = System.IO.Path.Combine(Path, fileName);
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