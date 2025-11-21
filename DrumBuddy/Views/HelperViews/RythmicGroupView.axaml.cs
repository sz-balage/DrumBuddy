using System.Reactive.Disposables;
using Avalonia;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels.HelperViewModels;
using ReactiveUI;

namespace DrumBuddy.Views.HelperViews;

public partial class RythmicGroupView : ReactiveUserControl<RythmicGroupViewModel>
{
    public static readonly StyledProperty<double> LineOverlayOpacityProperty =
        AvaloniaProperty.Register<RythmicGroupView, double>(nameof(LineOverlayOpacity), 1.0);

    public static readonly StyledProperty<double> NoteOverlayOpacityProperty =
        AvaloniaProperty.Register<RythmicGroupView, double>(nameof(LineOverlayOpacity), 1.0);

    public RythmicGroupView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.NotesImageAndBoundsList, v => v.NoteImagesList.ItemsSource)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.LinesCollection, v => v.LinesList.ItemsSource)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Width, v => v.Width);
        });
    }

    public double LineOverlayOpacity
    {
        get => GetValue(LineOverlayOpacityProperty);
        set => SetValue(LineOverlayOpacityProperty, value);
    }

    public double NoteOverlayOpacity
    {
        get => GetValue(NoteOverlayOpacityProperty);
        set => SetValue(NoteOverlayOpacityProperty, value);
    }
}