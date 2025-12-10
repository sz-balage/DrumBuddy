using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace DrumBuddy.Views;

public partial class AuthView : ReactiveUserControl<AuthViewModel>
{
    private bool _animationRunning = true;
    private Canvas? _canvas;
    private List<FloatingIcon> _floatingIcons = new();
    private Random _random = new();
    private IDisposable? _renderLoopSubscription;

    public AuthView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            var toggleBtn = this.FindControl<Button>("ToggleAnimationButton");
            toggleBtn.Click += (_, _) => ToggleAnimation();

            var pw = this.FindControl<TextBox>("PasswordBox");
            var cpw = this.FindControl<TextBox>("ConfirmPasswordBox");

            if (pw != null)
            {
                pw.AddHandler(KeyDownEvent, (sender, e) =>
                {
                    if (e is KeyEventArgs ke && ke.Key == Key.Space)
                        ke.Handled = true;
                }, handledEventsToo: true);
                //trim spaces 
                pw.GetObservable(TextBox.TextProperty)
                    .Subscribe(_ => FilterOutSpaces(pw)).DisposeWith(d);
            }

            if (cpw != null)
            {
                cpw.AddHandler(KeyDownEvent, (sender, e) =>
                {
                    if (e is KeyEventArgs ke && ke.Key == Key.Space)
                        ke.Handled = true;
                }, handledEventsToo: true);

                cpw.GetObservable(TextBox.TextProperty)
                    .Subscribe(_ => FilterOutSpaces(cpw)).DisposeWith(d);
            }

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
            d.Add(Disposable.Create(() =>
            {
                StopAnimationLoop();
                _floatingIcons.Clear();
                _canvas?.Children.Clear();
            }));
        });
    }

    private void ToggleAnimation()
    {
        if (_animationRunning)
        {
            StopAnimationLoop();
            ToggleAnimationButton.Content = "Start Animation";
        }
        else
        {
            StartAnimationLoop();
            ToggleAnimationButton.Content = "Stop Animation";
        }

        _animationRunning = !_animationRunning;
    }

    private void FilterOutSpaces(TextBox tb)
    {
        if (tb == null) return;
        var text = tb.Text ?? string.Empty;
        if (!text.Contains(' ')) return;

        var originalCaret = tb.CaretIndex;
        var spacesBeforeCaret = text.Take(Math.Min(originalCaret, text.Length)).Count(c => c == ' ');
        var newText = text.Replace(" ", string.Empty);
        var newCaret = Math.Max(0, originalCaret - spacesBeforeCaret);

        tb.Text = newText;
        tb.CaretIndex = Math.Min(newText.Length, newCaret);
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
        if (_renderLoopSubscription != null)
            return;

        _renderLoopSubscription =
            Observable.Interval(TimeSpan.FromMilliseconds(33), RxApp.MainThreadScheduler)
                .Subscribe(_ => { UpdateAnimation(); });
    }


    private void StopAnimationLoop()
    {
        _renderLoopSubscription?.Dispose();
        _renderLoopSubscription = null;
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