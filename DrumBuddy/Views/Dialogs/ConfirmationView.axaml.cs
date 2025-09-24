using Avalonia.Input;
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
            Confirm.Click += (sender, e) => Close(Confirmation.Close);
            Cancel.Click += (sender, e) => Close(Confirmation.Cancel);
            KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape) Close(Confirmation.Cancel);
                if (e.Key == Key.Enter) Close(Confirmation.Close);
            };
        });
    }
}