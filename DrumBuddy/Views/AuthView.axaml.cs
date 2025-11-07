using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;

namespace DrumBuddy.Views;

public class AuthView : ReactiveUserControl<AuthViewModel>
{
    public AuthView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            // Bind password TextBox (using PasswordChar instead of PasswordBox)
            this.Bind(ViewModel, vm => vm.Password, v => v.PasswordBox.Text)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Password, v => v.ConfirmPasswordBox.Text)
                .DisposeWith(d);

            // Bind submit button to appropriate command
            var submitButton = this.FindControl<Button>("SubmitButton");
            if (submitButton != null)
                submitButton.Click += (s, e) =>
                {
                    if (ViewModel!.IsLoginMode)
                        ViewModel.LoginCommand.Execute().Subscribe();
                    else
                        ViewModel.RegisterCommand.Execute().Subscribe();
                };
        });
    }
}