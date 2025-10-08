using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.ReactiveUI;
using DrumBuddy.Crash;

namespace DrumBuddy.Desktop;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    //TODO: handle app crashes at top level
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
            //TODO: subscribe to reactive command thrown exceptions
            var lastCrash = CrashService.GetCrashData();
            if (CrashService.SetCrashData(ex))
                if (lastCrash == null || lastCrash.CrashDate < DateTimeOffset.UtcNow - TimeSpan.FromSeconds(10))
                    try
                    {
                        Process.Start(typeof(Program).Assembly.Location.Replace(".dll", ".exe"));
                    }
                    catch (Exception exc)
                    {
                        ;
                    }
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