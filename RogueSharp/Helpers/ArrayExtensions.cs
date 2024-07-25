namespace RogueSharp.Helpers;

internal static class ArrayExtensions
{
    public static int IndexOf<T>(this T[] array, T value)
    {
        return Array.IndexOf(array, value);
    }
}
