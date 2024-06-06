namespace RogueSharp.Enumerations;

/// <summary>
/// Referred to as "objects" in original
/// </summary>
[Flags]
internal enum ItemFlags
{
    /// <summary>Object is cursed (was ISCURSED)</summary>
    IsCursed = 0x01,

    /// <summary>Player knows details about the object (was ISKNOW)</summary>
    IsKnown = 0x02,

    /// <summary>Object is a missile type (was ISMISL)</summary>
    IsMissile = 0x04,

    /// <summary>Object comes in groups (was ISMANY)</summary>
    IsMany = 0x10,

    /// <summary>Armor is permanently protected (was ISPROT)</summary>
    IsProtected = 0x40,
}
