namespace DrumBuddy.Core.Models;

public readonly record struct Bpm
{
    public Bpm(int value)
    {
        Value = value switch
        {
            _ when value is 0 or < 0 => throw new ArgumentException("BPM cannot be negative, or equal to zero!"),
            > 250 => throw new ArgumentException("BPM cannot be greater than 250!"),
            _ => value
        };
    }

    public Bpm() : this(-1)
    {
    }

    public int Value { get; }

    public static implicit operator int(Bpm value)
    {
        return value.Value;
    }

    public static implicit operator Bpm(int value)
    {
        return new Bpm(value);
    }
}