using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using PropertyBindingMixins = ReactiveUI.PropertyBindingMixins;

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
            PropertyBindingMixins.OneWayBind(this, ViewModel, vm => vm.IsLoginMode, v => v.LoginPromptTextBlock.Text,
                    isLogin => isLogin ? "Sign in to your account" : "Create a new account")
                .DisposeWith(d);
            PropertyBindingMixins.OneWayBind(this, ViewModel, vm => vm.IsLoginMode, v => v.SubmitButtonText.Text,
                    isLogin => isLogin ? "Sign in" : "Create account")
                .DisposeWith(d);
            PropertyBindingMixins.OneWayBind(this, ViewModel, vm => vm.IsLoginMode, v => v.ToggleButtonText.Text,
                    isLogin => isLogin ? "Register new account" : "Login")
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