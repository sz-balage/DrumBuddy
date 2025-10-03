using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DrumBuddy.DesignHelpers;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.ViewModels;
using DrumBuddy.ViewModels.Dialogs;
using DrumBuddy.Views.Dialogs;
using DrumBuddy.Views.HelperViews;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views;

public partial class ManualEditorView : ReactiveUserControl<ManualEditorViewModel>
{
    private const int StepCount = ManualEditorViewModel.Columns;
    private readonly SolidColorBrush _borderBrush = new(new Color(0xFF, 0x00, 0x7A, 0xCC));
    private ToggleButton?[,] _stepButtons = new ToggleButton?[0, 0];

    public ManualEditorView()
    {
        if (Design.IsDesignMode)
        {
            var vm = new ManualEditorViewModel(null, new SheetStorage(null,""), null, () => Task.CompletedTask);
            vm.LoadSheet(TestSheetProvider.GetTestSheet());
            ViewModel = vm;
        }

        if (Application.Current?.Resources.TryGetResource("Accent", null, out var accentObj) == true)
            _borderBrush = new SolidColorBrush((Color)accentObj);

        InitializeComponent();
        this.WhenActivated(async d =>
        {
            this.OneWayBind(ViewModel, vm => vm.Measures, v => v.MeasuresItemsControl.ItemsSource)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.MeasureDisplayText, v => v.MeasureDisplayText.Text)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanGoBack, v => v.BackButton.IsEnabled)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanGoForward, v => v.ForwardButton.IsEnabled)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsSaved, v => v.SaveButton.IsEnabled, b => !b)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.BpmDecimal, v => v.BpmNumeric.Value);
            this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameTextBlock.Text, s =>
            {
                if (s is null) return " - Unsaved sheet";

                return $" - {s}";
            }).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.AddMeasureCommand, v => v.AddMeasureButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.NavigateBackCommand, v => v.NavigateBackButton)
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
                    UpdateMeasureBorders(vm.CurrentMeasureIndex);
                    vm.WhenAnyValue(x => x.CurrentMeasureIndex)
                        .Subscribe(_ => BuildMatrix(vm))
                        .DisposeWith(d);
                })
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ViewModel.CurrentMeasureIndex)
                .Subscribe(currentIndex => UpdateMeasureBorders(currentIndex))
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowSaveDialog, SaveHandler);
            this.BindInteraction(ViewModel, vm => vm.ShowConfirmation, ConfirmationHandler);
        });
    }

    private Grid _matrixGrid => this.FindControl<Grid>("MatrixGrid");

    private async Task ConfirmationHandler(IInteractionContext<Unit, Confirmation> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new ConfirmationView();
        var result = await saveView.ShowDialog<Confirmation>(mainWindow);
        context.SetOutput(result);
    }

    private async Task SaveHandler(IInteractionContext<SheetCreationData, SheetNameAndDescription> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new SaveSheetView { ViewModel = new SaveSheetViewModel(context.Input) };
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
                    var measureView = presenter.FindDescendantOfType<MeasureView>();
                    var buttons = presenter.GetVisualDescendants().OfType<Button>().ToList();
                    var deleteButton = buttons.FirstOrDefault(b => b.Name == "DeleteMeasureButton");
                    var duplicateButton = buttons.FirstOrDefault(b => b.Name == "DuplicateMeasureButton");

                    if (measureView != null)
                        measureView.IsBeingEdited = i == currentIndex;

                    if (deleteButton != null)
                        deleteButton.IsVisible = i == currentIndex && itemsCount > 1;
                    if (duplicateButton != null)
                        duplicateButton.IsVisible = i == currentIndex;

                    if (i == currentIndex) presenter.BringIntoView();
                }
            }
        }, DispatcherPriority.Loaded);
    }

    // TODO: move most of this logic to axaml
    private void BuildMatrix(ManualEditorViewModel vm)
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

        _stepButtons = new ToggleButton[vm.Drums.Length, StepCount];

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
                    Margin = new Thickness(2),
                    Content = null,
                    IsChecked = vm.GetStep(r, c),
                    CornerRadius = new CornerRadius(3),
                    Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
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
                btn.Checked += (_, __) =>
                {
                    vm.ToggleStep(rr, cc);
                    UpdateColumnEnabledState(cc, vm);
                };
                btn.Unchecked += (_, __) =>
                {
                    vm.ToggleStep(rr, cc);
                    UpdateColumnEnabledState(cc, vm);
                };

                Grid.SetRow(btn, r + 1);
                Grid.SetColumn(btn, c + 1);
                _matrixGrid.Children.Add(btn);
                _stepButtons[r, c] = btn;
            }
        }

        for (var c = 0; c < StepCount; c++)
            UpdateColumnEnabledState(c, vm);
    }

    private void UpdateColumnEnabledState(int col, ManualEditorViewModel vm)
    {
        if (_stepButtons == null || _stepButtons.Length == 0) return;
        var active = vm.CountCheckedInColumn(col);
        var limitReached = active >= ManualEditorViewModel.MaxNotesPerColumn; // use constant from VM
        for (var r = 0; r < vm.Drums.Length; r++)
        {
            var btn = _stepButtons[r, col];
            if (btn == null) continue;
            if (btn.IsChecked == true)
                btn.IsEnabled = true; // allow unchecking
            else
                btn.IsEnabled = !limitReached;
        }
    }

    private void MeasureBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel == null)
            return;
        if (sender is Border border)
        {
            var index = MeasuresItemsControl.ItemContainerGenerator.IndexFromContainer(
                border.Parent.Parent as ContentPresenter);
            if (index >= 0) ViewModel.SelectMeasure(index);
        }
    }

    private void DeleteMeasureButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
            return;

        // Remove the currently selected measure
        ViewModel.DeleteSelectedMeasure();
    }

    private void DuplicateMeasureButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
            return;

        // Remove the currently selected measure
        ViewModel.DuplicateSelectedMeasure();
    }
}