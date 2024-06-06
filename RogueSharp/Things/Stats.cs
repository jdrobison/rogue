namespace RogueSharp.Things;

/// <summary>
/// Properties of a fighting being (monster or person)
/// </summary>
internal class Stats
{
    /// <summary>Strength</summary>
    public uint s_str;

    /// <summary>Experience</summary>
    public int s_exp;

    /// <summary>level of mastery</summary>
    public int s_lvl;

    /// <summary>Armor class</summary>
    public int s_arm;

    /// <summary>Hit points</summary>
    public int s_hpt;

    /// <summary>String describing damage done</summary>
    required public string s_dmg;

    /// <summary>Max hit points</summary>
    public int s_maxhp;
}
