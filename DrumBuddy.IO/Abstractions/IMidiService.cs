using DrumBuddy.IO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrumBuddy.IO.Enums;
using LanguageExt;

namespace DrumBuddy.IO.Abstractions
{
    public interface IMidiService
    {
        public IObservable<Beat> GetBeatsObservable();
        public bool IsConnected { get; }
        public IObservable<bool> InputDeviceDisconnected { get; }
        public Fin<Unit> TryConnect();

    }
}
