using DrumBuddy.ViewModels;
using DrumBuddy.Views;
using ReactiveUI;
using System;

namespace DrumBuddy
{
    public class AppViewLocator : ReactiveUI.IViewLocator
    {
        public IViewFor ResolveView<T>(T viewModel, string contract = null) => viewModel switch
        {
            HomeViewModel context => new HomeView() { ViewModel = context },
            RecordingViewModel context => new RecordingView() { ViewModel = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
        };
    }
}
