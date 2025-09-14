using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Abstractions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public sealed partial class ManualViewModel : ReactiveObject, IRoutableViewModel
{
    public const int Columns = 16; // one measure, 16 sixteenth steps
    private readonly List<bool[,]> _measureSteps;
    private readonly ISheetStorage _sheetStorage;
    private Sheet? _currentSheet;
    private int _currentMeasureIndex;
    private Bpm _bpm;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    public readonly ReadOnlyObservableCollection<Sheet> Sheets;
    [Reactive] private ManualEditorViewModel? _editor;
    [Reactive] private string? _name;
    [Reactive] private string? _description;
    [Reactive] private decimal _bpmDecimal;
    [Reactive] private bool _editorVisible;
    [Reactive] private bool _sheetListVisible;

    public Sheet? CurrentSheet
    {
        get => _currentSheet;
        private set => this.RaiseAndSetIfChanged(ref _currentSheet, value);
    }

    public int CurrentMeasureIndex
    {
        get => _currentMeasureIndex;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentMeasureIndex, value);
            this.RaisePropertyChanged(nameof(CanGoBack));
            this.RaisePropertyChanged(nameof(CanGoForward));
            this.RaisePropertyChanged(nameof(MeasureDisplayText));
        }
    }

    public bool CanGoBack => CurrentMeasureIndex > 0;
    public bool CanGoForward => CurrentMeasureIndex < _measureSteps?.Count - 1;
    public string MeasureDisplayText => $"Measure {CurrentMeasureIndex + 1} of {_measureSteps.Count}";

    public readonly ReadOnlyObservableCollection<MeasureViewModel> Measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();

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

    [ReactiveCommand]
    private void AddNewSheet()
    {
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
        Editor = new ManualEditorViewModel(HostScreen, () => { EditorVisible = false; }); //TODO: implement actual onclose action
        Editor.LoadSheet(sheet);
        EditorVisible = true;
        SheetListVisible = false;
    }
    public async Task LoadExistingSheets()
    {
        var sheets = await _sheetStorage.LoadSheetsAsync();
        foreach (var sheet in sheets)
        {
            _sheetSource.AddOrUpdate(sheet);
        }
    }
}