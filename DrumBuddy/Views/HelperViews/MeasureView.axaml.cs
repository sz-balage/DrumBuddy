using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using DynamicData.Binding;
using ReactiveUI;

namespace DrumBuddy.Views.HelperViews;

public partial class MeasureView : ReactiveUserControl<MeasureViewModel>
{
    public static readonly StyledProperty<bool> IsBeingEditedProperty =
        AvaloniaProperty.Register<MeasureView, bool>(nameof(IsBeingEdited));

    public MeasureView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.RythmicGroups, v => v._itemsControl.ItemsSource)
                .DisposeWith(d);
            ViewModel.EvaluationBoxes.ToObservableChangeSet()
                .Subscribe(_ => DrawEvaluationBoxes())
                .DisposeWith(d);

            ViewModel.WhenAnyValue(vm => vm.PointerPosition)
                .Subscribe(pos =>
                {
                    _pointer.StartPoint = _pointer.StartPoint.WithX(pos);
                    _pointer.EndPoint = _pointer.StartPoint.WithY(190);
                })
                .DisposeWith(d);

            this.Bind(ViewModel, vm => vm.IsPointerVisible, v => v.Pointer.IsVisible)
                .DisposeWith(d);
            this.WhenAnyValue(v => v.IsBeingEdited)
                .Subscribe(isEdited => _editIndicator.IsVisible = isEdited)
                .DisposeWith(d);
        });
        this.GetObservable(BoundsProperty).Subscribe(bounds =>
        {
            if (ViewModel != null)
            {
                ViewModel.Width = bounds.Width;
                ViewModel.Height = bounds.Height;
            }
        });
    }

    public bool IsBeingEdited
    {
        get => GetValue(IsBeingEditedProperty);
        set => SetValue(IsBeingEditedProperty, value);
    }

    private ItemsControl _itemsControl => this.FindControl<ItemsControl>("ItemsControl");
    private Canvas _evalBoxesCanvas => this.FindControl<Canvas>("EvalBoxesCanvas");
    private Line _pointer => this.FindControl<Line>("Pointer");

    private Grid _editIndicator => this.FindControl<Grid>("EditIndicator");
    // private Line _editIndicator2 => this.FindControl<Line>("EditIndicator2");

    private void DrawEvaluationBoxes()
    {
        if (ViewModel is null) return;

        _evalBoxesCanvas.Children.Clear();

        double measureWidth = 1200; // internal logical width before scaling
        var groupWidth = measureWidth / 4.0;
        double boxTop = 90;
        double boxHeight = 80;

        foreach (var box in ViewModel.EvaluationBoxes)
        {
            var left = box.StartRgIndex * groupWidth;
            var width = (box.EndRgIndex - box.StartRgIndex + 1) * groupWidth;
            if (Application.Current?.Resources.TryGetResource("AppGreen", null, out var appleGreenObj) == true &&
                Application.Current?.Resources.TryGetResource("Error", null, out var errorObj) == true)
            {
                var appleGreenBrush = new SolidColorBrush((Color)appleGreenObj);
                var errorBrush = new SolidColorBrush((Color)errorObj);
                var rect = new Rectangle
                {
                    Width = width,
                    Height = boxHeight,
                    Fill = box.State == EvaluationState.Correct
                        ? appleGreenBrush
                        : errorBrush,
                    Opacity = 0.3
                };

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, boxTop);
                _evalBoxesCanvas.Children.Add(rect);
            }
        }
    }
}