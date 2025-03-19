using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Client.Views;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using ReactiveUI;
using Splat;
using static Splat.Locator;

namespace DrumBuddy.Client;

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
        RegisterCoreServices();
        RegisterIOServices();
        CurrentMutable.RegisterConstant(new MainViewModel(Current.GetRequiredService<IMidiService>()));
        CurrentMutable.RegisterConstant<IScreen>(Current.GetService<MainViewModel>());
        CurrentMutable.RegisterConstant(new MainWindow());
        CurrentMutable.RegisterConstant(new HomeViewModel());
        CurrentMutable.RegisterConstant(new LibraryViewModel(Current.GetRequiredService<IScreen>(),Current.GetRequiredService<ISheetStorage>()));
        CurrentMutable.Register(() =>
            new RecordingViewModel(Current.GetRequiredService<IScreen>(),
                Current.GetRequiredService<IMidiService>(),
                Current.GetRequiredService<LibraryViewModel>()));
    }

    private static void RegisterCoreServices()
    {
        CurrentMutable.RegisterConstant<ISerializationService>(new SerializationService());
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "IO is the correct term here.")]
    private static void RegisterIOServices()
    {
        CurrentMutable.RegisterConstant<IMidiService>(new MidiService());
        CurrentMutable.RegisterConstant<ISheetStorage>(new SheetStorage(Current.GetRequiredService<ISerializationService>()));
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