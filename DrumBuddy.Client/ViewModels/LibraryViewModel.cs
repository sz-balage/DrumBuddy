using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Abstractions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class LibraryViewModel : ReactiveObject, ILibraryViewModel
{
    private readonly ReadOnlyObservableCollection<Sheet> _sheets;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private IObservable<bool> _removeCanExecute;
    private readonly ISheetStorage _sheetStorage;

    public LibraryViewModel(IScreen hostScreen, ISheetStorage sheetStorage)
    {
        HostScreen = hostScreen;
        _sheetStorage = sheetStorage;
        _sheetSource.Connect()
            .SortBy(s => s.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _sheets)
            .Subscribe();
        // _sheetSource.AddOrUpdate(new Sheet(new Bpm(100), ImmutableArray<Measure>.Empty, "New Sheet", "New Sheet"));
        _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null!);
        this.WhenNavigatedTo(() =>
        {
            return LoadSheets()
                .ToObservable()
                .Subscribe();
        });
    }

    [Reactive] private Sheet _selectedSheet;

    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }

    public async Task SaveSheet(Sheet sheet)
    {
        await _sheetStorage.SaveSheetAsync(sheet);
    }

    //TODO: implement rename sheet

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
    private async Task RenameSheet()
    {
        var newSheet = await ShowRenameDialog.Handle(_selectedSheet);
        if(newSheet != _selectedSheet)
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
        }
    }
    public async Task CompareSheets(Sheet baseSheet, Sheet comparedSheet)
    {
        var u = await ShowCompareDialog.Handle((baseSheet, comparedSheet));
    }
    private async Task LoadSheets()
    {
        var sheets = await _sheetStorage.LoadSheetsAsync();
        _sheetSource.Clear();
        _sheetSource.AddOrUpdate(sheets);
    }
    public Interaction<Sheet, Sheet> ShowRenameDialog { get; } = new();
    public Interaction<Sheet, Sheet?> ShowEditDialog { get; } = new();
    public Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; } = new();

    public bool SheetExists(string sheetName)
    {
        return _sheetStorage.SheetExists(sheetName);
    }
}

public interface ILibraryViewModel : IRoutableViewModel
{
    ReadOnlyObservableCollection<Sheet> Sheets { get; }
    ReactiveCommand<Unit, Unit> RemoveSheetCommand { get; }
    ReactiveCommand<Unit, Unit> RenameSheetCommand { get; }
    ReactiveCommand<Unit, Unit> EditSheetCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToRecordingViewCommand { get; }
    Sheet? SelectedSheet { get; set; } 
    bool SheetExists(string sheetName);
    Task SaveSheet(Sheet sheet);
    Interaction<Sheet, Sheet?> ShowEditDialog { get; }
    Interaction<Sheet, Sheet> ShowRenameDialog { get; }
    Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; }
    Task CompareSheets(Sheet baseSheet, Sheet comparedSheet);
}