using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.DesignHelpers;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy.Views.Dialogs;

public partial class CompareView : ReactiveWindow<ICompareViewModel>
{
    private bool _isSyncingScroll;
    private double _lastBaseOffset;
    private double _lastComparedOffset;

    public CompareView()
    {
        if (Design.IsDesignMode)
            ViewModel = new DesignCompareViewModel();
        InitializeComponent();
        this.WhenActivated(d =>
        {
            _baseSheetNameTB.Text = "Base Sheet: " + ViewModel.BaseSheetName + " (Measure count: " +
                                    ViewModel.BaseSheetMeasures.Count + ")";
            _comparedSheetNameTB.Text = "Compared Sheet: " + ViewModel.ComparedSheetName + " (Measure count: " +
                                        ViewModel.ComparedSheetMeasures.Count + ")";
            this.OneWayBind(ViewModel, vm => vm.CorrectPercentage, v => v._comparedSheetPercentageTB.Text,
                    d1 => $"{Math.Round(d1, 1)}% played correctly.")
                .DisposeWith(d);

            _baseScrollViewer.ScrollChanged += BaseScrollChanged;
            _comparedScrollViewer.ScrollChanged += ComparedScrollChanged;
        });
    }

    private TextBlock _baseSheetNameTB => this.FindControl<TextBlock>("BaseSheetNameTB")!;
    private TextBlock _comparedSheetNameTB => this.FindControl<TextBlock>("ComparedSheetNameTB")!;
    private TextBlock _comparedSheetPercentageTB => this.FindControl<TextBlock>("ComparedSheetPercentageTB")!;
    private ScrollViewer _baseScrollViewer => this.FindControl<ScrollViewer>("BaseScrollViewer")!;
    private ScrollViewer _comparedScrollViewer => this.FindControl<ScrollViewer>("ComparedScrollViewer")!;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Root.Opacity = 1;
    }

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

    private void GridSplitter_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (Content is Grid grid && grid.ColumnDefinitions.Count >= 3)
        {
            // reset both sheet columns to equal widths
            grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
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
    ///     Keeps two ScrollViewers in sync.
    ///     If source is longer → move target by delta.
    ///     If source is shorter → jump target to same offset.
    /// </summary>
    private void SyncScroll(ScrollViewer source, ref double lastSourceOffset, ScrollViewer target,
        ref double lastTargetOffset)
    {
        if (source.Extent.Height <= 0 || target.Extent.Height <= 0)
            return;

        var deltaY = source.Offset.Y - lastSourceOffset;
        lastSourceOffset = source.Offset.Y;

        var sourceIsLonger = source.Extent.Height > target.Extent.Height;

        if (sourceIsLonger)
        {
            // scroll step-by-step
            var newTargetOffset = target.Offset.Y + deltaY;
            newTargetOffset = Math.Max(0, Math.Min(newTargetOffset, target.Extent.Height - target.Viewport.Height));
            target.Offset = new Vector(target.Offset.X, newTargetOffset);

            lastTargetOffset = target.Offset.Y;
        }
        else
        {
            // jump the longer one to match the shorter one
            var targetY = Math.Min(source.Offset.Y, target.Extent.Height - target.Viewport.Height);
            target.Offset = new Vector(target.Offset.X, targetY);

            lastTargetOffset = target.Offset.Y;
        }
    }
}