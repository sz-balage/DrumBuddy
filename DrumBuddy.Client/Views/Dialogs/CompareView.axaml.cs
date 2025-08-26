using System;
using System.Net.Mime;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.DesignHelpers;
using DrumBuddy.Client.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy.Client.Views.Dialogs;

public partial class CompareView : ReactiveWindow<ICompareViewModel>
{
    private ItemsControl _baseSheetItemsControl => this.FindControl<ItemsControl>("BaseSheetItemsControl")!;
    private ItemsControl _comparedSheetItemsControl => this.FindControl<ItemsControl>("ComparedSheetItemsControl")!;
    private TextBlock _baseSheetNameTB => this.FindControl<TextBlock>("BaseSheetNameTB")!;
    private TextBlock _comparedSheetNameTB => this.FindControl<TextBlock>("ComparedSheetNameTB")!;
    private ScrollViewer _baseScrollViewer => this.FindControl<ScrollViewer>("BaseScrollViewer")!;
    private ScrollViewer _comparedScrollViewer => this.FindControl<ScrollViewer>("ComparedScrollViewer")!;

    private bool _isSyncingScroll = false;

    public CompareView()
    {
        if (Design.IsDesignMode)
            ViewModel = new DesignCompareViewModel();
        InitializeComponent();
        this.WhenActivated(d =>
        {
            _baseSheetNameTB.Text = "Base Sheet: " + ViewModel.BaseSheetName + " (Measure count: " + ViewModel.BaseSheetMeasures.Count + ")";
            _comparedSheetNameTB.Text = "Compared Sheet: " + ViewModel.ComparedSheetName + " (Measure count: " + ViewModel.ComparedSheetMeasures.Count + ")";
            this.OneWayBind(ViewModel, vm => vm.BaseSheetMeasures, v => v._baseSheetItemsControl.ItemsSource)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ComparedSheetMeasures, v => v._comparedSheetItemsControl.ItemsSource)
                .DisposeWith(d);
            _baseScrollViewer.ScrollChanged += BaseScrollChanged;
            _comparedScrollViewer.ScrollChanged += ComparedScrollChanged;
        });
    }
    private double _lastBaseOffset = 0;
    private double _lastComparedOffset = 0;

    private void BaseScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_isSyncingScroll) return;

        try
        {
            _isSyncingScroll = true;
            SyncScroll(_baseScrollViewer, ref _lastBaseOffset, _comparedScrollViewer, ref _lastComparedOffset);
        }
        finally
        {
            _isSyncingScroll = false;
        }
    }

    private void ComparedScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_isSyncingScroll) return;

        try
        {
            _isSyncingScroll = true;
            SyncScroll(_comparedScrollViewer, ref _lastComparedOffset, _baseScrollViewer, ref _lastBaseOffset);
        }
        finally
        {
            _isSyncingScroll = false;
        }
    }

    /// <summary>
    /// Keeps two ScrollViewers in sync. 
    /// If source is longer → move target by delta. 
    /// If source is shorter → jump target to same offset.
    /// </summary>
    private void SyncScroll(ScrollViewer source, ref double lastSourceOffset, ScrollViewer target, ref double lastTargetOffset)
    {
        if (source.Extent.Height <= 0 || target.Extent.Height <= 0)
            return;

        double deltaY = source.Offset.Y - lastSourceOffset;
        lastSourceOffset = source.Offset.Y;

        bool sourceIsLonger = source.Extent.Height > target.Extent.Height;

        if (sourceIsLonger)
        {
            // scroll step-by-step
            double newTargetOffset = target.Offset.Y + deltaY;
            newTargetOffset = Math.Max(0, Math.Min(newTargetOffset, target.Extent.Height - target.Viewport.Height));
            target.Offset = new Vector(target.Offset.X, newTargetOffset);

            lastTargetOffset = target.Offset.Y;
        }
        else
        {
            // jump the longer one to match the shorter one
            double targetY = Math.Min(source.Offset.Y, target.Extent.Height - target.Viewport.Height);
            target.Offset = new Vector(target.Offset.X, targetY);

            lastTargetOffset = target.Offset.Y;
        }
    }


}