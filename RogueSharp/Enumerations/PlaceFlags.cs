namespace RogueSharp.Enumerations;

/// <summary>
/// Flags describing a position on the map
/// </summary>
[Flags]
internal enum PlaceFlags
{
    /// <summary>Is a passageway (was F_PASS)</summary>
    Passage = 0x80,

    /// <summary>Have seen this spot before (was F_SEEN)</summary>
    Seen = 0x40,

    /// <summary>Object was dropped here (was F_DROPPED)</summary>
    Dropped = 0x20,

    /// <summary>Door is locked (was F_LOCKED)</summary>
    Locked = 0x20,

    /// <summary>What you see is what you get (was F_REAL)</summary>
    Real = 0x10,

    /// <summary>Passage number mask (was F_PNUM)</summary>
    PassageMask = 0x0f,

    /// <summary>Trap number mask (was F_TMASK)</summary>
    TrapMask = 0x07,
}
