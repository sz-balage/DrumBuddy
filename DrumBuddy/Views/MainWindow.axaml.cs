using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.ViewModels;
using DrumBuddy.ViewModels.HelperViewModels;
using DrumBuddy.Views.HelperViews;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private MidiService _midiService;
    private bool isClosingConfirmed;

    public MainWindow()
    {
        _midiService = Locator.Current.GetService<MidiService>();
        ViewModel = Locator.Current.GetService<MainViewModel>();
        Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://DrumBuddy/Assets/app.ico")));
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
            this.OneWayBind(ViewModel, vm => vm.CanRetry, v => v.RetryButton.IsEnabled,
                    string.IsNullOrEmpty)
                .DisposeWith(d);
            // this.Bind(ViewModel, vm => vm.SuccessMessage, v => v.SuccessMessage.Text)
            //     .DisposeWith(d);
            // this.OneWayBind(ViewModel, vm => vm.SuccessMessage, v => v.SuccessBorder.IsVisible,
            //         str => !string.IsNullOrEmpty(str))
            //     .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.TryConnectCommand, v => v._retryButton)
                .DisposeWith(d);
            NotificationPlaceholder.Children.Add(new NotificationHost
                { ViewModel = new NotificationHostViewModel() });
            Closing += async (_, e) =>
            {
                if (isClosingConfirmed)
                    return;
                if (ViewModel?.CurrentViewModel is ManualViewModel manualVm)
                {
                    var editorVm = manualVm.Editor;
                    if (!editorVm?.IsSaved ?? false)
                    {
                        e.Cancel = true;

                        var result = await editorVm.ShowConfirmation.Handle(Unit.Default);

                        if (result == Confirmation.Discard)
                        {
                            isClosingConfirmed = true;
                            Close();
                        }
                    }
                }
            };
        });
        InitializeComponent();
    }

    private RoutedViewHost _routedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
    private Button _retryButton => this.FindControl<Button>("RetryButton");
}