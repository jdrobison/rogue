using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Orc : Monster
{
    public override char Appearance => 'O';

    public override string m_name => "orc";

    public override int m_carry => 15;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.IsGreedy;

    public override Stats m_stats { get; } = CreateStats(experience: 5, level: 1, armor: 6, damage: "1x8");
}
