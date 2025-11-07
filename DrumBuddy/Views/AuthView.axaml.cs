using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Views;

public partial class AuthView : ReactiveUserControl<AuthViewModel>
{
    //TODO: make this navigatable instead of switching visibility in MainWindow
    //TODO: fix layout, make prettier with gradient background etc.
    public AuthView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.Password, v => v.PasswordBox.Text)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Password, v => v.ConfirmPasswordBox.Text)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.LoginPromptTextBlock.Text,
                    isLogin => isLogin ? "Sign in to your account" : "Create a new account")
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.SubmitButtonText.Text,
                    isLogin => isLogin ? "Sign in" : "Create account")
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.ToggleButtonText.Text,
                    isLogin => isLogin ? "Register new account" : "Login")
                .DisposeWith(d);
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