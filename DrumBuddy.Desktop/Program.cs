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
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
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

            if (Application.Current == null) BuildAvaloniaApp().SetupWithoutStarting();

            errorWindow.Show();
            errorWindow.Closed += (_, _) => Environment.Exit(1);
            Dispatcher.UIThread.MainLoop(CancellationToken.None);
        }
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        // IconProvider.Current.Register<FontAwesomeIconProvider>();
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
    }
}