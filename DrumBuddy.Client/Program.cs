using System;
using System.Collections.Immutable;
using Avalonia;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using Microsoft.Extensions.Configuration;

namespace DrumBuddy.Client;

internal class Program
{
    public static Sheet TestSheet { get; set; } = new Sheet(100, new ImmutableArray<Measure>(){new Measure([new RythmicGroup([new NoteGroup([new Note(Drum.Crash, NoteValue.Eighth)])])])}, "Test", "Test");
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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