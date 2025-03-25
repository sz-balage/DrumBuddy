using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using ReactiveUI;

namespace DrumBuddy.Client.DesignHelpers;

public class DesignLibraryViewModel : ReactiveObject, ILibraryViewModel
{
    public DesignLibraryViewModel()
    {
        List<Measure> measures = new()
        {
            new Measure([new RythmicGroup([new NoteGroup([new Note(Drum.Crash1,NoteValue.Eighth)])])])
        };
        Sheets = new ReadOnlyObservableCollection<Sheet>(new ObservableCollection<Sheet>
        {
            new(new Bpm(100), [..measures], "New Sheet123", "Test Description 123")
        });
    }
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public ReadOnlyObservableCollection<Sheet> Sheets { get; }
    public ReactiveCommand<Unit, Unit> RemoveSheetCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> RenameSheetCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> EditSheetCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> NavigateToRecordingViewCommand { get; } = ReactiveCommand.Create(() => { });
    public Sheet? SelectedSheet { get; set; }
    public bool SheetExists(string sheetName)
    {
        return true;
    }

    public Task SaveSheet(Sheet sheet)
    {
        return Task.CompletedTask;
    }

    public Interaction<Sheet, Sheet?> ShowEditDialog { get; } = new();

    public Interaction<Sheet, string?> ShowRenameDialog { get; } = new();
}