using RogueSharp.Enumerations;
using RogueSharp.Things;

namespace RogueSharp.Things.Monsters;

internal class Wraith : Monster
{
    public override char Appearance => 'W';

    public override string m_name => "wraith";

    public override int m_carry => 0;

    public override MonsterFlags m_flags { get; set; } = MonsterFlags.None;

    public override Stats m_stats { get; } = CreateStats(experience: 55, level: 5, armor: 4, damage: "1x6");
}
