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
    private Line _pointer => this.FindControl<Line>("Pointer");
}