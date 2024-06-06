using RogueSharp.Enumerations;

namespace RogueSharp.Things;

internal abstract class Monster : Thing
{
    /// <summary>The monster's appearance on the map</summary>
    public abstract char Appearance { get; }

    /// <summary>What to call the monster</summary>
    public abstract string m_name { get; }

    /// <summary>Probability of carrying something</summary>
    public abstract int m_carry { get; }

    /// <summary>things about the monster/// </summary>
    public abstract MonsterFlags m_flags { get; set; }

    /// <summary>Initial stats</summary>
    public abstract Stats m_stats { get; }

    protected static Stats CreateStats(
        int experience,
        int level,
        int armor,
        string damage)
    {
        const int DefaultStrength = 10;
        const int DefaultHitPoint = 1;

        return new Stats()
        {
            s_str = DefaultStrength,
            s_exp = experience,
            s_lvl = level,
            s_arm = armor,
            s_hpt = DefaultHitPoint,
            s_dmg = damage
        };
    }
}
