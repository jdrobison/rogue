namespace RogueSharp.Enumerations;

internal enum WeaponKind
{
    /// <summary>Mace (was MACE)</summary>
    Mace        = 0,

    /// <summary>Sword (was SWORD)</summary>
    Sword       = 1,

    /// <summary>Bow (was BOW)</summary>
    Bow     = 2,

    /// <summary>Arrow (was ARROW)</summary>
    Arrow       = 3,

    /// <summary>Dagger (was DAGGER)</summary>
    Dagger      = 4,

    /// <summary>Two-handed sword (was TWOSWORD)</summary>
    TwoSword    = 5,

    /// <summary>Dart (was DART)</summary>
    Dart        = 6,

    /// <summary>Shiraken (was SHIRAKEN)</summary>
    Shiraken    = 7,

    /// <summary>Spear (was SPEAR)</summary>
    Spear       = 8,

    /// <summary>Fake entry for dragon breath (ick) (was FLAME)</summary>
    Flame       = 9,

    /// <summary>The number of different kinds of weapons (not including <see cref="Flame"/>)</summary>
    Count = 9,
}
