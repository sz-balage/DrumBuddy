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
    public class LibraryViewModel : ReactiveObject, IRoutableViewModel
    {
        private ReadOnlyObservableCollection<Sheet> _sheets;
        private SourceCache<Sheet, string> _sheetSource = new(s => s.Name);

        public LibraryViewModel()
        {
            HostScreen = Locator.Current.GetService<IScreen>();
            _sheetSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _sheets)
                .Subscribe();
        }
        public void AddSheet(Sheet sheet)
        {
            _sheetSource.AddOrUpdate(sheet);
        }
        public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }
    }
}
