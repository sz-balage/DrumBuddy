using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels.HelperViewModels;
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
            ViewModel.WhenAnyValue(vm => vm.PointerPosition)
                .Subscribe(d =>
                {
                    _pointer.StartPoint = Pointer.StartPoint.WithX(d);
                    _pointer.EndPoint = Pointer.StartPoint.WithY(d);
                })
                .DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.IsPointerVisible)
                .Subscribe(b => { ; });
            this.Bind(ViewModel, vm => vm.IsPointerVisible, v => v.Pointer.IsVisible);
            this.Bind(ViewModel, vm => vm.Width, v => v.Width);
            //this.OneWayBind(ViewModel, vm => vm.PointerPosition, v => v.Pointer.StartPoint,
            //        d =>
            //        {
            //            return new Point(d, 0);
            //        })
            //    .DisposeWith(d);
            //this.OneWayBind(ViewModel, vm => vm.PointerPosition, v => v.Pointer.EndPoint,
            //        d =>
            //        {
            //            return new Point(0, d);
            //        })
            //    .DisposeWith(d);
        });
    }

    private ItemsControl _itemsControl => this.FindControl<ItemsControl>("ItemsControl");
    private Line _pointer => this.FindControl<Line>("Pointer");
}