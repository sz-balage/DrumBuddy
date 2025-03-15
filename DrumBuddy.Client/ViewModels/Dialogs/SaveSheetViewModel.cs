using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Client.ViewModels.Dialogs;

public partial class SaveSheetViewModel : ReactiveObject
{
    private IObservable<bool> _saveSheetCanExecute;

    [Reactive] private string _sheetName;

    public SaveSheetViewModel()
    {
        _saveSheetCanExecute = this.WhenAnyValue(vm => vm.SheetName).Select(s => !string.IsNullOrEmpty(s));
    }

    [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
    private void SaveSheet()
    {
    }
}