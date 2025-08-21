using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels.Dialogs;

namespace DrumBuddy.Client.Views.Dialogs;

public partial class CompareView : ReactiveWindow<CompareViewModel>
{
    public CompareView()
    {
        InitializeComponent();
    }
}