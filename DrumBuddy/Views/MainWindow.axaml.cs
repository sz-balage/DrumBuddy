using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        private RoutedViewHost _routedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
        public MainWindow()
        {
            ViewModel = Locator.Current.GetService<MainViewModel>();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PaneItems, v => v.PaneListBox.ItemsSource)
                    .DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedPaneItem, v => v.PaneListBox.SelectedItem)
                    .DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsPaneOpen, v => v.SplitView.IsPaneOpen)
                    .DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.TogglePaneCommand, v => v.TriggerPaneButton)
                    .DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Router, v => v.RoutedViewHost.Router)
                    .DisposeWith(d);
            });
            InitializeComponent();
        }
    }
}