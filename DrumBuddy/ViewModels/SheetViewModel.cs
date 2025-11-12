using System;
using System.Collections.Immutable;
using System.Reactive;
using ReactiveUI;
using DrumBuddy.Core.Models;

namespace DrumBuddy.ViewModels
{
    public class SheetViewModel : ReactiveObject
    {
        private bool _isSyncEnabled;
        private DateTime? _lastSyncedAt;
        private bool _isSyncing;

        public SheetViewModel(Sheet sheet)
        {
            Sheet = sheet;
            _isSyncEnabled = sheet.IsSyncEnabled;
            _lastSyncedAt = sheet.LastSyncedAt;
        }

        public Sheet Sheet { get; }

        public bool IsSyncEnabled
        {
            get => _isSyncEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSyncEnabled, value);
                Sheet.IsSyncEnabled = value;
                
            }
        }
        public DateTime? LastSyncedAt
        {
            get => _lastSyncedAt;
            set { this.RaiseAndSetIfChanged(ref _lastSyncedAt, value); Sheet.LastSyncedAt = value; }
        }
        public Guid Id => Sheet.Id;
        public string Name => Sheet.Name;
        public string Description => Sheet.Description;
        public Bpm Tempo => Sheet.Tempo;
        public TimeSpan Length => Sheet.Length;
        public bool IsSyncing
        {
            get => _isSyncing;
            set => this.RaiseAndSetIfChanged(ref _isSyncing, value);
        }
    }
}