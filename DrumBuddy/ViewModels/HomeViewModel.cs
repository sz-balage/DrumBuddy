using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels
{
    public partial class HomeViewModel : ReactiveObject, IRoutableViewModel
    {
        [Reactive]
        private string _welcomeText = "Welcome to DrumBuddy!";
        [Reactive]
        private string _subText = "To continue, please select an option from the menu on the left.";
        public HomeViewModel(IScreen host = null)
        {
            HostScreen = host ?? Locator.Current.GetService<IScreen>();
        }
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }
    }
}
