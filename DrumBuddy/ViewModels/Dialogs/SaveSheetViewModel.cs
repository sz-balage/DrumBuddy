using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrumBuddy.ViewModels.Dialogs
{
    public partial class SaveSheetViewModel : ReactiveObject
    {
        [Reactive]
        private string _sheetName;
        private IObservable<bool> _saveSheetCanExecute => SheetName.WhenAnyValue(s => !string.IsNullOrEmpty(s));
        [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
        private void SaveSheet()
        {
            //save the sheet
            ShouldBeSaved = true;
        }
        public bool ShouldBeSaved { get; private set; }
    }
}
