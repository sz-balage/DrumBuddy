using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy.Client.Views.Dialogs;

public partial class CompareView : ReactiveWindow<CompareViewModel>
{
    private ItemsControl _baseSheetItemsControl => this.FindControl<ItemsControl>("BaseSheetItemsControl")!;
    private ItemsControl _comparedSheetItemsControl => this.FindControl<ItemsControl>("ComparedSheetItemsControl")!;
    public CompareView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.BaseSheetMeasures, v => v._baseSheetItemsControl.ItemsSource)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ComparedSheetMeasures, v => v._comparedSheetItemsControl.ItemsSource)
                .DisposeWith(d);
        });
    }
}