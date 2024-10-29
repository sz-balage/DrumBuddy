using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DrumBuddy.ViewModels;
using DrumBuddy.Views;
using Splat;

namespace DrumBuddy
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = Locator.Current.GetService<MainWindow>();
                if (desktop.MainWindow.DataContext is MainViewModel vm)
                {
                    vm.SelectedPaneItem = vm.PaneItems[0];
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}