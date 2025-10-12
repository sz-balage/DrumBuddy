using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DrumBuddy.Core.Services;
using DrumBuddy.DesignHelpers;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Data.Storage;
using DrumBuddy.IO.Services;
using DrumBuddy.Services;
using DrumBuddy.ViewModels;
using DrumBuddy.Views;
using ReactiveUI;
using Splat;
using static Splat.Locator;

namespace DrumBuddy;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
            RegisterDesignTimeServices();
        else
            RegisterProdServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Locator.Current.GetService<MainWindow>();
            if (desktop.MainWindow.DataContext is MainViewModel vm) vm.SelectedPaneItem = vm.PaneItems[0];
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterDesignTimeServices()
    {
        CurrentMutable.Register(
            () => new LibraryView { ViewModel = Locator.Current.GetRequiredService<DesignLibraryViewModel>() },
            typeof(IViewFor<ILibraryViewModel>));
    }

    private static void RegisterProdServices()
    {
        RegisterCoreServices();
        RegisterIOServices();
        CurrentMutable.Register(() =>
            new FileStorageInteractionService(
                Locator.Current.GetRequiredService<SerializationService>(),
                Locator.Current.GetRequiredService<ConfigurationService>()
            ));
        CurrentMutable.RegisterConstant(new MainViewModel(
            Locator.Current.GetRequiredService<MidiService>()));
        CurrentMutable.RegisterConstant(new PdfGenerator());
        CurrentMutable.RegisterConstant<IScreen>(Locator.Current.GetService<MainViewModel>());
        CurrentMutable.RegisterConstant(new MainWindow());
        CurrentMutable.RegisterConstant(new HomeViewModel());
        CurrentMutable.RegisterConstant(new LibraryViewModel(Locator.Current.GetRequiredService<IScreen>(),
            Locator.Current.GetRequiredService<SheetStorage>(),
            Locator.Current.GetRequiredService<PdfGenerator>(),
            Locator.Current.GetRequiredService<FileStorageInteractionService>()));
        CurrentMutable.RegisterConstant(new ManualViewModel(Locator.Current.GetRequiredService<IScreen>()));
        CurrentMutable.RegisterConstant(
            new ConfigurationViewModel(Locator.Current.GetRequiredService<IScreen>(),
                Locator.Current.GetRequiredService<MidiService>(),
                Locator.Current.GetRequiredService<ConfigurationService>()));
        CurrentMutable.Register(() =>
            new RecordingViewModel(Locator.Current.GetRequiredService<IScreen>(),
                Locator.Current.GetRequiredService<MidiService>(),
                Locator.Current.GetRequiredService<ConfigurationService>(),
                Locator.Current.GetRequiredService<SheetStorage>(),
                Locator.Current.GetRequiredService<MetronomePlayer>()));

        CurrentMutable.Register(() => new HomeView { ViewModel = Locator.Current.GetRequiredService<HomeViewModel>() },
            typeof(IViewFor<HomeViewModel>));
        CurrentMutable.Register(
            () => new RecordingView { ViewModel = Locator.Current.GetRequiredService<RecordingViewModel>() },
            typeof(IViewFor<RecordingViewModel>));
        CurrentMutable.Register(
            () => new LibraryView { ViewModel = Locator.Current.GetRequiredService<LibraryViewModel>() },
            typeof(IViewFor<ILibraryViewModel>));
        CurrentMutable.Register(
            () => new ManualView { ViewModel = Locator.Current.GetRequiredService<ManualViewModel>() },
            typeof(IViewFor<ManualViewModel>));
        CurrentMutable.Register(
            () => new ConfigurationView { ViewModel = Locator.Current.GetRequiredService<ConfigurationViewModel>() },
            typeof(IViewFor<ConfigurationViewModel>));
    }

    private static void RegisterCoreServices()
    {
        CurrentMutable.RegisterConstant<SerializationService>(new SerializationService());
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "IO is the correct term here.")]
    private static void RegisterIOServices()
    {
        var exeDir = AppContext.BaseDirectory;
        var connectionString = $"Data Source={Path.Combine(exeDir, "sheet_db.db")};";
        CurrentMutable.RegisterConstant(new MetronomePlayer(FilePathProvider.GetPathToHighBeepSound(),
            FilePathProvider.GetPathToRegularBeepSound()));
        CurrentMutable.RegisterConstant(
            new SheetStorage(
                Locator.Current.GetRequiredService<SerializationService>(),
                connectionString
            )
        );
        CurrentMutable.RegisterConstant(
            new FileConfigurationStorage(Locator.Current.GetRequiredService<SerializationService>(),
                Path.Combine(FilePathProvider.GetPathForSavedFiles(), "config")));
        CurrentMutable.RegisterConstant(new ConfigurationService(
            Locator.Current.GetRequiredService<FileConfigurationStorage>(),
            Locator.Current.GetRequiredService<MetronomePlayer>()));
        CurrentMutable.RegisterConstant(new MidiService(Locator.Current.GetRequiredService<ConfigurationService>()));
    }
}