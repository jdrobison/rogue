namespace RogueSharp.Enumerations;

[Flags]
internal enum RoomFlags
{
    /// <summary>Room is dark (was ISDARK)</summary>
    IsDark = 0x01,

    /// <summary>Room is gone (a corridor) (was ISGONE)</summary>
    IsGone = 0x02,

    /// <summary>Room is maze (was ISMAZE)</summary>
    IsMaze = 0x04,
}
