using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Xeroc : Monster
{
    public override char Appearance => 'X';

    public override string m_name => "xeroc";

    public override int m_carry => 30;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 100, level: 7, armor: 7, damage: "4x4");
}
