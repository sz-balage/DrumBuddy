using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels
{
    public partial class LibraryViewModel : ReactiveObject, IRoutableViewModel
    {
        private ReadOnlyObservableCollection<Sheet> _sheets;
        private SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
        private IObservable<bool> _removeCanExecute;
        public LibraryViewModel()
        {
            HostScreen = Locator.Current.GetService<IScreen>();
            _sheetSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _sheets)
                .Subscribe();
            _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null);
        }
        public void AddSheet(Sheet sheet)
        {
            _sheetSource.AddOrUpdate(sheet);
        }
        [Reactive]
        private Sheet _selectedSheet;
        [ReactiveCommand(CanExecute = nameof(_removeCanExecute))]
        private void RemoveSheet()
        {
            _sheetSource.Remove(_selectedSheet);
        }
        public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }
    }
}
