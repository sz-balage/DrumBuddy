using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DrumBuddy.Models;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using Unit = System.Reactive.Unit;

namespace DrumBuddy.Views.Dialogs;

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
            var observerCloseWithName = Observer.Create<Unit>(u =>
                Close(new SheetNameAndDescription(ViewModel.SheetName, ViewModel.SheetDescription)));
            ViewModel?.SaveSheetCommand.Subscribe(observerCloseWithName); //make optional name
            Cancel.Click += (sender, e) => Close(new SheetNameAndDescription(null, null));
            KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape) Close(new SheetNameAndDescription(null, null));
            };
        });
    }
}