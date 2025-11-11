using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DrumBuddy.ViewModels;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace DrumBuddy.Views;

public partial class AuthView : ReactiveUserControl<AuthViewModel>
{
    private Canvas? _canvas;
    private List<FloatingIcon> _floatingIcons = new();
    private Random _random = new();
    private IDisposable? _renderLoopSubscription;

    public AuthView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            _canvas = this.FindControl<Canvas>("FloatingIconsCanvas");

            if (_canvas != null)
            {
                _canvas.Loaded += (s, e) => InitializeFloatingIcons();
                StartAnimationLoop();
            }

            this.Bind(ViewModel, vm => vm.Password, v => v.PasswordBox.Text)
                .DisposeWith(d);

            this.Bind(ViewModel, vm => vm.ConfirmPassword, v => v.ConfirmPasswordBox.Text)
                .DisposeWith(d);

            this.BindValidation(ViewModel, vm => vm.Email, v => v.EmailValidation.Text)
                .DisposeWith(d);  
            this.BindValidation(ViewModel, vm => vm.Email, v => v.ResetEmailValidation.Text)
                .DisposeWith(d);
            
            this.BindValidation(ViewModel, vm => vm.Password, v => v.PasswordValidation.Text)
                .DisposeWith(d);
            
            this.BindValidation(ViewModel, vm => vm.ConfirmPassword, v => v.ConfirmPasswordValidation.Text)
                .DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.LoginPromptTextBlock.Text,
                    isLogin => isLogin ? "Sign in to your account" : "Create a new account")
                .DisposeWith(d);
            
            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.LoginButton.IsVisible)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.LoginCommand, v => v.LoginButton)
                .DisposeWith(d);
            
            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.RegisterButton.IsVisible, b => !b)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.RegisterCommand, v => v.RegisterButton)
                .DisposeWith(d);
            
            this.OneWayBind(ViewModel, vm => vm.IsLoginMode, v => v.ToggleButtonText.Text,
                    isLogin => isLogin ? "Don't have an account? Register" : "Already have an account? Sign In")
                .DisposeWith(d);

            d.Add(Disposable.Create(StopAnimationLoop));
        });
    }

    private void InitializeFloatingIcons()
    {
        if (_canvas == null) return;

        for (int i = 0; i < 20; i++)
        {
            var icon = new FloatingIcon
            {
                X = _random.Next(0, (int)_canvas.Bounds.Width),
                Y = _random.Next(0, (int)_canvas.Bounds.Height),
                Size = _random.Next(40, 100),
                Opacity = _random.Next(10, 40) / 100.0,
                SpeedX = (_random.NextDouble() - 0.5) * 2,
                SpeedY = (_random.NextDouble() - 0.5) * 2,
                RotationSpeed = (_random.NextDouble() - 0.5) * 3,
                Rotation = _random.Next(0, 360)
            };

            _floatingIcons.Add(icon);
            AddIconToCanvas(icon);
        }
    }

    private void AddIconToCanvas(FloatingIcon icon)
    {
        if (_canvas == null) return;

        var image = new Image
        {
            Width = icon.Size,
            Height = icon.Size,
            Opacity = icon.Opacity,
            Source = new Bitmap(AssetLoader.Open(new Uri("avares://DrumBuddy/Assets/app.ico")))
        };

        image.RenderTransform = new RotateTransform(icon.Rotation);
        Canvas.SetLeft(image, icon.X);
        Canvas.SetTop(image, icon.Y);

        _canvas.Children.Add(image);
        icon.Visual = image;
    }

    private void StartAnimationLoop()
    {
        if (_canvas == null) return;

        var timer = new System.Threading.Timer(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(() => UpdateAnimation());
        }, null, 0, 16);

        _renderLoopSubscription = Disposable.Create(() => timer.Dispose());
    }

    private void StopAnimationLoop()
    {
        _renderLoopSubscription?.Dispose();
    }

    private void UpdateAnimation()
    {
        if (_canvas == null) return;

        var width = _canvas.Bounds.Width;
        var height = _canvas.Bounds.Height;

        foreach (var icon in _floatingIcons)
        {
            icon.X += icon.SpeedX;
            icon.Y += icon.SpeedY;

            if (icon.X < -icon.Size) icon.X = width;
            if (icon.X > width) icon.X = -icon.Size;
            if (icon.Y < -icon.Size) icon.Y = height;
            if (icon.Y > height) icon.Y = -icon.Size;

            icon.Rotation += icon.RotationSpeed;
            if (icon.Rotation >= 360) icon.Rotation -= 360;

            if (icon.Visual != null)
            {
                Canvas.SetLeft(icon.Visual, icon.X);
                Canvas.SetTop(icon.Visual, icon.Y);
                icon.Visual.RenderTransform = new RotateTransform(icon.Rotation);
            }
        }
    }

    private class FloatingIcon
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; set; }
        public double Opacity { get; set; }
        public double SpeedX { get; set; }
        public double SpeedY { get; set; }
        public double Rotation { get; set; }
        public double RotationSpeed { get; set; }
        public Image? Visual { get; set; }
    }
}
