using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive.Linq;

namespace DrumBuddy.ViewModels.Dialogs
{
    public partial class SaveSheetViewModel : ReactiveObject
    {
        [Reactive]
        private string _sheetName;
        private IObservable<bool> _saveSheetCanExecute;
        public SaveSheetViewModel()
        {
            _saveSheetCanExecute = this.WhenAnyValue(vm => vm.SheetName).Select(s => !string.IsNullOrEmpty(s));
        }
        [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
        private void SaveSheet() {}
    }
}
