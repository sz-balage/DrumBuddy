using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;

namespace DrumBuddy.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
    }
}