using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DrumBuddy.Views.Dialogs;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();
    }

    private void OnReportLinkClicked(object? sender, RoutedEventArgs e)
    {
        const string url = "https://github.com/sz-balage/DrumBuddy/issues/new?template=bug_report.md";
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to open browser: {ex}");
        }
    }
}