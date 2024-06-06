namespace RogueSharp.Enumerations;

internal enum TrapKind
{
    /// <summary>Door trap (was T_DOOR)</summary>
    Door = 0,

    /// <summary>Arrow trap (was T_ARROW)</summary>
    Arrow = 1,

    /// <summary>Sleep trap (was T_SLEEP)</summary>
    Sleep = 2,

    /// <summary>Bear trap (was T_BEAR)</summary>
    Bear = 3,

    /// <summary>Teleportation trap (was T_TELEP)</summary>
    Teleport = 4,

    /// <summary>Dart trap (was T_DART) </summary>
    Dart = 5,

    /// <summary>Rust trap (was T_RUST)</summary>
    Rust = 6,

    /// <summary>Mystery trap (was T_MYST)</summary>
    Mystery = 7,

    /// <summary>The number of different kinds of traps</summary>
    Count = 8,
}
