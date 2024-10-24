using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;

namespace DrumBuddy.ViewModels.Dialogs
{
    public partial class SaveSheetViewModel : ReactiveObject
    {
        [Reactive]
        private string _sheetName;
        private IObservable<bool> _saveSheetCanExecute => SheetName.WhenAnyValue(s => !string.IsNullOrEmpty(s));
        [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
        private void SaveSheet(){}
        public bool ShouldBeSaved { get; private set; }
    }
}
