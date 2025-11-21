using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using DrumBuddy.Api;
using DrumBuddy.Api.Refit;
using DrumBuddy.Core.Services;
using DrumBuddy.DesignHelpers;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Services;
using DrumBuddy.IO.Storage;
using DrumBuddy.Services;
using DrumBuddy.ViewModels;
using DrumBuddy.Views;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using Refit;
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

    public void SetTheme(ThemeVariant theme)
    {
        RequestedThemeVariant = theme;

        if (theme == ThemeVariant.Dark)
        {
            Resources["HeaderGray"] = new SolidColorBrush(Color.Parse("#333333"));
            Resources["CardItemColor"] = new SolidColorBrush(Color.Parse("#1E1E1E"));
            Resources["LighterGray"] = new SolidColorBrush(Color.Parse("#444444"));
            Resources["DarkerGray"] = new SolidColorBrush(Color.Parse("#555555"));
            Resources["Primary"] = Color.Parse("#146090");
            Resources["Secondary"] = Color.Parse("#1C90D9");
            Resources["ButtonPointerOverBackground"] = Color.Parse("#5C5C5C");
            Resources["NoteColor"] = Color.Parse("#FFFFFF");
        }
        else
        {
            Resources["HeaderGray"] = new SolidColorBrush(Color.Parse("#CCCCCC"));
            Resources["CardItemColor"] = new SolidColorBrush(Color.Parse("#f5f5f5"));
            Resources["LighterGray"] = new SolidColorBrush(Color.Parse("#ffe6e6e6"));
            Resources["DarkerGray"] = new SolidColorBrush(Color.Parse("#ADADAD"));
            Resources["Primary"] = Color.Parse("#81C4EE");
            Resources["Secondary"] = Color.Parse("#C9E6F8");
            Resources["ButtonPointerOverBackground"] = Color.Parse("#F5F5F5");
            Resources["NoteColor"] = Color.Parse("#000000");
        }
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
        var tokenService = new UserService(
            Locator.Current.GetRequiredService<SheetRepository>());
        CurrentMutable.Register(() => tokenService, typeof(UserService));


        //TODO: handle dev and prod base addresses
#if DEBUG
        var baseAddress = new Uri("https://api.dev.drumbuddy.hu"); // dev backend
#else
        var baseAddress = new Uri("https://api.drumbuddy.hu"); // production backend
#endif

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };
        var authHandler = new AuthHeaderHandler(tokenService)
        {
            InnerHandler = new HttpClientHandler()
        };

        var loggingHandler = new LoggingHandler { InnerHandler = authHandler };
        var httpClient = new HttpClient(loggingHandler) { BaseAddress = baseAddress };

        var authApi =
            RestService.For<IAuthApi>(new HttpClient(authHandler) { BaseAddress = baseAddress }, refitSettings);
        var sheetApi =
            RestService.For<ISheetApi>(new HttpClient(authHandler) { BaseAddress = baseAddress }, refitSettings);
        var configApi = RestService.For<IConfigurationApi>(httpClient, refitSettings);

        CurrentMutable.Register(
            () => new ApiClient(authApi, sheetApi, configApi, tokenService,
                Locator.Current.GetRequiredService<SerializationService>()),
            typeof(ApiClient));
        CurrentMutable.RegisterConstant(new ConfigurationService(
            Locator.Current.GetRequiredService<ConfigurationRepository>(),
            Locator.Current.GetRequiredService<MetronomePlayer>(),
            Locator.Current.GetRequiredService<UserService>(),
            Locator.Current.GetRequiredService<ApiClient>()
        ));
        CurrentMutable.RegisterConstant(new SheetService(
            Locator.Current.GetRequiredService<SheetRepository>(),
            Locator.Current.GetRequiredService<UserService>(),
            Locator.Current.GetRequiredService<ApiClient>()
        ));
        CurrentMutable.Register(() =>
            new FileStorageInteractionService(
                Locator.Current.GetRequiredService<SerializationService>(),
                Locator.Current.GetRequiredService<MidiService>(),
                Locator.Current.GetRequiredService<ConfigurationService>()
            ));
        CurrentMutable.RegisterConstant(new MainViewModel(
            Locator.Current.GetRequiredService<MidiService>(),
            Locator.Current.GetRequiredService<ConfigurationService>()));
        CurrentMutable.RegisterConstant(new PdfGenerator());
        CurrentMutable.RegisterConstant<IScreen>(Locator.Current.GetService<MainViewModel>());
        CurrentMutable.RegisterConstant(new MainWindow());
        CurrentMutable.RegisterConstant(new NotificationService(
            Locator.Current.GetRequiredService<MainWindow>()), "MainWindowNotificationService");
        CurrentMutable.RegisterConstant(new HomeViewModel());
        CurrentMutable.RegisterConstant(new LibraryViewModel(Locator.Current.GetRequiredService<IScreen>(),
            Locator.Current.GetRequiredService<SheetService>(),
            Locator.Current.GetRequiredService<PdfGenerator>(),
            Locator.Current.GetRequiredService<FileStorageInteractionService>(),
            Locator.Current.GetRequiredService<MidiService>()
        ));
        CurrentMutable.RegisterConstant(new ManualViewModel(Locator.Current.GetRequiredService<IScreen>()));
        CurrentMutable.RegisterConstant(
            new ConfigurationViewModel(Locator.Current.GetRequiredService<IScreen>(),
                Locator.Current.GetRequiredService<MidiService>(),
                Locator.Current.GetRequiredService<ConfigurationService>()));
        CurrentMutable.Register(() =>
            new RecordingViewModel(Locator.Current.GetRequiredService<IScreen>(),
                Locator.Current.GetRequiredService<MidiService>(),
                Locator.Current.GetRequiredService<ConfigurationService>(),
                Locator.Current.GetRequiredService<SheetService>(),
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
            () => new ManualEditorView(),
            typeof(IViewFor<ManualEditorViewModel>)
        );

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
        // var exeDir = AppContext.BaseDirectory;
        // var connectionString = $"Data Source={Path.Combine(exeDir, "sheet_db.db")};";
        // CurrentMutable.RegisterConstant(
        //     new SheetStorage(
        //         Locator.Current.GetRequiredService<SerializationService>(),
        //         connectionString
        //     )
        // );

        var exeDir = AppContext.BaseDirectory;
        var dbPath = Path.Combine(exeDir, "sheet_db.db");
        var connectionString = $"Data Source={dbPath};";

        var dbOptions = new DbContextOptionsBuilder<DrumBuddyDbContext>()
            .UseSqlite(connectionString)
            .Options;

        var dbContext = new DrumBuddyDbContext(dbOptions);

        dbContext.Database.EnsureCreated();

        CurrentMutable.RegisterConstant(dbContext);
        CurrentMutable.RegisterConstant(
            new SheetRepository(
                dbContext,
                Locator.Current.GetRequiredService<SerializationService>()
            )
        );
        CurrentMutable.RegisterConstant(new MetronomePlayer(FilePathProvider.GetPathToHighBeepSound(),
            FilePathProvider.GetPathToRegularBeepSound()));
        CurrentMutable.RegisterConstant(
            new ConfigurationRepository(dbContext, Locator.Current.GetRequiredService<SerializationService>()));
        CurrentMutable.RegisterConstant(new MidiService());
    }
}