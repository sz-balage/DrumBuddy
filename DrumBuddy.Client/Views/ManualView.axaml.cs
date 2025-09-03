using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Client.Views;

public partial class ManualView : ReactiveUserControl<ManualViewModel>
{
    public ManualView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            // Bind sheet list
            this.OneWayBind(ViewModel,
                    vm => vm.Sheets,
                    v => v.SheetsLB.ItemsSource)
                .DisposeWith(d);

            // Bind buttons to commands
            this.BindCommand(ViewModel,
                    vm => vm.GoToCreateCommand,
                    v => v.CreateNewButton)
                .DisposeWith(d);

            this.BindCommand(ViewModel,
                    vm => vm.GoToEditCommand,
                    v => v.EditExistingButton)
                .DisposeWith(d);

            // React to Mode change and toggle panels
            this.WhenAnyValue(v => v.ViewModel!.Mode)
                .Subscribe(mode =>
                {
                    ChoicePanel.IsVisible = mode == ManualMode.Choice;
                    EditPanel.IsVisible   = mode == ManualMode.Edit;
                    CreatePanel.IsVisible = mode == ManualMode.Create;
                })
                .DisposeWith(d);
        });
    }
}