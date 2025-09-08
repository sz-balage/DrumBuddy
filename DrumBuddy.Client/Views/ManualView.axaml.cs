using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using ReactiveUI;

namespace DrumBuddy.Client.Views;

public partial class ManualView : ReactiveUserControl<ManualViewModel>
{
    private const int StepCount = ManualViewModel.Columns;


    public ManualView()
    {
        if (Design.IsDesignMode)
        {
            var vm = new ManualViewModel(null);
            vm.LoadSheet(Program.TestSheet);
            ViewModel = vm;
        }
        InitializeComponent();

        this.WhenActivated(d =>
        { 
            //TODO: figure out multiple measures
            this.WhenAnyValue(v => v.ViewModel)
                .WhereNotNull()
                .Subscribe(vm => BuildMatrix(vm))
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasuresItemsControl.ItemsSource)
                .DisposeWith(d);
        });
    }

    private Grid _matrixGrid => this.FindControl<Grid>("MatrixGrid");

    private void BuildMatrix(ManualViewModel vm)
    {
        _matrixGrid.Children.Clear();
        _matrixGrid.ColumnDefinitions.Clear();
        _matrixGrid.RowDefinitions.Clear();

        _matrixGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        for (var c = 0; c < StepCount; c++)
            _matrixGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        _matrixGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var header = new TextBlock
        {
            Text = "Drum \\ Step",
            Margin = new Thickness(0, 0, 8, 8),
            VerticalAlignment = VerticalAlignment.Bottom
        };
        Grid.SetRow(header, 0);
        Grid.SetColumn(header, 0);
        _matrixGrid.Children.Add(header);

        for (var c = 0; c < StepCount; c++)
        {
            var label = new TextBlock
            {
                Text = (c + 1).ToString(),
                Margin = new Thickness(2, 0, 2, 8),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Opacity = 0.75
            };

            if (c % 4 == 0) label.FontWeight = FontWeight.SemiBold;

            Grid.SetRow(label, 0);
            Grid.SetColumn(label, c + 1);
            _matrixGrid.Children.Add(label);
        }

        var rows = vm.Drums.Length;

        for (var r = 0; r < rows; r++)
        {
            _matrixGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            var drumLabel = new TextBlock
            {
                Text = vm.Drums[r].ToString(),
                Margin = new Thickness(0, 4, 8, 4),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(drumLabel, r + 1);
            Grid.SetColumn(drumLabel, 0);
            _matrixGrid.Children.Add(drumLabel);

            for (var c = 0; c < StepCount; c++)
            {
                var btn = new ToggleButton
                {
                    Width = 28,
                    Height = 28,
                    Margin = new Thickness(c % 4 == 0 ? 6 : 2, 4, 2, 4), // little gap before each group of 4
                    Content = null, // clean square
                    IsChecked = vm.GetStep(r, c)
                };

                var rr = r;
                var cc = c;
                btn.Checked += (_, __) => vm.ToggleStep(rr, cc);
                btn.Unchecked += (_, __) => vm.ToggleStep(rr, cc);

                Grid.SetRow(btn, r + 1);
                Grid.SetColumn(btn, c + 1);
                _matrixGrid.Children.Add(btn);
            }
        }
    }
}