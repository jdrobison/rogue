using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Flytrap : Monster
{
    public override char Appearance => 'F';

    public override string m_name => "venus flytrap";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsMean;

    /// <remarks>
    /// NOTE: the damage is %%% so that xstr won't merge this
    /// string with others, since it is written on in the program
    /// </remarks>
    public override Stats m_stats { get; } = CreateStats(experience: 80, level: 8, armor: 3, damage: "%%%x0");
}
