namespace RogueSharp.Enumerations;

/// <summary>
/// Kinds of rods, wands, and staffs
/// </summary>
internal enum StickKind
{
    WS_LIGHT     = 0,
    WS_INVIS     = 1,
    WS_ELECT     = 2,
    WS_FIRE      = 3,
    WS_COLD      = 4,
    WS_POLYMORPH = 5,
    WS_MISSILE   = 6,
    WS_HASTE_M   = 7,
    WS_SLOW_M    = 8,
    WS_DRAIN     = 9,
    WS_NOP       = 10,
    WS_TELAWAY   = 11,
    WS_TELTO     = 12,
    WS_CANCEL    = 13,

    /// <summary>The number of different kinds of sticks</summary>
    Count = 14,
}
