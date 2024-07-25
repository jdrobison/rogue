namespace RogueSharp.Helpers;

internal static class ConsoleKeyInfoExtensions
{
    public static ConsoleKeyInfo ToLower(this ConsoleKeyInfo key)
    {
        if (!char.IsLetter(key.KeyChar))
            return key;

        if (char.IsLower(key.KeyChar))
            return key;

        return key.RemoveModifiers(ConsoleModifiers.Shift);
    }

    public static ConsoleKeyInfo ToUpper(this ConsoleKeyInfo key)
    {
        if (!char.IsLetter(key.KeyChar))
            return key;

        if (char.IsUpper(key.KeyChar))
            return key;

        return key.AddModifiers(ConsoleModifiers.Shift);
    }

    public static ConsoleKeyInfo AddModifiers(this ConsoleKeyInfo key, ConsoleModifiers modifiersToAdd)
    {
        ConsoleModifiers modifiers = key.Modifiers | modifiersToAdd;
        if (modifiers == key.Modifiers)
            return key;

        bool shift   = modifiers.HasFlag(ConsoleModifiers.Shift);
        bool alt     = modifiers.HasFlag(ConsoleModifiers.Alt);
        bool control = modifiers.HasFlag(ConsoleModifiers.Control);

        char keyChar = key.KeyChar;
        if (char.IsLetter(keyChar))
            keyChar = shift ? char.ToUpper(keyChar) : char.ToLower(keyChar);

        return new ConsoleKeyInfo(keyChar, key.Key, shift, alt, control);
    }

    public static ConsoleKeyInfo RemoveModifiers(this ConsoleKeyInfo key, ConsoleModifiers modifiersToRemove)
    {
        ConsoleModifiers modifiers = key.Modifiers & ~modifiersToRemove;
        if (modifiers == key.Modifiers)
            return key;

        bool shift   = modifiers.HasFlag(ConsoleModifiers.Shift);
        bool alt     = modifiers.HasFlag(ConsoleModifiers.Alt);
        bool control = modifiers.HasFlag(ConsoleModifiers.Control);

        char keyChar = key.KeyChar;
        if (char.IsLetter(keyChar))
            keyChar = shift ? char.ToUpper(keyChar) : char.ToLower(keyChar);

        return new ConsoleKeyInfo(keyChar, key.Key, shift, alt, control);
    }
}
