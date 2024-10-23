using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy;

public partial class SaveSheetView : ReactiveWindow<SaveSheetViewModel>
{
    private TextBox NameTextB => this.FindControl<TextBox>("NameTB");
    private Button Save => this.FindControl<Button>("SaveButton");
    private Button Close => this.FindControl<Button>("CloseButton");
    public SaveSheetView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.SheetName, v => v.NameTextB.Text).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.SaveSheetCommand, v => v.Save).DisposeWith(d);
            Close.Click += (s, e) => Close();
        });
    }
}