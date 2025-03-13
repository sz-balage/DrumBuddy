using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using ReactiveUI;

namespace DrumBuddy.Views.HelperViews;

public partial class RythmicGroupView : ReactiveUserControl<RythmicGroupViewModel>
{
    private Path NotePath => this.FindControl<Path>("NotePath");

    public RythmicGroupView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            // this.OneWayBind(ViewModel, vm => vm.Drawing, v => v._tempEllipse.Data)
            //     .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.NotesImageAndBoundsList, v => v.NoteImagesList.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Lines, v => v.LinesList.ItemsSource).DisposeWith(d);
        });
    }

    private TextBlock DrawRythmicGroup(RythmicGroup rg)
    {
        return new TextBlock
        {
            Text = $"{rg.NoteGroups.Length.ToString()} notes were hit",
            TextWrapping = TextWrapping.Wrap
        };
    }
}