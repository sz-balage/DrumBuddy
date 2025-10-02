using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public sealed partial class ManualViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private readonly ISheetStorage _sheetStorage;
    public readonly ReadOnlyObservableCollection<Sheet> Sheets;
    [Reactive] private ManualEditorViewModel? _editor;
    [Reactive] private bool _editorVisible;
    [Reactive] private bool _sheetListVisible;
    [Reactive] private bool _isLoadingSheets;

    public ManualViewModel(IScreen host)
    {
        _sheetStorage = Locator.Current.GetRequiredService<ISheetStorage>();
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
        Editor = new ManualEditorViewModel(HostScreen, Locator.Current.GetRequiredService<ISheetStorage>(),
            Locator.Current.GetRequiredService<NotificationService>(),
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
        Editor = new ManualEditorViewModel(HostScreen, Locator.Current.GetRequiredService<ISheetStorage>(),
            Locator.Current.GetRequiredService<NotificationService>(),
            () => OnClose());
        Editor.LoadSheet(sheet);
        EditorVisible = true;
        SheetListVisible = false;
    }
    
    public async Task LoadExistingSheets()
    {
        IsLoadingSheets = true;
        var sheets = await _sheetStorage.LoadSheetsAsync();
        foreach (var sheet in sheets) _sheetSource.AddOrUpdate(sheet);
        IsLoadingSheets = false;
        if (sheets.Length == 0)
            AddNewSheet();
        
    }
}