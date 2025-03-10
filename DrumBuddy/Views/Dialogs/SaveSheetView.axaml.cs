using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Models;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;
using Unit = System.Reactive.Unit;

namespace DrumBuddy;

public partial class SaveSheetView : ReactiveWindow<SaveSheetViewModel>
{
    private TextBox NameTextB => this.FindControl<TextBox>("NameTB");
    private Button SaveButton => this.FindControl<Button>("Save");
    private Button CloseButton => this.FindControl<Button>("Cancel");
    public SaveSheetView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.SheetName, v => v.NameTextB.Text).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.SaveSheetCommand, v => v.SaveButton).DisposeWith(d);
            var observerCloseWithName = Observer.Create<Unit>(u => Close(ViewModel.SheetName));
            ViewModel?.SaveSheetCommand.Subscribe(observerCloseWithName); //make optional name
            CloseButton.Click += (sender, e) => Close(null);
            this.KeyDown += (sender, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    if(!string.IsNullOrEmpty(ViewModel.SheetName))
                        ViewModel.SaveSheetCommand.Execute().Subscribe(observerCloseWithName);
                }
                if(e.Key == Avalonia.Input.Key.Escape)
                {
                    Close(null);
                }
            };
        });

    }
}