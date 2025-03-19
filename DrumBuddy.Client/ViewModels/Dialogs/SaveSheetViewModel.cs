using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Core.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels.Dialogs;

public partial class SaveSheetViewModel : ReactiveObject
{
    private IObservable<bool> _saveSheetCanExecute;
    private readonly LibraryViewModel _library;
    private readonly SheetCreationData _sheetCreationData;
    [Reactive] private string _sheetName;
    [Reactive] private string _sheetDescription = "";

    public SaveSheetViewModel(SheetCreationData sheetCreationData)
    {
        _library = Locator.Current.GetRequiredService<LibraryViewModel>();
        _sheetCreationData = sheetCreationData;
        _saveSheetCanExecute = this.WhenAnyValue(vm => vm.SheetName).Select(s => !string.IsNullOrEmpty(s));
    }

    [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
    private async Task SaveSheet()
    {
        _sheetName = _sheetName.Trim();
        Sheet sheetToSave = new(_sheetCreationData.Bpm, _sheetCreationData.Measures, _sheetName, _sheetDescription);
        await _library.TrySaveSheet(sheetToSave);
    }
}