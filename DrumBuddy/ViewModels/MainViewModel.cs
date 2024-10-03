using System;
using DrumBuddy.Helpers.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
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
        private ListItemTemplate _selectedPaneItem;
        private void OnSelectedPaneItemChanged(ListItemTemplate? value)
        {
            if (value is null)
                return;
            if (Router.GetCurrentViewModel()?.GetType() == value.ModelType)
                return;
            IsPaneOpen = false;
            Router.Navigate.Execute(Locator.Current.GetService(value.ModelType) as IRoutableViewModel);
        }
        public ObservableCollection<ListItemTemplate> PaneItems { get; } = new()
        {
            new(typeof(HomeViewModel),"HomeIcon"),
            new(typeof(RecordingViewModel),"RecordIcon")
        };
        public RoutingState Router { get; } = new();
        public MainViewModel()
        {
            this.WhenAnyValue(vm => vm.SelectedPaneItem)
                .Subscribe(OnSelectedPaneItemChanged);
        }
    }
}
