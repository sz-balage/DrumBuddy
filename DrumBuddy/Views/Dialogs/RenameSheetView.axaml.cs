﻿using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Models;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace DrumBuddy.Views.Dialogs;

public partial class RenameSheetView : ReactiveWindow<RenameSheetViewModel>
{
    public RenameSheetView()
    {
        if (Design.IsDesignMode)
            ViewModel = new RenameSheetViewModel(new Sheet(new Bpm(100), ImmutableArray<Measure>.Empty,
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                "New Sheet"));
        this.WhenActivated(d =>
        {
            OriginalNameTB.Text = $"Rename {ViewModel!.OriginalSheet.Name}";
            this.BindValidation(ViewModel, vm => vm.NewName, v => v.NewNameValidation.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.NewName, v => v.NewNameTB.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.NewDescription, v => v.DescriptionTb.Text).DisposeWith(d);

            this.BindCommand(ViewModel, vm => vm.RenameSheetCommand, v => v.Save).DisposeWith(d);
            var originalSheet = ViewModel.OriginalSheet;
            var observerCloseWithName = Observer.Create<Unit>(u => Close(new Sheet(originalSheet.Tempo,
                originalSheet.Measures, ViewModel.NewName, ViewModel.NewDescription)));
            ViewModel?.RenameSheetCommand.Subscribe(observerCloseWithName); //make optional name
            Cancel.Click += (sender, e) => Close(ViewModel!.OriginalSheet);
            KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape) Close(ViewModel!.OriginalSheet);
            };
            this.Closing += (sender, args) =>
            {
                if (!args.IsProgrammatic)
                {
                    this.Close(ViewModel!.OriginalSheet);
                }
            };
        });
        InitializeComponent();
    }
}