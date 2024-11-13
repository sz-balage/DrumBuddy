using System;
using System.Reactive.Disposables;
using System.Threading;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Services;
using DrumBuddy.ViewModels;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        private RoutedViewHost _routedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
        private TextBlock _errorTB => this.FindControl<TextBlock>("ErrorMessage");
        private Button _retryButton => this.FindControl<Button>("RetryButton");
        private Border _errorBorder => this.FindControl<Border>("ErrorBorder");
        private MidiService _midiService;
        public MainWindow()
        {
            _midiService = Locator.Current.GetService<MidiService>();
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
                this.Bind(ViewModel, vm => vm.ErrorMessage, v => v._errorTB.Text)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ErrorMessage, v => v._errorBorder.IsVisible,
                        str => !string.IsNullOrEmpty(str))
                    .DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.TryConnectCommand, v => v._retryButton)
                    .DisposeWith(d);
                
            });
            InitializeComponent();
        }
        
    }
}