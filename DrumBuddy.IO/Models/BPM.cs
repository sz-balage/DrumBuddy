using LanguageExt;

namespace DrumBuddy.IO.Models
{
    public readonly record struct BPM
    {
        public static Either<ArgumentException, BPM> From(int value) => value switch
        {
            < 0 => new ArgumentException("BPM cannot be negative!"),
            > 250 => new ArgumentException("BPM cannot be greater than 250!"), //for now
            _ => new BPM(value)
        };
        public BPM() => throw new NotSupportedException("This type is not intended to use without value!");
        private BPM(int value) => Value = value;
        public int Value { get; }
        public static implicit operator int(BPM value) => value.Value;
    }
}
