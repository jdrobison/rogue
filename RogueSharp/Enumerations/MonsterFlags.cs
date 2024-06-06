namespace RogueSharp.Enumerations;

[Flags]
internal enum MonsterFlags
{
    None = 0,

    /// <summary>Creature can confuse (was CANHUH)</summary>
    CanHuh = 0x0000001,

    /// <summary>Creature can see invisible creatures (was CANSEE)</summary>
    CanSee = 0x0000002,

    /// <summary>Creature is blind (was ISBLIND)</summary>
    IsBlind = 0x0000004,

    /// <summary>Creature has special qualities cancelled (was ISCANC)</summary>
    IsCancelled = 0x0000010,

    /// <summary>Creature has been seen (used for objects) (was ISFOUND)</summary>
    IsFound = 0x0000020,

    /// <summary>Creature runs to protect gold (was ISGREED)</summary>
    IsGreedy = 0x0000040,

    /// <summary>Creature has been hastened (was ISHASTE)</summary>
    IsHastened = 0x0000100,

    /// <summary>Creature is the target of an 'f' command (was ISTARGET)</summary>
    IsTarget = 0x0000200,

    /// <summary>Creature has been held (was ISHELD)</summary>
    IsHeld = 0x0000400,

    /// <summary>Creature is confused (was ISHUH)</summary>
    IsHuh = 0x0001000,

    /// <summary>Creature is invisible (was ISINVIS)</summary>
    IsInvisible = 0x0002000,

    /// <summary>Creature can wake when player enters room (was ISMEAN)</summary>
    IsMean = 0x0004000,

    /// <summary>Creature can regenerate (was ISREGEN)</summary>
    IsRegen = 0x0010000,

    /// <summary>Creature is running at the player (was ISRUN)</summary>
    IsRunning = 0x0020000,

    /// <summary>Creature can fly (was ISFLY)</summary>
    IsFlying = 0x0040000,

    /// <summary>Creature has been slowed (was ISSLOW)</summary>
    IsSlow = 0x0100000,
}
