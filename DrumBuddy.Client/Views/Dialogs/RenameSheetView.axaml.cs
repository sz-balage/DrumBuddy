using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels.Dialogs;
using DrumBuddy.Core.Models;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace DrumBuddy.Client.Views.Dialogs;

public partial class RenameSheetView : ReactiveWindow<RenameSheetViewModel>
{
    public RenameSheetView()
    {
        if(Design.IsDesignMode)
            ViewModel = new RenameSheetViewModel(new Sheet(new Bpm(100),ImmutableArray<Measure>.Empty, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "New Sheet"));
        this.WhenActivated(d =>
        {
            OriginalNameTB.Text = $"Rename {ViewModel!.OriginalName}";
            this.BindValidation(ViewModel, vm => vm.NewName, v => v.NewNameValidation.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.NewName, v => v.NewNameTB.Text).DisposeWith(d);
            
            this.BindCommand(ViewModel, vm => vm.RenameSheetCommand, v => v.Save).DisposeWith(d);
            var observerCloseWithName = Observer.Create<Unit>(u => Close(ViewModel.NewName));
            ViewModel?.RenameSheetCommand.Subscribe(observerCloseWithName); //make optional name
            Cancel.Click += (sender, e) => Close(null);
            KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape) Close(null);
            };
        });
        InitializeComponent();
    }
}