using Avalonia;
using Avalonia.ReactiveUI;
using System;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using DrumBuddy.ViewModels;
using DrumBuddy.Views;
using ReactiveUI;
using Splat;

namespace DrumBuddy
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().AfterSetup(RegisterServices).StartWithClassicDesktopLifetime(args);
        }

        private static void RegisterServices(AppBuilder appBuilder)
        {
            Locator.CurrentMutable.RegisterConstant(new MidiService());
            Locator.CurrentMutable.RegisterConstant<IMidiService>(Locator.Current.GetService<MidiService>());
            Locator.CurrentMutable.RegisterConstant<IScreen>(new MainViewModel());
            Locator.CurrentMutable.RegisterConstant<MainViewModel>(Locator.Current.GetService<IScreen>() as MainViewModel);
            Locator.CurrentMutable.RegisterConstant(new MainWindow());
            Locator.CurrentMutable.RegisterConstant(new HomeViewModel());
            Locator.CurrentMutable.RegisterConstant(new LibraryViewModel());
            Locator.CurrentMutable.Register(() => new RecordingViewModel());
            
        }
        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp() =>
            // IconProvider.Current.Register<FontAwesomeIconProvider>();
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI()
                .LogToTrace();
    }
}
