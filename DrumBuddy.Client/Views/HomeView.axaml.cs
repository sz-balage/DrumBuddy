using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Client.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
    }
}