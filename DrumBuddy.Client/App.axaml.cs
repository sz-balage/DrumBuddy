using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DrumBuddy.Client.DesignHelpers;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Client.Views;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Services;
using DrumBuddy.IO;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using static Splat.Locator;
using Splat;

namespace DrumBuddy.Client;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            RegisterDesignTimeServices();
        }
        else
        { 
            RegisterProdServices();
        }
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Locator.Current.GetService<MainWindow>();
            if (desktop.MainWindow.DataContext is MainViewModel vm) vm.SelectedPaneItem = vm.PaneItems[0];
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterDesignTimeServices()
    {
        CurrentMutable.Register(() => new LibraryView { ViewModel = Locator.Current.GetRequiredService<DesignLibraryViewModel>() }, typeof(IViewFor<ILibraryViewModel>));
    }
    private static void RegisterProdServices()
    {
        RegisterCoreServices();
        RegisterIOServices();
        CurrentMutable.RegisterConstant(new MainViewModel(Locator.Current.GetRequiredService<IMidiService>()));
        CurrentMutable.RegisterConstant<IScreen>(Locator.Current.GetService<MainViewModel>());
        CurrentMutable.RegisterConstant(new MainWindow());
        CurrentMutable.RegisterConstant(new HomeViewModel());
        CurrentMutable.RegisterConstant(new LibraryViewModel(Locator.Current.GetRequiredService<IScreen>(),Locator.Current.GetRequiredService<ISheetStorage>()));
        CurrentMutable.RegisterConstant(new ManualViewModel(Locator.Current.GetRequiredService<IScreen>()));
        CurrentMutable.RegisterConstant(new ConfigurationViewModel(Locator.Current.GetRequiredService<IScreen>(),Locator.Current.GetRequiredService<IMidiService>(),Locator.Current.GetRequiredService<ConfigurationService>()));
        CurrentMutable.Register(() =>
            new RecordingViewModel(Locator.Current.GetRequiredService<IScreen>(),
                Locator.Current.GetRequiredService<IMidiService>()));
        
        // HomeViewModel context => new Views.HomeView { ViewModel = context },
        // RecordingViewModel context => new RecordingView { ViewModel = context },
        // ILibraryViewModel context => new LibraryView { ViewModel = context },
        CurrentMutable.Register(() => new HomeView { ViewModel = Locator.Current.GetRequiredService<HomeViewModel>() }, typeof(IViewFor<HomeViewModel>));
        CurrentMutable.Register(() => new RecordingView { ViewModel = Locator.Current.GetRequiredService<RecordingViewModel>() }, typeof(IViewFor<RecordingViewModel>));
        CurrentMutable.Register(() => new LibraryView { ViewModel = Locator.Current.GetRequiredService<LibraryViewModel>() }, typeof(IViewFor<ILibraryViewModel>));
        CurrentMutable.Register(() => new ManualView() { ViewModel = Locator.Current.GetRequiredService<ManualViewModel>() }, typeof(IViewFor<ManualViewModel>));
        CurrentMutable.Register(() => new ConfigurationView() { ViewModel = Locator.Current.GetRequiredService<ConfigurationViewModel>() }, typeof(IViewFor<ConfigurationViewModel>));
    }

    private static void RegisterCoreServices()
    {
        CurrentMutable.RegisterConstant<ISerializationService>(new SerializationService());
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "IO is the correct term here.")]
    private static void RegisterIOServices()
    {
        var connectionString = $"Data Source={Path.Combine(Environment.CurrentDirectory,"sheet_db.db")};";
        CurrentMutable.RegisterConstant<IMidiService>(new MidiService());
        CurrentMutable.RegisterConstant<ISheetStorage>(
            new SheetStorage(
                Locator.Current.GetRequiredService<ISerializationService>(), 
                connectionString
            )
        );
        CurrentMutable.RegisterConstant<FileConfigurationStorage>(new FileConfigurationStorage(Locator.Current.GetRequiredService<ISerializationService>()));
        CurrentMutable.RegisterConstant(new ConfigurationService(
            Locator.Current.GetRequiredService<FileConfigurationStorage>()));
    }

}