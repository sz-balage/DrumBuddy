using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Unit = System.Reactive.Unit;

namespace DrumBuddy.Client.Views.Dialogs;

public partial class SaveSheetView : ReactiveWindow<SaveSheetViewModel>
{
    public SaveSheetView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.SheetName, v => v.NameTB.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.SheetDescription, v => v.DescriptionTb.Text).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.SaveSheetCommand, v => v.Save).DisposeWith(d);
            this.BindValidation(ViewModel, vm => vm.SheetName, v => v.SheetTitleValidation.Text).DisposeWith(d);
            var observerCloseWithName = Observer.Create<Unit>(u => Close(ViewModel.SheetName));
            ViewModel?.SaveSheetCommand.Subscribe(observerCloseWithName); //make optional name
            Cancel.Click += (sender, e) => Close(null);
            KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                    if (!string.IsNullOrEmpty(ViewModel.SheetName))
                        ViewModel.SaveSheetCommand.Execute().ObserveOn(RxApp.MainThreadScheduler).Subscribe(observerCloseWithName);
                if (e.Key == Key.Escape) Close(null);
            };
        });
    }

    private TextBox NameTextB => this.FindControl<TextBox>("NameTB")!;
    private Button SaveButton => this.FindControl<Button>("Save")!;
    private Button CloseButton => this.FindControl<Button>("Cancel")!;
    private TextBox DescriptionTextB => this.FindControl<TextBox>("DescriptionTB")!;
}