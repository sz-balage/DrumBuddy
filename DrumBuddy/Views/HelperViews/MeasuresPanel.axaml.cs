using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using DrumBuddy.ViewModels.HelperViewModels;

namespace DrumBuddy.Views.HelperViews;

public partial class MeasuresPanel : UserControl
{
    public static readonly StyledProperty<IEnumerable> MeasuresProperty =
        AvaloniaProperty.Register<MeasuresPanel, IEnumerable>(nameof(Measures));

    private readonly Subject<int> _measurePressedIdx = new();

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
    public IObservable<int> MeasurePressedIdx => _measurePressedIdx.AsObservable();
    public event EventHandler<PointerPressedEventArgs> MeasurePointerPressed;

    private void OnMeasurePointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is MeasureView measureView)
        {
            var index = MeasuresItemControl.ItemContainerGenerator.IndexFromContainer(
                measureView.Parent as Control);
            _measurePressedIdx.OnNext(index);
        }
    }

    public List<MeasureView> GetVisualDescendants()
    {
        return MeasuresItemControl.GetVisualDescendants().OfType<MeasureView>()
            .Where(m => !m.ViewModel.Measure.IsEmpty).ToList();
    }

    public void BringCurrentMeasureIntoView(MeasureViewModel measure)
    {
        var idx = MeasuresItemControl.Items.IndexOf(measure);
        var container = MeasuresItemControl.ContainerFromIndex(idx);
        if (container != null)
            // Scroll the container into view
            container.BringIntoView();
    }
}