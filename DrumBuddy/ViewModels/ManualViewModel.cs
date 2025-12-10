using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public sealed partial class ManualViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly SheetService _sheetService;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    public readonly ReadOnlyObservableCollection<Sheet> Sheets;
    [Reactive] private ManualEditorViewModel? _editor;
    [Reactive] private bool _editorVisible;
    [Reactive] private bool _isLoadingSheets;
    [Reactive] private bool _sheetListVisible;

    public ManualViewModel(IScreen host)
    {
        _sheetService = Locator.Current.GetRequiredService<SheetService>();
        HostScreen = host;
        UrlPathSegment = "manual-editor";
        _sheetSource.Connect()
            .SortBy(s => s.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out Sheets)
            .Subscribe();
        EditorVisible = false;
        SheetListVisible = false;
        this.WhenNavigatedTo(() => LoadExistingSheets()
            .ToObservable()
            .Subscribe());
    }

    public IScreen HostScreen { get; }
    public string? UrlPathSegment { get; }

    private async Task OnClose()
    {
        EditorVisible = false;
        _ = LoadExistingSheets();
    }

    [ReactiveCommand]
    private void AddNewSheet()
    {
        Editor = new ManualEditorViewModel(HostScreen, Locator.Current.GetRequiredService<SheetService>(),
            () => OnClose());
        EditorVisible = true;
    }

    [ReactiveCommand]
    private void EditExistingSheet()
    {
        SheetListVisible = true;
    }

    [ReactiveCommand]
    private void CancelSheetChoosing()
    {
        SheetListVisible = false;
    }

    public void ChooseSheet(Sheet sheet)
    {
        Editor = new ManualEditorViewModel(HostScreen, Locator.Current.GetRequiredService<SheetService>(),
            () => OnClose());
        Editor.LoadSheet(sheet);
        EditorVisible = true;
        SheetListVisible = false;
    }

    public void Reset()
    {
        Editor = null;
        EditorVisible = false;
        SheetListVisible = false;
    }

    public async Task LoadExistingSheets()
    {
        IsLoadingSheets = true;
        _sheetSource.Clear();
        var sheets = await _sheetService.LoadSheetsAsync();
        foreach (var sheet in sheets) _sheetSource.AddOrUpdate(sheet);
        IsLoadingSheets = false;
        if (sheets.Length == 0)
            AddNewSheet();
    }
}