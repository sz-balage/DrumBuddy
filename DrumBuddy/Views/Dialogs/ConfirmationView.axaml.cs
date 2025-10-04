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
            ViewModel = new ConfirmationViewModel();
            Discard.Click += (sender, e) => Close(Confirmation.Discard);
            Cancel.Click += (sender, e) => Close(Confirmation.Cancel);
            Save.Click += (sender, e) => Close(Confirmation.Save);
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