using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels.HelperViewModels;

namespace DrumBuddy.Views.HelperViews;

public partial class MeasureView : ReactiveUserControl<MeasureViewModel>
{
    public MeasureView()
    {
        InitializeComponent();

    }
}