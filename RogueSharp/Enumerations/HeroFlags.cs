namespace RogueSharp.Enumerations;

[Flags]
internal enum HeroFlags
{
    /// <summary>Hero is levitating (was ISLEVIT)</summary>
    IsLevitating = 0x01,

    /// <summary>Hero is on acid tr (was ISHALU)ip</summary>
    IsHallucinating = 0x02,

    /// <summary>Hero can detect unseen monsters (was SEEMONST)</summary>
    SeeMonsters = 0x04,
}
