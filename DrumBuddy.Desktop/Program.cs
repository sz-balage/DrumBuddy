using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DrumBuddy.ViewModels.Dialogs;
using DrumBuddy.Views.Dialogs;

namespace DrumBuddy.Desktop;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
#if !DEBUG
        try
        {
#endif
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
#if !DEBUG
    
        }
        catch (Exception ex)
        {
            var errorWindow = new ErrorWindow
            {
                DataContext = new ErrorViewModel
                {
                    Title = ex.Message,
                    Description = $"Source: {ex.Source}\n\n{ex.StackTrace}"
                }
            };

            if (Application.Current == null)
                BuildAvaloniaApp().SetupWithoutStarting();

            errorWindow.Show();
            errorWindow.Closed += (_, _) => Environment.Exit(1);
            Dispatcher.UIThread.MainLoop(CancellationToken.None);
        }
#endif
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
    }
}