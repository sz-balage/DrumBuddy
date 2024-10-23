using DrumBuddy.IO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrumBuddy.IO.Abstractions
{
    public interface IMidiService
    {
        public IObservable<Beat> GetBeatsObservable(BPM tempo);
    }
}
