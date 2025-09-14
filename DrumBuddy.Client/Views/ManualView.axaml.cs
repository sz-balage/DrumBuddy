using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Client.ViewModels.Dialogs;
using ReactiveUI;
using Splat;

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

        this.WhenActivated(async d =>
        {
            await ViewModel.LoadExistingSheets();
            this.OneWayBind(ViewModel, vm => vm.EditorVisible, v => v.CardGrid.IsVisible, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.EditorVisible, v => v.Root.IsVisible, b => b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasuresItemsControl.ItemsSource)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.MeasureDisplayText, v => v.MeasureDisplayText.Text)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanGoBack, v => v.BackButton.IsEnabled)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanGoForward, v => v.ForwardButton.IsEnabled)
                .DisposeWith(d);  
            this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameTextBlock.Text, s =>
                {
                    if (s is null)
                    {
                        return string.Empty;
                    }
                    return $"({s})";
                }).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.AddMeasureCommand, v => v.AddMeasureButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.GoToPreviousMeasureCommand, v => v.BackButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.GoToNextMeasureCommand, v => v.ForwardButton)
                .DisposeWith(d);     
            this.BindCommand(ViewModel, vm => vm.SaveCommand, v => v.SaveButton)
                .DisposeWith(d);
            this.WhenAnyValue(v => v.ViewModel)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    BuildMatrix(vm);
                    vm.WhenAnyValue(x => x.CurrentMeasureIndex)
                        .Subscribe(_ => BuildMatrix(vm))
                        .DisposeWith(d);
                })
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ViewModel.CurrentMeasureIndex)
                .Subscribe(currentIndex => UpdateMeasureBorders(currentIndex))
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowSaveDialog, SaveHandler);
        });
    }

    private Grid _matrixGrid => this.FindControl<Grid>("MatrixGrid");
    private async Task SaveHandler(IInteractionContext<SheetCreationData, SheetNameAndDescription> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new Dialogs.SaveSheetView { ViewModel = new SaveSheetViewModel(context.Input) };
        var result = await saveView.ShowDialog<SheetNameAndDescription>(mainWindow);
        context.SetOutput(result);
    }
    private void UpdateMeasureBorders(int currentIndex)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var itemsCount = MeasuresItemsControl.ItemCount;
            for (var i = 0; i < itemsCount; i++)
            {
                var container = MeasuresItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (container is ContentPresenter presenter)
                {
                    var border = presenter.FindDescendantOfType<Border>();
                    if (border != null)
                        border.BorderBrush = i == currentIndex
                            ? new SolidColorBrush(new Color(0xFF, 0x00, 0x7A, 0xCC))
                            : new SolidColorBrush(Colors.Transparent);

                    // 👇 Scroll the current measure into view
                    if (i == currentIndex) presenter.BringIntoView();
                }
            }
        }, DispatcherPriority.Loaded);
    }
    private void BuildMatrix(ManualViewModel vm)
    {
        _matrixGrid.Children.Clear();
        _matrixGrid.ColumnDefinitions.Clear();
        _matrixGrid.RowDefinitions.Clear();

        _matrixGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100))); 
        for (var c = 0; c < StepCount; c++)
            _matrixGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

        _matrixGrid.RowDefinitions.Add(new RowDefinition(new GridLength(40))); 
        for (var r = 0; r < vm.Drums.Length; r++)
            _matrixGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

        var header = new TextBlock
        {
            Text = "Drum \\ Step",
            Margin = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 16,
            FontFamily = new FontFamily("NunitoFont"),
            FontWeight = FontWeight.SemiBold
        };
        Grid.SetRow(header, 0);
        Grid.SetColumn(header, 0);
        _matrixGrid.Children.Add(header);

        for (var c = 0; c < StepCount; c++)
        {
            var label = new TextBlock
            {
                Text = (c + 1).ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontFamily = new FontFamily("NunitoFont"),
                Foreground = new SolidColorBrush(Colors.Black)
            };

            if (c % 4 == 0)
            {
                label.FontWeight = FontWeight.Bold;
                label.Foreground = new SolidColorBrush(new Color(0xFF, 0x00, 0x7A, 0xCC));
            }

            Grid.SetRow(label, 0);
            Grid.SetColumn(label, c + 1);
            _matrixGrid.Children.Add(label);
        }

        for (var r = 0; r < vm.Drums.Length; r++)
        {
            var drumLabel = new TextBlock
            {
                Text = vm.Drums[r].ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(4, 0, 4, 0),
                FontSize = 16, 
                FontFamily = new FontFamily("NunitoFont")
            };
            Grid.SetRow(drumLabel, r + 1);
            Grid.SetColumn(drumLabel, 0);
            _matrixGrid.Children.Add(drumLabel);

            for (var c = 0; c < StepCount; c++)
            {
                var btn = new ToggleButton
                {
                    Margin = new Thickness(2), // space between buttons
                    Content = null,
                    IsChecked = vm.GetStep(r, c),
                    CornerRadius = new CornerRadius(3),
                    Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)), // default background
                    BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                    BorderThickness = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var checkedStyle = new Style(x => x.OfType<ToggleButton>().Class(":checked"));
                checkedStyle.Setters.Add(new Setter(BackgroundProperty,
                    new SolidColorBrush(Color.FromRgb(200, 80, 80))));
                checkedStyle.Setters.Add(new Setter(BorderBrushProperty,
                    new SolidColorBrush(Color.FromRgb(220, 100, 100))));
                btn.Styles.Add(checkedStyle);


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