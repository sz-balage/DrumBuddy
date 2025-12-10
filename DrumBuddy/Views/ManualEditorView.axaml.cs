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
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DrumBuddy.DesignHelpers;
using DrumBuddy.Models;
using DrumBuddy.Services;
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
            var vm = new ManualEditorViewModel(null, new SheetService(null, null, null), () => Task.CompletedTask);
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
                .Subscribe(vm => { UpdateMeasureBorders(vm.CurrentMeasureIndex); })
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ViewModel.CurrentMeasureIndex)
                .Subscribe(currentIndex => UpdateMeasureBorders(currentIndex))
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowSaveDialog, SaveHandler);
            this.BindInteraction(ViewModel, vm => vm.ShowConfirmation, ConfirmationHandler);
        });
    }

    private async Task ConfirmationHandler(IInteractionContext<Unit, Confirmation> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new ConfirmationView
        {
            ViewModel = new ConfirmationViewModel
            {
                Message = "You have unsaved changes. Are you sure you want to exit?",
                ShowDiscard = true,
                ShowConfirm = true,
                ConfirmText = "Save",
                DiscardText = "Discard",
                CancelText = "Cancel"
            }
        };
        var result = await saveView.ShowDialog<Confirmation>(mainWindow);
        context.SetOutput(result);
    }

    private async Task SaveHandler(IInteractionContext<SheetCreationData, SheetNameAndDescription> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new SaveSheetView
            { ViewModel = new SaveSheetViewModel(context.Input, ViewModel.CurrentSheet.Id) };
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
                    var moveLeftButton = buttons.FirstOrDefault(b => b.Name == "MoveLeftMeasureButton");
                    var moveRightButton = buttons.FirstOrDefault(b => b.Name == "MoveRightMeasureButton");

                    if (measureView != null)
                        measureView.IsBeingEdited = i == currentIndex;

                    if (deleteButton != null)
                        deleteButton.IsVisible = i == currentIndex && itemsCount > 1;
                    if (duplicateButton != null)
                        duplicateButton.IsVisible = i == currentIndex;

                    if (moveLeftButton != null)
                        moveLeftButton.IsVisible = i == currentIndex && i > 0 && itemsCount > 1;
                    if (moveRightButton != null)
                        moveRightButton.IsVisible = i == currentIndex && i < itemsCount - 1 && itemsCount > 1;

                    if (i == currentIndex) presenter.BringIntoView();
                }
            }
        }, DispatcherPriority.Loaded);
    }

    private void MoveLeftMeasureButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null) return;
        ViewModel.MoveSelectedMeasureLeft();
    }

    private void MoveRightMeasureButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null) return;
        ViewModel.MoveSelectedMeasureRight();
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

        ViewModel.DeleteSelectedMeasure();
    }

    private void DuplicateMeasureButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
            return;

        ViewModel.DuplicateSelectedMeasure();
    }
}