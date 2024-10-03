using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels.HelperViewModels;
using ReactiveUI;

namespace DrumBuddy.Views.HelperViews;

public partial class MeasureView : ReactiveUserControl<MeasureViewModel>
{
    private ItemsControl _itemsControl => this.FindControl<ItemsControl>("ItemsControl");
    private Line _pointer => this.FindControl<Line>("Pointer");
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
                    this._pointer.StartPoint = this.Pointer.StartPoint.WithX(d);
                    this._pointer.EndPoint = this.Pointer.StartPoint.WithY(d);
                })
                .DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.IsPointerVisible)
                .Subscribe(b =>
                {
                    ;
                });
            this.Bind(ViewModel, vm => vm.IsPointerVisible, v => v.Pointer.IsVisible);
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
}