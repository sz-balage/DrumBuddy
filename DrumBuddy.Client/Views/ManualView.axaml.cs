using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;

namespace DrumBuddy.Client.Views;

public partial class ManualView : ReactiveUserControl<ManualViewModel>
{
    public ManualView()
    {
        InitializeComponent();
    }
}