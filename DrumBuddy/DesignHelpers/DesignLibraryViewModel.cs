using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;
using DrumBuddy.ViewModels;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy.DesignHelpers;

public class DesignLibraryViewModel : ReactiveObject, ILibraryViewModel
{
    private ReadOnlyObservableCollection<SheetViewModel> _sheets;
    private SheetViewModel? _selectedSheet;
    private ReactiveCommand<Unit, Unit> _turnOnSyncForSelectedSheetCommand;
    private ReactiveCommand<Unit, Unit> _turnOffSyncForSelectedSheetCommand;

    public DesignLibraryViewModel()
    {
        List<Measure> measures = new()
        {
            new Measure([new RythmicGroup([new NoteGroup([new Note(Drum.Crash1, NoteValue.Eighth)])])])
        };
        Sheets = new ReadOnlyObservableCollection<Sheet>(new ObservableCollection<Sheet>
        {
            new(new Bpm(100), [..measures], "New Sheet123", "Test Description 123")
        });
    }

    public ReactiveCommand<Unit, Unit> SaveSelectedSheetAsCommand { get; }

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public ReadOnlyObservableCollection<Sheet> Sheets { get; }

    ReadOnlyObservableCollection<SheetViewModel> ILibraryViewModel.Sheets => _sheets;

    public ReactiveCommand<Unit, Unit> RemoveSheetCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> RenameSheetCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> EditSheetCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> ManuallyEditSheetCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToRecordingViewCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> NavigateToManualViewCommand { get; } = ReactiveCommand.Create(() => { });

    SheetViewModel? ILibraryViewModel.SelectedSheet
    {
        get => _selectedSheet;
        set => _selectedSheet = value;
    }

    public Sheet? SelectedSheet { get; set; }

    public bool SheetExists(string sheetName)
    {
        return true;
    }

    public Task SaveSheet(Sheet sheet)
    {
        return Task.CompletedTask;
    }

    public Task SaveSelectedSheetAs(SaveFormat format)
    {
        return Task.CompletedTask;
    }

    public Interaction<Sheet, Sheet?> ShowEditDialog { get; } = new();

    public Interaction<Sheet, Sheet> ShowRenameDialog { get; } = new();
    public Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; }
    public Interaction<ConfirmationViewModel, Confirmation> ShowConfirmationDialog { get; }
    public ReactiveCommand<Unit, Unit> DuplicateSheetCommand { get; set; }

    public Task CompareSheets(Sheet baseSheet, Sheet comparedSheet)
    {
        return Task.CompletedTask;
    }

    public Task BatchRemoveSheets(List<SheetViewModel> selected)
    {
        throw new System.NotImplementedException();
    }

    public void BatchExportSheets(List<SheetViewModel> selected, SaveFormat saveFormat)
    {
        throw new System.NotImplementedException();
    }

    ReactiveCommand<Unit, Unit> ILibraryViewModel.TurnOnSyncForSelectedSheetCommand => _turnOnSyncForSelectedSheetCommand;

    ReactiveCommand<Unit, Unit> ILibraryViewModel.TurnOffSyncForSelectedSheetCommand => _turnOffSyncForSelectedSheetCommand;

}