using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public partial class LibraryViewModel : ReactiveObject, ILibraryViewModel
{
    //TODO: make sheets exportable/importable to/from files

    private readonly NotificationService _notificationService;
    private readonly PdfGenerator _pdfGenerator;
    private readonly IObservable<bool> _removeCanExecute;
    private readonly ReadOnlyObservableCollection<Sheet> _sheets;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private readonly SheetStorage _sheetStorage;

    [Reactive] private Sheet _selectedSheet;

    public LibraryViewModel(IScreen hostScreen, SheetStorage sheetStorage, NotificationService notificationService,
        PdfGenerator pdfGenerator)
    {
        _notificationService = notificationService;
        _pdfGenerator = pdfGenerator;
        HostScreen = hostScreen;
        _sheetStorage = sheetStorage;
        _sheetSource.Connect()
            .SortBy(s => s.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _sheets)
            .Subscribe();
        // _sheetSource.AddOrUpdate(new Sheet(new Bpm(100), ImmutableArray<Measure>.Empty, "New Sheet", "New Sheet"));
        _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null!);
        this.WhenNavigatedTo(() => LoadSheets()
            .ToObservable()
            .Subscribe());
    }

    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }

    public async Task SaveSheet(Sheet sheet)
    {
        await _sheetStorage.SaveSheetAsync(sheet);
    }

    public async Task CompareSheets(Sheet baseSheet, Sheet comparedSheet)
    {
        _ = await ShowCompareDialog.Handle((baseSheet, comparedSheet));
    }


    public Interaction<Sheet, Sheet> ShowRenameDialog { get; } = new();
    public Interaction<Sheet, Sheet?> ShowEditDialog { get; } = new();
    public Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; } = new();

    public bool SheetExists(string sheetName)
    {
        return _sheetStorage.SheetExists(sheetName);
    }

    [ReactiveCommand(CanExecute = nameof(_removeCanExecute))]
    private async Task RemoveSheet()
    {
        await _sheetStorage.RemoveSheetAsync(_selectedSheet);
        _sheetSource.Remove(_selectedSheet);
    }

    [ReactiveCommand]
    private void NavigateToRecordingView()
    {
        var mainVm = HostScreen as MainViewModel;
        mainVm!.NavigateFromCode(Locator.Current.GetRequiredService<RecordingViewModel>());
    }

    [ReactiveCommand]
    private void NavigateToManualView()
    {
        var mainVm = HostScreen as MainViewModel;
        mainVm!.NavigateFromCode(Locator.Current.GetRequiredService<ManualViewModel>());
    }

    [ReactiveCommand]
    private async Task RenameSheet()
    {
        var newSheet = await ShowRenameDialog.Handle(_selectedSheet);
        if (newSheet != _selectedSheet)
        {
            await _sheetStorage.RenameSheetAsync(SelectedSheet.Name, newSheet);
            _sheetSource.Remove(SelectedSheet);
            _sheetSource.AddOrUpdate(newSheet);
            //SelectedSheet.RenameSheet(dialogResult);
        }
    }

    [ReactiveCommand]
    private async Task EditSheet()
    {
        var editResult = await ShowEditDialog.Handle(SelectedSheet);
        if (editResult != null)
        {
            await _sheetStorage.UpdateSheetAsync(editResult);
            _sheetSource.AddOrUpdate(editResult);
            _notificationService.ShowNotification($"The sheet {editResult.Name} successfully saved.",
                NotificationType.Success);
        }
    }

    private async Task LoadSheets()
    {
        var sheets = await _sheetStorage.LoadSheetsAsync();
        _sheetSource.Clear();
        _sheetSource.AddOrUpdate(sheets);
    }
}

public interface ILibraryViewModel : IRoutableViewModel
{
    ReadOnlyObservableCollection<Sheet> Sheets { get; }
    ReactiveCommand<Unit, Unit> RemoveSheetCommand { get; }
    ReactiveCommand<Unit, Unit> RenameSheetCommand { get; }
    ReactiveCommand<Unit, Unit> EditSheetCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToRecordingViewCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToManualViewCommand { get; }
    Sheet? SelectedSheet { get; set; }
    Interaction<Sheet, Sheet?> ShowEditDialog { get; }
    Interaction<Sheet, Sheet> ShowRenameDialog { get; }
    Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; }
    bool SheetExists(string sheetName);
    Task SaveSheet(Sheet sheet);
    Task CompareSheets(Sheet baseSheet, Sheet comparedSheet);
}