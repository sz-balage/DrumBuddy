using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using DrumBuddy.Views.HelperViews;
using ReactiveUI;
using System.Reactive.Disposables;

namespace DrumBuddy.Views;

public partial class RecordingView : ReactiveUserControl<RecordingViewModel>
{
    private ItemsControl MeasureControl => this.FindControl<ItemsControl>("MeasuresItemControl");
    private MeasureView MeasureView => this.FindControl<MeasureView>("measure");
    public RecordingView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            //MeasureView.ViewModel= new MeasureViewModel();
            // ViewModel.BeatObservableFromUI = Observable.FromEventPattern<RoutedEventArgs>(SnareButton, nameof(SnareButton.Click))
            //     .Select(_ => new Beat(DateTime.Now, DrumType.Snare));

            this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasureControl.ItemsSource)
                .DisposeWith(d);

            // ViewModel.NoteObservable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(notes =>
            // {
            //     foreach (var note in notes)
            //     {
            //         Output.Text = Output.Text += $"A {note.DrumType.ToString()} with {note.Value} value was hit. Timing: {note.Timing}\n";
            //     }
            // });
        });
        AvaloniaXamlLoader.Load(this);
    }
}