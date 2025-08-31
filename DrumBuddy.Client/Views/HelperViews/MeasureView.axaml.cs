using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DynamicData.Binding;
using ReactiveUI;

namespace DrumBuddy.Client.Views.HelperViews;

public partial class MeasureView : ReactiveUserControl<MeasureViewModel>
{
    public MeasureView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.RythmicGroups, v => v._itemsControl.ItemsSource)
                .DisposeWith(d); 
            
            // redraw overlays whenever EvaluationBoxes changes
            ViewModel.EvaluationBoxes.ToObservableChangeSet()
                .Subscribe(_ => DrawEvaluationBoxes())
                .DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.PointerPosition)
                .Subscribe(d =>
                {
                    _pointer.StartPoint = Pointer.StartPoint.WithX(d);
                    _pointer.EndPoint = Pointer.StartPoint.WithY(190);
                })
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.IsPointerVisible, v => v.Pointer.IsVisible)
                .DisposeWith(d);;
            this.Bind(ViewModel, vm => vm.Width, v => v.Width)
                .DisposeWith(d);;
            this.Bind(ViewModel, vm => vm.Height, v => v.Height)
                .DisposeWith(d);;
        });
    }

    private ItemsControl _itemsControl => this.FindControl<ItemsControl>("ItemsControl");
    private Canvas _evalBoxesCanvas => this.FindControl<Canvas>("EvalBoxesCanvas");
    private Line _pointer => this.FindControl<Line>("Pointer");
    private void DrawEvaluationBoxes()
    {
        if (ViewModel is null) return;

        _evalBoxesCanvas.Children.Clear();

        double measureWidth = ViewModel.Width;
        double groupWidth = measureWidth / 4.0;
        double boxTop = 90;
        double boxHeight = 80; // covers staff lines (90–170)

        foreach (var box in ViewModel.EvaluationBoxes)
        {
            double left = box.StartRgIndex * groupWidth;
            double width = (box.EndRgIndex - box.StartRgIndex + 1) * groupWidth;

            var rect = new Rectangle
            {
                Width = width,
                Height = boxHeight,
                Fill = box.State == EvaluationState.Correct
                    ? Brushes.Green
                    : Brushes.Red,
                Opacity = 0.3
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, boxTop);
            _evalBoxesCanvas.Children.Add(rect);
        }
    }
}