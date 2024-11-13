using System;
using DrumBuddy.Helpers.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Services;
using LanguageExt;
using LanguageExt.Common;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels
{
    public partial class MainViewModel : ReactiveObject, IScreen
    {
        private IMidiService _midiService;
        public MainViewModel()
        {
            _midiService = Locator.Current.GetService<MidiService>();
            _midiService!.InputDeviceDisconnected
                .Subscribe(connected =>
                {
                    NoConnection = true;
                });
            _midiService.TryConnect()
                .Match(
                    Succ: _ => SuccessfulConnection(),
                    Fail: err =>
                    {
                        NoConnection = true;
                        //ConnectionError(err);
                    });
            this.WhenAnyValue(vm => vm.SelectedPaneItem)
                .Subscribe(OnSelectedPaneItemChanged);
            ErrorMessage = string.Empty;
        }
        private void SuccessfulConnection()
        {
            NoConnection = false;
            ConnectedSucc = true;
            var timer = new Timer(_ =>
            {
                Dispatcher.UIThread.Invoke(() => ConnectedSucc = false);

            }, null, TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
        }

        private void ConnectionError(Error error)
        {
            ErrorMessage = error.Message;
            var timer = new Timer(_ =>
            {
                Dispatcher.UIThread.Invoke(() => ErrorMessage = string.Empty);
            }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        }
        [Reactive]
        private bool _isPaneOpen;
        [Reactive]
        private bool _connectedSucc;
        [Reactive]
        private bool _noConnection;
        [ReactiveCommand]
        private void TogglePane() => IsPaneOpen = !IsPaneOpen;
        [Reactive]
        private NavigationMenuItemTemplate _selectedPaneItem;

        [Reactive] 
        private string _errorMessage;
        private void OnSelectedPaneItemChanged(NavigationMenuItemTemplate? value)
        {
            if (value is null)
                return;
            if (Router.GetCurrentViewModel()?.GetType() == value.ModelType)
                return; 
            IsPaneOpen = false;
            Router.NavigateAndReset.Execute(Locator.Current.GetService(value.ModelType) as IRoutableViewModel);
        }
        public ObservableCollection<NavigationMenuItemTemplate> PaneItems { get; } = new()
        {
            new(typeof(HomeViewModel),"HomeIcon"),
            new(typeof(RecordingViewModel),"RecordIcon"),
            new(typeof(LibraryViewModel),"LibraryIcon")
        };
        public RoutingState Router { get; } = new();
        

        [ReactiveCommand]
        private void TryConnect()
        {
            _midiService.TryConnect()
                .Match(
                    Succ: _ => SuccessfulConnection(),
                    Fail: ConnectionError);
        }
      
    }
}
