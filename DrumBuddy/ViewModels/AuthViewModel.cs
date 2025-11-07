using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using DrumBuddy.Api;
using DrumBuddy.Extensions;
using DrumBuddy.Services;
using DrumBuddy.Views;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace DrumBuddy.ViewModels;

public partial class AuthViewModel : ReactiveObject
{
    private readonly ApiClient _apiClient;
    private readonly NotificationService _notificationService;
    private readonly TokenService _tokenService;
    [Reactive] private string _confirmPassword = string.Empty;

    [Reactive] private string _email = string.Empty;
    [Reactive] private bool _isLoading;
    [Reactive] private bool _isLoginMode = true;
    [Reactive] private string _password = string.Empty;
    [Reactive] private bool _rememberMe;
    [Reactive] private string _userName = string.Empty;

    public AuthViewModel(MainWindow mainWindow)
    {
        _apiClient = Locator.Current.GetRequiredService<ApiClient>();
        _tokenService = Locator.Current.GetRequiredService<TokenService>();

        _notificationService = new NotificationService(mainWindow);

        _ = LoadRememberedCredentialsAsync();

        var canLogin = this.WhenAnyValue(
            vm => vm.Email,
            vm => vm.Password,
            vm => vm.IsLoading,
            (email, password, isLoading) =>
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(password) &&
                !isLoading);

        LoginCommand = ReactiveCommand.CreateFromTask(ExecuteLogin, canLogin);

        var canRegister = this.WhenAnyValue(
            vm => vm.Email,
            vm => vm.Password,
            vm => vm.ConfirmPassword,
            vm => vm.IsLoading,
            (email, password, confirm, isLoading) =>
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(password) &&
                password == confirm &&
                !isLoading);

        RegisterCommand = ReactiveCommand.CreateFromTask(ExecuteRegister, canRegister);

        LoginCommand.Where(s => s).Subscribe(success => HandleAuthSuccess(success, "Login successful"));
        RegisterCommand.Where(s => s).Subscribe(success => HandleAuthSuccess(success, "Registration successful"));
    }

    public ReactiveCommand<Unit, bool> LoginCommand { get; }
    public ReactiveCommand<Unit, bool> RegisterCommand { get; }

    private async Task LoadRememberedCredentialsAsync()
    {
        var credentials = await _tokenService.LoadRememberedCredentialsAsync();
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
            _ = _tokenService.SaveRememberedCredentialsAsync(_email, _password);
        else
            _ = _tokenService.ClearRememberedCredentialsAsync();

        NavigateToHome();
    }

    private async Task<bool> ExecuteLogin()
    {
        try
        {
            IsLoading = true;
            await _apiClient.LoginAsync(_email, _password);
            return true;
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
        //clear remembered credentials on new registration
        await _tokenService.ClearRememberedCredentialsAsync();
        return true;
    }
    catch (Refit.ApiException apiException)
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
    private string GetApiErrorMessage(Refit.ApiException apiException)
    {
        try
        {
            // Deserialize to JsonDocument to handle JsonElement
            using var doc = System.Text.Json.JsonDocument.Parse(apiException.Content);
            var root = doc.RootElement;

            // Check for errors array
            if (root.TryGetProperty("errors", out var errorsElement))
            {
                var errorMessages = new List<string>();

                if (errorsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var error in errorsElement.EnumerateArray())
                    {
                        if (error.ValueKind == System.Text.Json.JsonValueKind.String)
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

            // Check for message field
            if (root.TryGetProperty("message", out var messageElement) && 
                messageElement.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return messageElement.GetString() ?? "An error occurred";
            }
        }
        catch
        {
            // Fallback to raw content
        }

        return apiException.Content ?? "An error occurred. Please try again.";
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