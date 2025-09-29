using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using DrumBuddy.ViewModels.HelperViewModels;

namespace DrumBuddy.Views.HelperViews;

public partial class MeasuresPanel : UserControl
{
    // ✅ Dependency property for binding measures collection
    public static readonly StyledProperty<IEnumerable> MeasuresProperty =
        AvaloniaProperty.Register<MeasuresPanel, IEnumerable>(nameof(Measures));

    public MeasuresPanel()
    {
        InitializeComponent();

        // Bind the Measures property to ItemsControl.ItemsSource
        this.GetObservable(MeasuresProperty)
            .Subscribe(measures => _measuresItemControl.ItemsSource = measures);
    }

    public IEnumerable Measures
    {
        get => GetValue(MeasuresProperty);
        set => SetValue(MeasuresProperty, value);
    }

    private ItemsControl _measuresItemControl => this.FindControl<ItemsControl>("MeasuresItemControl");

    // ✅ Event for measure pointer presses
    public event EventHandler<PointerPressedEventArgs> MeasurePointerPressed;

    private void OnMeasurePointerPressed(object sender, PointerPressedEventArgs e)
    {
        // ✅ Only forward if the source is a MeasureView
        if (e.Source is Control control &&
            (control is MeasureView || control.FindLogicalAncestorOfType<MeasureView>() != null))
            MeasurePointerPressed?.Invoke(this, e);
    }

    public List<MeasureView> GetVisualDescendants()
    {
        return MeasuresItemControl.GetVisualDescendants().OfType<MeasureView>()
            .Where(m => !m.ViewModel.Measure.IsEmpty).ToList();
    }

    public void BringCurrentMeasureIntoView(MeasureViewModel measure)
    {
        var idx = MeasuresItemControl.Items.IndexOf(measure);
        var container = MeasuresItemControl.ContainerFromIndex(idx + 3);
        if (container != null)
            // Scroll the container into view
            container.BringIntoView();
    }
}