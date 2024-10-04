using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using ReactiveUI;

namespace DrumBuddy.Views.HelperViews;

public partial class RythmicGroupView : ReactiveUserControl<RythmicGroupViewModel>
{
    private ContentControl _content => this.FindControl<ContentControl>("Content");
    public RythmicGroupView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(x => x.RythmicGroup)
                .WhereNotNull()
                .Subscribe(rg =>
                {
                    _content.Content = DrawRythmicGroup(rg);
                })
                .DisposeWith(d);
        });
    }

    private TextBlock DrawRythmicGroup(RythmicGroup rg)
    {
        return new TextBlock()
        {
            Text = $"{rg.Notes.Length.ToString()} notes were hit",
            TextWrapping = TextWrapping.Wrap
        };
    }
}