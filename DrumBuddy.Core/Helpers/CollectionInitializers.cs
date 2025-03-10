namespace DrumBuddy.Core.Helpers;

public static class CollectionInitializers
{
    public static List<T> CreateList<T>(T value)
    {
        var list = new List<T>();
        list.Add(value);
        return list;
    }
    public static List<T> CreateList<T>(IEnumerable<T> values)
    {
        var list = new List<T>();
        list.AddRange(values);
        return list;
    }
}