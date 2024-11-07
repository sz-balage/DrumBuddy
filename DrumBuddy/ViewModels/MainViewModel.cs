using System;
using DrumBuddy.Helpers.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Services;
using LanguageExt;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels
{
    public partial class MainViewModel : ReactiveObject, IScreen
    {
        [Reactive]
        private bool _isPaneOpen;
        [ReactiveCommand]
        private void TogglePane() => IsPaneOpen = !IsPaneOpen;
        [Reactive]
        private NavigationMenuItemTemplate _selectedPaneItem;
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
        public MainViewModel()
        {
            this.WhenAnyValue(vm => vm.SelectedPaneItem)
                .Subscribe(OnSelectedPaneItemChanged);
        }

    }
}
