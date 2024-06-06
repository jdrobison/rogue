using RogueSharp.Enumerations;

namespace RogueSharp.Things;

internal abstract class Item : Thing
{
    /// <summary>What it says if you read it</summary>
    public string? _o_text;          

    /// <summary>What you need to launch it</summary>
    public int _o_launch;         

    /// <summary>What character it is in the pack</summary>
    public char _o_packch;         

    /// <summary>Damage if used like sword</summary>
    required public string _o_damage;

    /// <summary>Damage if thrown</summary>
    required public string _o_hurldmg;

    /// <summary>count for plural objects</summary>
    public int _o_count;           

    /// <summary>Which object of a type it is</summary>
    public int _o_which;           

    /// <summary>Plusses to hit</summary>
    public int _o_hplus;           

    /// <summary>Plusses to damage</summary>
    public int _o_dplus;           

    /// <summary>Armor protection</summary>
    public int _o_arm;             

    /// <summary>information about items</summary>
    public ItemFlags _o_flags;           

    /// <summary>group number for this item</summary>
    public int _o_group;

    /// <summary>Label for item</summary>
    public string? _o_label;			
}

internal abstract class Item<TKind> : Item
{
    /// <summary>What kind of item is it?</summary>
    required public TKind Kind { get; init; }
}
