using System;
using Avalonia.ReactiveUI;
using DrumBuddy.Models;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy.Views.Dialogs;

public partial class ConfirmationView : ReactiveWindow<ConfirmationViewModel>
{
    public ConfirmationView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(vm => vm.ShowConfirm, vm => vm.ShowDiscard).Subscribe(vals =>
            {
                if (!vals.Item1 || !vals.Item2)
                    ButtonGrid.Columns = 2;
                else
                    ButtonGrid.Columns = 3;
            });
            Discard.Click += (sender, e) => Close(Confirmation.Discard);
            Cancel.Click += (sender, e) => Close(Confirmation.Cancel);
            Save.Click += (sender, e) => Close(Confirmation.Confirm);
            this.Closing += (sender, args) =>
            {
                if (!args.IsProgrammatic)
                {
                    Close(Confirmation.Cancel);
                }
            };
        });
    }
}