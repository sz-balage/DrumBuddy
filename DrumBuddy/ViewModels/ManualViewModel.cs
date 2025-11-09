using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Services;
using DrumBuddy.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public sealed partial class ManualViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private readonly SheetService _sheetService;
    public readonly ReadOnlyObservableCollection<Sheet> Sheets;
    [Reactive] private ManualEditorViewModel? _editor;
    [Reactive] private bool _editorVisible;
    [Reactive] private bool _sheetListVisible;
    [Reactive] private bool _isLoadingSheets;

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
    
    public async Task LoadExistingSheets()
    {
        IsLoadingSheets = true;
        var sheets = await _sheetService.LoadSheetsAsync(); //TODO: figure this out
        foreach (var sheet in sheets) _sheetSource.AddOrUpdate(sheet);
        IsLoadingSheets = false;
        if (sheets.Length == 0)
            AddNewSheet();
        
    }
}