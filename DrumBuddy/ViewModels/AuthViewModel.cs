using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using DrumBuddy.Api;
using DrumBuddy.Extensions;
using DrumBuddy.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using Refit;
using Splat;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace DrumBuddy.ViewModels;

public partial class AuthViewModel : ReactiveObject, IValidatableViewModel
{
    private readonly ApiClient _apiClient;
    private readonly ConfigurationViewModel _configVm;
    private readonly NotificationService _notificationService;
    private readonly UserService _userService;

    [Reactive] private string _confirmPassword = string.Empty;
    [Reactive] private string _email = string.Empty;
    [Reactive] private bool _isLoading;
    [Reactive] private bool _isLoginMode = true;
    [Reactive] private bool _isResetPasswordMode;
    [Reactive] private string _password = string.Empty;
    [Reactive] private bool _rememberMe;
    [Reactive] private string _userName = string.Empty;

    public AuthViewModel()
    {
        _apiClient = Locator.Current.GetRequiredService<ApiClient>();
        _userService = Locator.Current.GetRequiredService<UserService>();
        _configVm = Locator.Current.GetRequiredService<ConfigurationViewModel>();
        _notificationService = Locator.Current.GetRequiredService<NotificationService>("MainWindowNotificationService");
        _ = LoadRememberedCredentialsAsync();

        this.ValidationRule(
            vm => vm.Email,
            IsValidEmail,
            "Please enter a valid email address");

        this.ValidationRule(
            vm => vm.Password,
            this.WhenAnyValue(vm => vm.IsLoginMode, vm => vm.Password),
            x => !x.Item1 || !string.IsNullOrWhiteSpace(x.Item2),
            _ => "Password cannot be empty");

        this.ValidationRule(
            vm => vm.Password,
            this.WhenAnyValue(vm => vm.IsLoginMode, vm => vm.Password),
            x => x.Item1 || IsStrongPassword(x.Item2),
            _ => "Password must be at least 6 characters and contain at least one uppercase letter");

        this.ValidationRule(
            vm => vm.ConfirmPassword,
            this.WhenAnyValue(vm => vm.IsLoginMode, vm => vm.ConfirmPassword, vm => vm.Password),
            x => x.Item1 || x.Item2 == x.Item3,
            _ => "Passwords do not match");

        var isValid = this.IsValid();

        LoginCommand = ReactiveCommand.CreateFromTask(ExecuteLogin, isValid);
        RegisterCommand = ReactiveCommand.CreateFromTask(ExecuteRegister, isValid);

        LoginCommand.Where(s => s).Subscribe(success => HandleAuthSuccess(success, "Login successful"));
        RegisterCommand.Where(s => s).Subscribe(success => HandleAuthSuccess(success, "Registration successful"));
    }

    private IObservable<bool> _signInCanExecute => this.IsValid();

    public ReactiveCommand<Unit, bool> LoginCommand { get; }
    public ReactiveCommand<Unit, bool> RegisterCommand { get; }
    public IValidationContext ValidationContext { get; } = new ValidationContext();

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return password.Length >= 6 && password.Any(char.IsUpper);
    }

    private async Task LoadRememberedCredentialsAsync()
    {
        var credentials = await _userService.LoadRememberedCredentialsAsync();
        if (credentials.HasValue)
        {
            Email = credentials.Value.Email ?? string.Empty;
            Password = credentials.Value.Password ?? string.Empty;
            RememberMe = true;
        }
    }

    private void HandleAuthSuccess(bool success, string title)
    {
        _notificationService.ShowNotification(new Notification(
            title,
            $"Welcome, {_email}!",
            NotificationType.Success));
        if (_rememberMe)
            _ = _userService.SaveRememberedCredentialsAsync(_email, _password);
        else
            _userService.ClearRememberedCredentials();

        NavigateToHome();
    }

    [ReactiveCommand]
    private async Task GuestLogin()
    {
        await _configVm.LoadConfig();
        _notificationService.ShowNotification(new Notification(
            "Guest Login",
            "You are now logged in as a guest.",
            NotificationType.Success));
        NavigateToHome();
    }

    private async Task<bool> ExecuteLogin()
    {
        try
        {
            IsLoading = true;
            await _apiClient.LoginAsync(_email, _password);
            await _configVm.LoadConfig();
            return true;
        }
        catch (ApiException apiException)
        {
            var errorMessage = GetApiErrorMessage(apiException);
            _notificationService.ShowNotification(new Notification(
                "Login Error",
                errorMessage,
                NotificationType.Error));
            return false;
        }
        catch (Exception ex)
        {
            _notificationService.ShowNotification(new Notification(
                "Login Error",
                ex.Message,
                NotificationType.Error));
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<bool> ExecuteRegister()
    {
        try
        {
            IsLoading = true;
            await _apiClient.RegisterAsync(_email, _password, _userName);
            _userService.ClearRememberedCredentials();
            await _configVm.LoadConfig();
            IsLoginMode = true;
            return true;
        }
        catch (ApiException apiException)
        {
            var errorMessage = GetApiErrorMessage(apiException);
            _notificationService.ShowNotification(new Notification(
                "Registration Error",
                errorMessage,
                NotificationType.Error));
            return false;
        }
        catch (Exception ex)
        {
            _notificationService.ShowNotification(new Notification(
                "Registration Error",
                ex.Message,
                NotificationType.Error));
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ReactiveCommand]
    private async Task ResetPassword()
    {
        try
        {
            IsLoading = true;
            //TODO: Change email to user input
            var response = await _apiClient.ForgotPasswordAsync(Email);
            _notificationService.ShowNotification(new Notification(
                "Password Reset",
                response.Message,
                NotificationType.Success));
            CancelResetPassword();
        }
        catch (ApiException apiException)
        {
            var errorMessage = GetApiErrorMessage(apiException);
            _notificationService.ShowNotification(new Notification(
                "Password Reset Error",
                errorMessage,
                NotificationType.Error));
        }
        catch (Exception ex)
        {
            _notificationService.ShowNotification(new Notification(
                "Password Reset Error",
                ex.Message,
                NotificationType.Error));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetApiErrorMessage(ApiException apiException)
    {
        try
        {
            using var doc = JsonDocument.Parse(apiException.Content);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errorsElement))
            {
                var errorMessages = new List<string>();

                if (errorsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var error in errorsElement.EnumerateArray())
                    {
                        if (error.ValueKind == JsonValueKind.String)
                        {
                            errorMessages.Add(error.GetString() ?? "Unknown error");
                        }
                    }
                }

                if (errorMessages.Count > 0)
                {
                    return string.Join("\n", errorMessages);
                }
            }

            if (root.TryGetProperty("message", out var messageElement) &&
                messageElement.ValueKind == JsonValueKind.String)
            {
                return messageElement.GetString() ?? "An error occurred";
            }
        }
        catch
        {
            // Fallback to raw content
            if (apiException.StatusCode == HttpStatusCode.Unauthorized)
                return "Invalid email or password.";
        }

        return apiException.Content ?? "An error occurred. Please try again.";
    }

    [ReactiveCommand]
    private void CancelResetPassword()
    {
        IsResetPasswordMode = false;
        Email = string.Empty;
    }

    [ReactiveCommand]
    private void ShowResetPassword()
    {
        IsResetPasswordMode = true;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        UserName = string.Empty;
        RememberMe = false;
    }

    private void NavigateToHome()
    {
        var mainVm = Locator.Current.GetService<MainViewModel>();
        mainVm?.SetAuthenticated();
    }

    [ReactiveCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        UserName = string.Empty;
        RememberMe = false;
    }
}