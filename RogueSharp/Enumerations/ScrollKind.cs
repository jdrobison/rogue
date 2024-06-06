namespace RogueSharp.Enumerations;

internal enum ScrollKind
{
    S_CONFUSE   = 0,
    S_MAP       = 1,
    S_HOLD      = 2,
    S_SLEEP     = 3,
    S_ARMOR     = 4,
    S_ID_POTION = 5,
    S_ID_SCROLL = 6,
    S_ID_WEAPON = 7,
    S_ID_ARMOR  = 8,
    S_ID_R_OR_S = 9,
    S_SCARE     = 10,
    S_FDET      = 11,
    S_TELEP     = 12,
    S_ENCH      = 13,
    S_CREATE    = 14,
    S_REMOVE    = 15,
    S_AGGR      = 16,
    S_PROTECT   = 17,

    /// <summary>The number of different kinds of scrolls</summary>
    Count = 18,
}
