using System.Collections.Immutable;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.Models;
using DrumBuddy.Services.Layout;
using Splat;

namespace DrumBuddy.Services;

public class NoteDrawHelper
{
    private readonly NoteLayoutEngine _engine;
    private readonly NoteDrawRenderer _renderer;

    public NoteDrawHelper(double width, double height)
    {
        var config = Locator.Current.GetRequiredService<ConfigurationService>();

        _engine = new NoteLayoutEngine(
            width,
            height,
            config.DrumPositions
        );

        _renderer = new NoteDrawRenderer();
    }

    public (ImmutableArray<NoteImageAndBounds>, ImmutableArray<LineAndStroke>)
        GetLinesAndImagesToDraw(RythmicGroup group)
    {
        var layout = _engine.Generate(group);
        return _renderer.Render(layout);
    }
}