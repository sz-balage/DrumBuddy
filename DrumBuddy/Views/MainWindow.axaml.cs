using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Views
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        private RoutedViewHost _routedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
        public MainWindow()
        {
            ViewModel = new MainViewModel();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Router, v => v._routedViewHost.Router);
            });
            InitializeComponent();
        }
    }
}